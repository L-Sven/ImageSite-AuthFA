using System;
using lu.Context;
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
            var connectionString = Environment.GetEnvironmentVariable(Constants.DB_CONNECTIONSTRING);

            builder.Services.AddDbContext<AuthContext>(options => options.UseSqlServer(connectionString));
        }
    }
}