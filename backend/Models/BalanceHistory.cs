using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class BalanceHistory
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string? UserId { get; set; }
    
    [ForeignKey("UserId")]
    public User? User { get; set; }
    
    [Required]
    public decimal Amount { get; set; }
    
    [Required]
    public string? Currency { get; set; }
    
    [Required]
    public string? TransactionType { get; set; } // "DEPOSIT", "WITHDRAWAL", "PAYMENT"
    
    [Required]
    public string? Description { get; set; }
    
    public string? OrderId { get; set; }
    
    public DateTime Timestamp { get; set; }
    
    public decimal BalanceAfterTransaction { get; set; }
}

} 