using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class Balance
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

    public DateTime LastUpdated { get; set; }
}
} 