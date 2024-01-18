using Marten;
using Microsoft.AspNetCore.Mvc;
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
        await session.SaveChangesAsync();
        
        return session.Events.AggregateStream<MyThing>(newId);
    }
}