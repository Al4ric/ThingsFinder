using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace ThingFinder.Tests;

public class CustomWebAppFactory<TProgram>
    : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var postgresContainer = new PostgreSqlBuilder()
            .WithDatabase("thingsDb")
            .WithUsername("thingsfinder")
            .WithPassword("temporaryPassword")
            .WithPortBinding(5432, true)
            .Build();

        postgresContainer.StartAsync().GetAwaiter().GetResult();
        builder.UseSetting("ConnectionStrings:PostgresConnection", postgresContainer.GetConnectionString());
    }
}