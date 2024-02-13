using System.Diagnostics;
using DotNet.Testcontainers.Builders;
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

public class CustomWebAppFactory
    : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string TestTracerName = "ThingsFinder.Tests";
    private readonly TracerProvider _tracerProvider;
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithDatabase("thingsDb")
        .WithUsername("thingsfinder")
        .WithPassword("temporaryPassword")
        .WithPortBinding(5432, true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
        .Build();
    
    public Tracer TestTracer { get; }

    public HttpClient HttpClient { get; private set; } = default!;

    public List<Activity> CollectedSpans { get; set; } = [];

    public CustomWebAppFactory()
    {
        _tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddSource(TestTracerName)
            .AddSource(ActivityHelper.Source.Name)
            .ConfigureResource(r => r.AddService(TestTracerName))
            .AddAspNetCoreInstrumentation()
            .AddInMemoryExporter(CollectedSpans)
            .Build();
        
        TestTracer = _tracerProvider.GetTracer(TestTracerName);
    }
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:PostgresConnection", _postgresContainer.GetConnectionString());
        
        builder.ConfigureLogging(l => l.ClearProviders());
        
        builder.ConfigureServices((_, sp) =>
        {
            sp.AddSingleton(_tracerProvider);
        });
    }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();
        HttpClient = CreateClient();
    }

    public new async Task DisposeAsync()
    {
        _tracerProvider.ForceFlush();
        _tracerProvider.Dispose();
        await _postgresContainer.StopAsync();
    }
}