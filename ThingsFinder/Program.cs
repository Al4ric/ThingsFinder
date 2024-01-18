using Marten;
using Marten.Events.Projections;
using ThingsFinder;
using ThingsFinder.Models;
using Weasel.Core;

var builder = WebApplication.CreateBuilder(args);

// Build configuration
var configuration = builder.Configuration;

builder.Services.AddMarten(options =>
{
    options.Connection(configuration.GetConnectionString("PostgresConnection") ?? 
                       throw new InvalidOperationException("Connection string is not found"));
    options.AutoCreateSchemaObjects = AutoCreate.All;

    options.Projections.Snapshot<MyThing>(SnapshotLifecycle.Inline);
});

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