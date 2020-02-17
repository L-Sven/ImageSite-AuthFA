using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Security.Cryptography;
using lu.Models;
using lu.Context;
using Microsoft.EntityFrameworkCore;
using lu.Helpers;

namespace lu
{
    public class Authentication
    {
        private AuthContext _context;

        public Authentication(AuthContext context)
        {
            _context = context;
        }

        [FunctionName("authenticate")]
        public async Task<IActionResult> Authenticate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "Authenticate")] HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<User>(requestBody);

            if (data == null)
            {
                return new BadRequestObjectResult("No data was provided");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(user => user.Email == data.Email);

            var result = EncryptionHelper.ValidatePassword(user.Password, data.Password);
            user.Password = null;
            data = null;

            return result
                ? (ActionResult)new OkObjectResult(JsonConvert.SerializeObject(user))
                : new BadRequestObjectResult("Could not find user.");
        }

        [FunctionName("register")]
        public async Task<IActionResult> Register(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Register")] HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<User>(requestBody);

            var userExists = await _context.Users.FirstOrDefaultAsync(user => user.Email == data.Email);

            if (data == null
                || data.Email.Length < 1
                || data.Password.Length < 1)
            {
                return new BadRequestObjectResult("No data was provided");
            }
            else if (userExists != null)
            {
                return new BadRequestObjectResult("User already exists");
            }

            var newUser = data;
            newUser.Password = EncryptionHelper.Hash(data.Password);

            var result = await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();

            return new OkObjectResult("User created");
        }
    }
}
