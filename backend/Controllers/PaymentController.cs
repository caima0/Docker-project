using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using api.Services;
using api.Models;
using api.Interfaces;
using api.Dtos.PayU;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using api.Data;
using Microsoft.Extensions.Logging;

namespace api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly PayUService _payUService;
        private readonly IRateRepository _rateRepository;
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDBContex _context;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            PayUService payUService, 
            IRateRepository rateRepository,
            UserManager<User> userManager,
            ApplicationDBContex context,
            ILogger<PaymentController> logger)
        {
            _payUService = payUService ?? throw new ArgumentNullException(nameof(payUService));
            _rateRepository = rateRepository ?? throw new ArgumentNullException(nameof(rateRepository));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreatePayment([FromBody] PaymentRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { error = "Request body cannot be null" });
                }

                if (request.Amount <= 0)
                {
                    return BadRequest(new { error = "Amount must be greater than zero" });
                }

                if (string.IsNullOrEmpty(request.Currency))
                {
                    return BadRequest(new { error = "Currency is required" });
                }

                var rate = await _rateRepository.GetByCodeAsync(request.Currency);
                
                if (rate == null)
                {
                    return BadRequest(new { error = $"Currency {request.Currency} not found in available rates" });
                }

                decimal amountInPLN = request.Amount * (decimal)rate.Ask;

                _logger.LogInformation($"Creating payment: {request.Amount} {request.Currency} (converted to {amountInPLN} PLN)");

                var paymentResponse = await _payUService.CreatePaymentAsync(
                    amountInPLN,
                    "PLN", 
                    $"Currency exchange: {request.Amount} {request.Currency} to PLN"
                );

                if (paymentResponse == null)
                {
                    _logger.LogWarning("Failed to create payment - no response from PayU");
                    return BadRequest(new { error = "Failed to create payment" });
                }

                if (string.IsNullOrEmpty(paymentResponse.RedirectUri))
                {
                    _logger.LogWarning("No redirect URL received from PayU");
                    return BadRequest(new { error = "No payment redirect URL received from PayU" });
                }

                var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(email))
                {
                    _logger.LogWarning("Email not found in claims");
                    return BadRequest(new { error = "Email not found in token" });
                }

                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    _logger.LogWarning($"User with email {email} not found");
                    return BadRequest(new { error = "User not found" });
                }

                _logger.LogInformation($"Creating pending payment for user {user.Id}: {request.Amount} {request.Currency}");

                var existingBalance = await _context.Balances
                    .FirstOrDefaultAsync(b => b.UserId == user.Id && b.Currency == request.Currency);

                decimal balanceAfterTransaction;
                if (existingBalance == null)
                {
                    balanceAfterTransaction = request.Amount;
                }
                else
                {
                    balanceAfterTransaction = existingBalance.Amount + request.Amount;
                }

                var strategy = _context.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                    using (var transaction = await _context.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            if (existingBalance == null)
                            {
                                var newBalance = new Balance
                                {
                                    UserId = user.Id,
                                    Currency = request.Currency,
                                    Amount = request.Amount,
                                    LastUpdated = DateTime.UtcNow
                                };
                                _context.Balances.Add(newBalance);
                                _logger.LogInformation($"Created new balance for user {user.Id} with currency {request.Currency}");
                            }
                            else
                            {
                                var newBalance = new Balance
                                {
                                    UserId = user.Id,
                                    Currency = request.Currency,
                                    Amount = balanceAfterTransaction,
                                    LastUpdated = DateTime.UtcNow
                                };
                                _context.Balances.Remove(existingBalance);
                                _context.Balances.Add(newBalance);
                                _logger.LogInformation($"Updated balance for user {user.Id} with currency {request.Currency}");
                            }

                            var paymentDetails = new BalanceHistory
                            {
                                UserId = user.Id,
                                Amount = request.Amount,
                                Currency = request.Currency,
                                TransactionType = "PENDING_PAYMENT",
                                Description = $"Pending payment for {request.Amount} {request.Currency}",
                                OrderId = paymentResponse.OrderId,
                                Timestamp = DateTime.UtcNow,
                                BalanceAfterTransaction = balanceAfterTransaction
                            };
                            _context.BalanceHistory.Add(paymentDetails);

                            await _context.SaveChangesAsync();
                            await transaction.CommitAsync();

                            _logger.LogInformation($"Created pending payment record with OrderId: {paymentResponse.OrderId}");
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();
                            _logger.LogError(ex, "Error creating payment and updating balance");
                            throw;
                        }
                    }
                });

                return Ok(new { 
                    redirectUrl = paymentResponse.RedirectUri,
                    orderId = paymentResponse.OrderId,
                    extOrderId = paymentResponse.ExtOrderId,
                    originalAmount = request.Amount,
                    originalCurrency = request.Currency,
                    convertedAmount = amountInPLN,
                    convertedCurrency = "PLN",
                    exchangeRate = rate.Ask,
                    status = paymentResponse.Status
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid argument in payment creation");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in payment creation");
                var detail = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { error = "Payment creation failed.", message = ex.Message, detail });
            }
        }

        [HttpGet("status/{orderId}")]
        public async Task<IActionResult> GetOrderStatus(string orderId)
        {
            try
            {
                if (string.IsNullOrEmpty(orderId))
                {
                    return BadRequest(new { error = "Order ID is required" });
                }

                _logger.LogInformation($"Checking status for order: {orderId}");
                var orderStatus = await _payUService.GetOrderStatusAsync(orderId);
                
                if (orderStatus == null)
                {
                    _logger.LogWarning($"Order {orderId} not found");
                    return NotFound(new { error = $"Order {orderId} not found" });
                }

                var order = orderStatus.Orders.FirstOrDefault();
                if (order == null)
                {
                    _logger.LogWarning("No orders found in the response");
                    return BadRequest(new { error = "No order details found" });
                }

                _logger.LogInformation($"Order status: {order.Status}, Description: {orderStatus.Status.StatusDesc}");

                if (order.Status == "COMPLETED")
                {
                    _logger.LogInformation("Payment completed, updating user balance");
                    var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                    if (string.IsNullOrEmpty(email))
                    {
                        _logger.LogWarning("Email not found in claims");
                        return BadRequest(new { error = "Email not found in token" });
                    }

                    var user = await _userManager.FindByEmailAsync(email);
                    if (user == null)
                    {
                        _logger.LogWarning($"User with email {email} not found");
                        return BadRequest(new { error = "User not found" });
                    }

                    var pendingPayment = await _context.BalanceHistory
                        .FirstOrDefaultAsync(bh => bh.UserId == user.Id && 
                                                 bh.OrderId == order.OrderId && 
                                                 bh.TransactionType == "PENDING_PAYMENT");

                    if (pendingPayment == null)
                    {
                        _logger.LogWarning($"No pending payment found for order {order.OrderId}");
                        return BadRequest(new { error = "No pending payment found" });
                    }

                    _logger.LogInformation($"Found pending payment: Amount={pendingPayment.Amount}, Currency={pendingPayment.Currency}");

                    var rate = await _rateRepository.GetByCodeAsync(pendingPayment.Currency!);

                    if (rate == null)
                    {
                        _logger.LogWarning($"Exchange rate not found for currency {pendingPayment.Currency}");
                        return BadRequest(new { error = $"Exchange rate not found for currency {pendingPayment.Currency}" });
                    }

                    decimal plnAmount = decimal.Parse(order.TotalAmount);
                    decimal targetCurrencyAmount = plnAmount / (decimal)rate.Ask;

                    _logger.LogInformation($"Converting {plnAmount} PLN to {targetCurrencyAmount} {pendingPayment.Currency} using rate {rate.Ask}");

                    var strategy = _context.Database.CreateExecutionStrategy();
                    await strategy.ExecuteAsync(async () =>
                    {
                        using (var transaction = await _context.Database.BeginTransactionAsync())
                        {
                            try
                            {
                                var balance = await _context.Balances
                                    .FirstOrDefaultAsync(b => b.UserId == user.Id && b.Currency == pendingPayment.Currency);

                                decimal newBalanceAmount;
                                if (balance == null)
                                {
                                    _logger.LogInformation($"Creating new balance for user {user.Id} with currency {pendingPayment.Currency}");
                                    newBalanceAmount = targetCurrencyAmount;
                                    balance = new Balance
                                    {
                                        UserId = user.Id,
                                        Currency = pendingPayment.Currency,
                                        Amount = newBalanceAmount,
                                        LastUpdated = DateTime.UtcNow
                                    };
                                    _context.Balances.Add(balance);
                                    _logger.LogInformation($"Added new balance to context. Entity state: {_context.Entry(balance).State}");
                                }
                                else
                                {
                                    _logger.LogInformation($"Updating existing balance for user {user.Id}. Current amount: {balance.Amount}, Adding: {targetCurrencyAmount}");
                                    newBalanceAmount = balance.Amount + targetCurrencyAmount;
                                    
                                    var newBalance = new Balance
                                    {
                                        UserId = user.Id,
                                        Currency = pendingPayment.Currency,
                                        Amount = newBalanceAmount,
                                        LastUpdated = DateTime.UtcNow
                                    };
                                    
                                    _context.Balances.Remove(balance);
                                    _context.Balances.Add(newBalance);
                                    balance = newBalance;
                                    
                                    _logger.LogInformation($"Replaced old balance with new balance. New amount: {newBalanceAmount}");
                                }

                                await _context.SaveChangesAsync();
                                _logger.LogInformation($"Saved balance changes. New balance amount: {balance.Amount}");

                                pendingPayment.TransactionType = "PAYMENT";
                                pendingPayment.Description = $"Payment completed: {plnAmount} PLN converted to {targetCurrencyAmount} {pendingPayment.Currency}";
                                pendingPayment.BalanceAfterTransaction = balance.Amount;
                                pendingPayment.Amount = targetCurrencyAmount;
                                
                                var balanceHistory = new BalanceHistory
                                {
                                    UserId = user.Id,
                                    Amount = targetCurrencyAmount,
                                    Currency = pendingPayment.Currency,
                                    TransactionType = "PAYMENT",
                                    Description = $"Payment completed: {plnAmount} PLN converted to {targetCurrencyAmount} {pendingPayment.Currency}",
                                    Timestamp = DateTime.UtcNow,
                                    BalanceAfterTransaction = balance.Amount
                                };
                                _context.BalanceHistory.Add(balanceHistory);
                                _logger.LogInformation($"Added balance history. Entity state: {_context.Entry(balanceHistory).State}");

                                await _context.SaveChangesAsync();
                                _logger.LogInformation("Saved all changes to database");
                                await transaction.CommitAsync();
                                _logger.LogInformation("Transaction committed successfully");

                                var updatedBalance = await _context.Balances
                                    .FirstOrDefaultAsync(b => b.UserId == user.Id && b.Currency == pendingPayment.Currency);
                                
                                if (updatedBalance != null)
                                {
                                    _logger.LogInformation($"Verified balance update - Current balance: {updatedBalance.Amount} {updatedBalance.Currency}");
                                }
                                else
                                {
                                    _logger.LogWarning("Balance verification failed - Could not find updated balance");
                                }
                            }
                            catch (Exception ex)
                            {
                                await transaction.RollbackAsync();
                                _logger.LogError(ex, "Error during transaction - rolling back changes");
                                throw;
                            }
                        }
                    });
                }
                else
                {
                    _logger.LogInformation($"Payment not completed. Status: {order.Status}");
                }

                return Ok(orderStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing order status");
                return StatusCode(500, new { error = "An unexpected error occurred while checking order status" });
            }
        }

        [HttpGet("balance")]
        public async Task<IActionResult> GetUserBalance()
        {
            try
            {
                var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(email))
                {
                    return BadRequest(new { error = "Email not found in token" });
                }

                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return BadRequest(new { error = "User not found" });
                }

                var balances = await _context.Balances
                    .Where(b => b.UserId == user.Id)
                    .Select(b => new { b.Currency, b.Amount, b.LastUpdated })
                    .ToListAsync();

                return Ok(balances);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user balance");
                return StatusCode(500, new { error = "An unexpected error occurred while fetching balance" });
            }
        }

        [HttpGet("balance/history")]
        public async Task<IActionResult> GetBalanceHistory([FromQuery] string? currency = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(email))
                {
                    return BadRequest(new { error = "Email not found in token" });
                }

                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return BadRequest(new { error = "User not found" });
                }

                var query = _context.BalanceHistory
                    .Where(bh => bh.UserId == user.Id);

                if (!string.IsNullOrEmpty(currency))
                {
                    query = query.Where(bh => bh.Currency == currency);
                }

                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                var history = await query
                    .OrderByDescending(bh => bh.Timestamp)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(bh => new
                    {
                        bh.Amount,
                        bh.Currency,
                        bh.TransactionType,
                        bh.Description,
                        bh.OrderId,
                        bh.Timestamp,
                        bh.BalanceAfterTransaction
                    })
                    .ToListAsync();

                return Ok(new
                {
                    Items = history,
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    CurrentPage = page,
                    PageSize = pageSize
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching balance history");
                return StatusCode(500, new { error = "An unexpected error occurred while fetching balance history" });
            }
        }
    }
} 