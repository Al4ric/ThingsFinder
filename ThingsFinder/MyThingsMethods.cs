using Marten;
using Marten.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using ThingsFinder.Events;
using ThingsFinder.Models;
using ThingsFinder.Requests;

namespace ThingsFinder;

public static class MyThingsMethods
{
    public static async Task<MyThing?> CreateMyThingAsync(
        [FromServices] IDocumentStore store, 
        [FromBody] CreateMyThingRequest request, 
        [FromServices] ILogger<MyThing> logger)
    {
        var newId = Guid.NewGuid();
        
        await using var session = store.LightweightSession();
        
        var newThingEvent = new CreateMyThingEvent(request.Name, request.Description, request.Image, request.Tags);
        
        session.Events.StartStream<MyThing>(newId, newThingEvent);
        try
        {
            await session.SaveChangesAsync();
            logger.LogInformation("New MyThing created with id {Id}", newId);
        }
        catch (Exception ex) when (ex is ConcurrencyException or NpgsqlException)
        {
            // Log the exception
            logger.LogError(ex, "Exception caught while saving changes");

            // Re-throw the exception to avoid hiding it
            throw;
        }

        return session.Events.AggregateStream<MyThing>(newId);
    }
}