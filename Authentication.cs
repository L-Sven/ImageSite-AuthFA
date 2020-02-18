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
using lu.Interfaces;

namespace lu
{
    public class Authentication
    {
        private IAuthenticationService _authService;

        public Authentication(IAuthenticationService authService)
        {
            _authService = authService;
        }

        [FunctionName("authenticate")]
        public async Task<IActionResult> Authenticate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "authenticate")] HttpRequest req,
            ILogger log)
        {
            var data = await ReadRequestBodyAsync<User>(req);

            if (data == null
                || data.Email?.Length < 1
                || data.Password?.Length < 1)
            {
                return new BadRequestObjectResult("Neccessary logininformation was not provided");
            }

            Jwt token;
            try
            {
                token = await _authService.AuthenticateAsync(data);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }

            return token != null
                ? (ActionResult)new OkObjectResult(JsonConvert.SerializeObject(token))
                : new BadRequestObjectResult("Something went wrong during authentication. Please try again.");
        }

        [FunctionName("register")]
        public async Task<IActionResult> Register(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "register")] HttpRequest req,
            ILogger log)
        {
            var data = await ReadRequestBodyAsync<User>(req);

            if (data == null
                || data.Email.Length < 1
                || data.Password.Length < 1
                || data.FirstName.Length < 1
                || data.LastName.Length < 1)
            {
                return new BadRequestObjectResult("Neccessary information was not provided");
            }

            bool success;

            try
            {
                success = await _authService.CreateUserAsync(data);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }

            return success
                    ? (ActionResult)new OkObjectResult("User created")
                    : new BadRequestObjectResult("User could not be saved. Please try again");
        }

        [FunctionName("validate")]
        public async Task<IActionResult> Validate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "validate")] HttpRequest req,
            ILogger log)
        {
            var data = await ReadRequestBodyAsync<Jwt>(req);

            bool success;
            try
            {
                success = _authService.ValidateJwtToken(data.Token);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult("Error. Could not validate token");
            }

            return new OkObjectResult(success);
        }

        private async Task<T> ReadRequestBodyAsync<T>(HttpRequest req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            return JsonConvert.DeserializeObject<T>(requestBody);
        }
    }
}
