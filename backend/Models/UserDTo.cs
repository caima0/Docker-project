using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class UserDTo
    {
        public required string Name {get; set;} 
        public required string Password{get; set;}
    }
}