using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;

namespace api.Interfaces
{
    public interface IUserMockInterface
    {
        string? Authenticate(UserDTo user);
        bool Register(UserDTo user);
        UserDTo? GetCurrentUser();
    }
}