using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Testcontainers.PostgreSql;
using ThingsFinder;

namespace ThingFinder.Tests;

public class CustomWebAppFactory<TProgram>
    : WebApplicationFactory<TProgram> where TProgram : class
{
    private const string TestTracerName = "ThingsFinder.Tests";
    private readonly TracerProvider _tracerProvider;
    
    public Tracer TestTracer { get; }
    
    public CustomWebAppFactory(ICollection<Activity> spans)
    {
        _tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddSource(TestTracerName)
            .AddSource(ActivityHelper.Source.Name)
            .ConfigureResource(r => r.AddService(TestTracerName))
            .AddAspNetCoreInstrumentation()
            .AddInMemoryExporter(spans)
            .Build();
        
        TestTracer = _tracerProvider.GetTracer(TestTracerName);
    }
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
        
        builder.ConfigureLogging(l => l.ClearProviders());
        
        builder.ConfigureServices((_, sp) =>
        {
            sp.AddSingleton(_tracerProvider);
        });
    }
    
    public override ValueTask DisposeAsync()
    {
        _tracerProvider.ForceFlush();
        _tracerProvider.Dispose();
        return base.DisposeAsync();
    }
}