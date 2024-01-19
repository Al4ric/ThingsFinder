using Marten;
using Marten.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Trace;
using ThingsFinder.Events;
using ThingsFinder.Models;
using ThingsFinder.Requests;

namespace ThingsFinder;

public static class MyThingsMethods
{
    public static async Task<MyThing?> CreateMyThingAsync([FromServices] IDocumentStore store, [FromBody] CreateMyThingRequest request)
    {
        var newId = Guid.NewGuid();
        
        await using var session = store.LightweightSession();
        
        var newThingEvent = new CreateMyThingEvent(request.Name, request.Description, request.Image, request.Tags);
        
        session.Events.StartStream<MyThing>(newId, newThingEvent);
        try
        {
            await session.SaveChangesAsync();
        }
        catch (Exception ex) when (ex is ConcurrencyException || ex is NpgsqlException)
        {
            // Create tracer for logging
            var tracerProvider = Sdk.CreateTracerProviderBuilder()
                .AddConsoleExporter()
                .Build();

            // Get tracer from tracer provider
            var tracer = tracerProvider.GetTracer("things-finder");

            // Start a new span for logging
            using var scope = tracer.StartActiveSpan("SaveChangesError");
            // Log the exception
            scope.AddEvent($"Exception caught: {ex.Message}");

            // Re-throw the exception to avoid hiding it
            throw;
        }

        return session.Events.AggregateStream<MyThing>(newId);
    }
}