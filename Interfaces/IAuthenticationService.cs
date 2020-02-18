using System;
using System.Threading.Tasks;
using lu.Models;

namespace lu.Interfaces
{
    public interface IAuthenticationService : IDisposable
    {
        Task<bool> CreateUserAsync(User user);
        Task<Jwt> AuthenticateAsync(User user);
        bool ValidateJwtToken(string token);
    }
}
