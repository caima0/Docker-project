using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.Account;
using api.Interfaces;
using api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static api.Interfaces.ITokenService;
using api.Data;

namespace api.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly ITokenService _tokenService;
        private readonly SignInManager<User> _signInManager;
        private readonly ApplicationDBContex _context;

        public AccountController(
            UserManager<User> userManager, 
            ITokenService tokenService, 
            SignInManager<User> signInManager,
            ApplicationDBContex context)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        [HttpGet]
        public async Task<IActionResult> GetAccounts()
        {
            try
            {
                var users = await _userManager.Users.ToListAsync();
                var userDtos = new List<NewUserDto>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    userDtos.Add(new NewUserDto
                    {
                        UserName = user.UserName ?? throw new InvalidOperationException("Username cannot be null"),
                        Email = user.Email ?? throw new InvalidOperationException("Email cannot be null"),
                        Token = await _tokenService.CreateToken(user),
                        Roles = roles.ToList()
                    });
                }

                return Ok(userDtos);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [HttpGet("{username}")]
        public async Task<IActionResult> GetAccount(string username)
        {
            try
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == username.ToLower());
                if (user == null) return NotFound("User not found");

                var roles = await _userManager.GetRolesAsync(user);

                return Ok(new NewUserDto
                {
                    UserName = user.UserName ?? throw new InvalidOperationException("Username cannot be null"),
                    Email = user.Email ?? throw new InvalidOperationException("Email cannot be null"),
                    Token = await _tokenService.CreateToken(user),
                    Roles = roles.ToList()
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [HttpPut("{email}")]
        public async Task<IActionResult> UpdateAccount(string email, [FromBody] UpdateAccountDto updateDto)
        {
            try
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Email == email.ToLower());
                if (user == null) return NotFound("User not found");

                if (!string.IsNullOrEmpty(updateDto.Email))
                    user.Email = updateDto.Email;

                if (!string.IsNullOrEmpty(updateDto.Username))
                    user.UserName = updateDto.Username;

                if (!string.IsNullOrEmpty(updateDto.NewPassword))
                {
                    var result = await _userManager.RemovePasswordAsync(user);
                    if (!result.Succeeded)
                        return BadRequest(result.Errors);

                    result = await _userManager.AddPasswordAsync(user, updateDto.NewPassword);
                    if (!result.Succeeded)
                        return BadRequest(result.Errors);
                }

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                    return BadRequest(updateResult.Errors);

                var roles = await _userManager.GetRolesAsync(user);

                return Ok(new NewUserDto
                {
                    UserName = user.UserName ?? throw new InvalidOperationException("Username cannot be null"),
                    Email = user.Email ?? throw new InvalidOperationException("Email cannot be null"),
                    Token = await _tokenService.CreateToken(user),
                    Roles = roles.ToList()
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [HttpDelete("{username}")]
        public async Task<IActionResult> DeleteAccount(string username)
        {
            try
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == username.ToLower());
                if (user == null) return NotFound("User not found");

                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                    return BadRequest(result.Errors);

                return Ok("Account deleted successfully");
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == loginDto.Username.ToLower());

            if (user == null) return Unauthorized("Invalid username");

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            if (!result.Succeeded) return Unauthorized("Username not found and/or password incorrect");

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(
                new NewUserDto
                {
                    UserName = user.UserName ?? throw new InvalidOperationException("Username cannot be null"),
                    Email = user.Email ?? throw new InvalidOperationException("Email cannot be null"),
                    Token = await _tokenService.CreateToken(user),
                    Roles = roles.ToList()
                }
            );
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                if (registerDto == null)
                    return BadRequest(new { message = "Registration data is required." });
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var appUser = new User
                {
                    UserName = registerDto.Username,
                    Email = registerDto.Email
                };

                var createUser = await _userManager.CreateAsync(appUser, registerDto.Password ?? string.Empty);

                if (createUser.Succeeded)
                {
                    var roleResult = await _userManager.AddToRoleAsync(appUser, "User");
                    if (roleResult.Succeeded)
                    {
                        var roles = await _userManager.GetRolesAsync(appUser);
                        return Ok(new NewUserDto
                        {
                            UserName = appUser.UserName ?? throw new InvalidOperationException("Username cannot be null"),
                            Email = appUser.Email ?? throw new InvalidOperationException("Email cannot be null"),
                            Token = await _tokenService.CreateToken(appUser),
                            Roles = roles.ToList()
                        });
                    }
                    return BadRequest(roleResult.Errors);
                }
                return BadRequest(createUser.Errors);
            }
            catch (DbUpdateException dbEx)
            {
                var inner = dbEx.InnerException?.Message ?? dbEx.Message;
                return StatusCode(500, new { message = "Database error while saving.", detail = inner });
            }
            catch (Exception ex)
            {
                var detail = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { message = ex.Message, detail });
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            try
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Email == forgotPasswordDto.Email.ToLower());
                if (user == null) return NotFound("User not found");

                var removeResult = await _userManager.RemovePasswordAsync(user);
                if (!removeResult.Succeeded)
                    return BadRequest(removeResult.Errors);

                var addResult = await _userManager.AddPasswordAsync(user, forgotPasswordDto.NewPassword);
                if (!addResult.Succeeded)
                    return BadRequest(addResult.Errors);

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                    return BadRequest(updateResult.Errors);

                return Ok(new { message = "Password has been reset successfully" });
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }
    }


}