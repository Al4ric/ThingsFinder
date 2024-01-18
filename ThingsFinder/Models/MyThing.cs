using Marten.Events;
using ThingsFinder.Events;

namespace ThingsFinder.Models;

public class MyThing
{
    public MyThing(string name, string description, byte[] image, List<string> tags)
    {
        Name = name;
        Description = description;
        Image = image;
        Tags = tags;
    }
    
    public static MyThing Create(IEvent<CreateMyThingEvent> @event)
    {
        var newThing = new MyThing(@event.Data.Name, @event.Data.Description, @event.Data.Image, @event.Data.Tags)
        {
            Id = @event.StreamId
        };

        return newThing;
    }

    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public byte[] Image { get; set; }
    public List<string> Tags { get; set; }

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