using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Account
{
    public class UpdateAccountDto
    {
        [EmailAddress]
        public string? Email { get; set; }
        
        public string? Username { get; set; }
        
        [MinLength(6)]
        public string? NewPassword { get; set; }
    }
} 