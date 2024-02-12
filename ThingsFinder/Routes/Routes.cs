namespace ThingsFinder.Routes;

public static class MyRoutingExtensions
{
    public static void UseMyRouting(this WebApplication app)
    {
        app.MapPost("createMyThing", MyThingsMethods.CreateMyThingAsync)
            .WithName("CreateMyThing")
            .WithOpenApi();

        app.MapGet("getMyThing/{id}", MyThingsMethods.GetMyThingByIdAsync)
            .WithName("GetMyThingById")
            .WithOpenApi();
    }
}