using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace api.Models
{
    public class User : IdentityUser
    {
      public ICollection<Balance> Balances { get; set; } = new List<Balance>();
public ICollection<BalanceHistory> BalanceHistory { get; set; } = new List<BalanceHistory>();


    }
}