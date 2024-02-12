using Marten.Events;
using ThingsFinder.Events;

namespace ThingsFinder.Models;

public class MyThing(string name, string description, byte[] image, List<string> tags)
{
    public static MyThing Create(IEvent<CreateMyThingEvent> @event)
    {
        var newThing = new MyThing(@event.Data.Name, @event.Data.Description, @event.Data.Image, @event.Data.Tags)
        {
            Id = @event.StreamId
        };

        return newThing;
    }

    public Guid Id { get; set; }
    public string Name { get; set; } = name;
    public string Description { get; set; } = description;
    public byte[] Image { get; set; } = image;
    public List<string> Tags { get; set; } = tags;

    // Event sourcing
    public void Apply(CreateMyThingEvent @event)
    {
        Name = @event.Name;
        Description = @event.Description;
        Image = @event.Image;
        Tags = @event.Tags;
    }
    
    public void Apply(RenameMyThingEvent @event)
    {
        Name = @event.NewName;
    }
}