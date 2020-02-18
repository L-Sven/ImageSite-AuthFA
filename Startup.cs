using System;
using lu.Context;
using lu.Interfaces;
using lu.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(lu.Startup))]

namespace lu
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var connectionString = "Server= .\\SQLEXPRESS;Database=ImageProcess;Trusted_Connection=True;User Id=luFA01;Password=#SkM4rina0346;"; //Environment.GetEnvironmentVariable(Constants.DB_CONNECTIONSTRING);

            builder.Services.AddDbContext<AuthContext>(options => options.UseSqlServer(connectionString));

            builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
        }
    }
}