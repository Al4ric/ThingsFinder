﻿using Marten;
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
        [FromBody] CreateMyThingRequest request)
    {
        var newId = Guid.NewGuid();
        
        await using var session = store.LightweightSession();
        
        var newThingEvent = new CreateMyThingEvent(request.Name, request.Description, request.Image, request.Tags);
        
        session.Events.StartStream<MyThing>(newId, newThingEvent);
        try
        {
            await session.SaveChangesAsync();
            
        }
        catch (Exception ex) when (ex is ConcurrencyException or NpgsqlException)
        {
            using var errorActivity = ActivityHelper.Source.StartActivity(name: "Error_While_Creating_MyThing");
            errorActivity?.SetTag("error", ex.Message);
            
            throw;
        }

        return session.Events.AggregateStream<MyThing>(newId);
    }
}