using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using lu.Context;
using lu.Helpers;
using lu.Interfaces;
using lu.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace lu.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private bool _isDisposed;
        private AuthContext _context;

        public AuthenticationService()
        {

        }

        public AuthenticationService(AuthContext context)
        {
            _context = context;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public void Dispose(bool dispose)
        {
            if (_isDisposed) return;

            if (dispose)
            {

            }

            _isDisposed = true;
        }

        public async Task<bool> CreateUserAsync(User user)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Email == user.Email);

            if (userExists) throw new Exception("User with this email already exists.");

            var newUser = user;
            newUser.Password = EncryptionHelper.Hash(user.Password);
            user = null;

            await _context.Users.AddAsync(newUser);
            var success = await _context.SaveChangesAsync() > 0;

            return success;
        }

        public async Task<Jwt> AuthenticateAsync(User user)
        {
            var storedUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == user.Email);

            var result = EncryptionHelper.ValidatePassword(storedUser.Password, user.Password);

            if (!result) throw new Exception("Could not login. Please check email and/or password");

            var token = CreateJwtToken(storedUser);

            return token;
        }

        public bool ValidateJwtToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = GetTokenValidationParameters();

            SecurityToken validatedToken;
            tokenHandler.ValidateToken(token, validationParameters, out validatedToken);

            return true;
        }

        private TokenValidationParameters GetTokenValidationParameters()
        {
            return new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidIssuer = Environment.GetEnvironmentVariable(Constants.JWT_ISSUER),
                ValidateAudience = true,
                ValidAudience = Environment.GetEnvironmentVariable(Constants.JWT_AUDIENCE),
                IssuerSigningKey = GetSymmetricSecurityKey(),
                ValidateLifetime = true
            };
        }

        private Jwt CreateJwtToken(User user)
        {
            var credentials = new SigningCredentials(GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha512Signature);

            var secToken = new JwtSecurityToken(
                issuer: Environment.GetEnvironmentVariable(Constants.JWT_ISSUER),
                audience: Environment.GetEnvironmentVariable(Constants.JWT_AUDIENCE),
                claims: new[]{
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
                    new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName)
                },
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credentials
            );

            var handler = new JwtSecurityTokenHandler();
            var token = handler.WriteToken(secToken);

            return new Jwt() { Token = token };
        }

        private SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            string key = Environment.GetEnvironmentVariable(Constants.JWT_KEY);
            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        }
    }
}