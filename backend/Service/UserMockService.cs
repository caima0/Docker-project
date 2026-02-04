using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using api.Interfaces;
using api.Models;
using Microsoft.IdentityModel.Tokens;

namespace api.Service
{
    public class UserMockService : IUserMockInterface
    {
        private readonly List<UserDTo> _users= new List<UserDTo>();

        private readonly IConfiguration _configuration;
        
        private readonly IHttpContextAccessor _contextAccessor;

        public UserMockService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _contextAccessor = httpContextAccessor;
        }

        public bool Register(UserDTo user)
        { 
            if(_users.Any(item=>item.Name==user.Name))
            
            return false;
            _users.Add(user);
            return true;
            
        }
    public string? Authenticate(UserDTo user)
    {
        var existingUser= _users.FirstOrDefault(u=>u.Name == user.Name && u.Password == user.Password);

        if(existingUser==null)
           return null;
        var tokenHandler= new JwtSecurityTokenHandler();

        var key = _configuration.GetRequiredSection("JwtSettings")["SecretKey"] ?? throw new ArgumentNullException();

        var tokenDescriptor = new SecurityTokenDescriptor
        {
             Subject = new System.Security.Claims.ClaimsIdentity([new Claim(ClaimTypes.Name, user.Name)]),
             Expires = DateTime.UtcNow.AddHours(1),
             SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256Signature)
        };
         var token = tokenHandler.CreateToken(tokenDescriptor);
         return tokenHandler.WriteToken(token);
        
    }

     public UserDTo? GetCurrentUser()
     {
        var uniqName = _contextAccessor.HttpContext?.User. FindFirst(ClaimTypes.Name)?.Value; 
        return _users.SingleOrDefault(item =>item.Name == uniqName);
     }
    }
}