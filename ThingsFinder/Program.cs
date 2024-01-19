using Marten;
using Marten.Events.Projections;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using ThingsFinder;
using ThingsFinder.Models;
using Weasel.Core;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

const string serviceName = "things-finder";

// Build configuration
var configuration = builder.Configuration;

builder.Logging.AddConsole();

builder.Services.AddMarten(options =>
{
    options.Connection(configuration.GetConnectionString("PostgresConnection") ?? 
                       throw new InvalidOperationException("Connection string is not found"));
    options.AutoCreateSchemaObjects = AutoCreate.All;

    options.Projections.Snapshot<MyThing>(SnapshotLifecycle.Inline);
});

// Add OpenTelemetry Tracing
builder.Logging.AddOpenTelemetry(options =>
{
    options
        .SetResourceBuilder(
            ResourceBuilder.CreateDefault()
                .AddService(serviceName))
        .AddConsoleExporter();
});
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddConsoleExporter());

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("createMyThing", MyThingsMethods.CreateMyThingAsync)
    .WithName("CreateMyThing")
    .WithOpenApi();

app.Run();

public partial class Program;