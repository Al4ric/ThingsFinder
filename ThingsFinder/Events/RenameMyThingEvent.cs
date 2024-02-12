namespace ThingsFinder.Events;

public class RenameMyThingEvent(string newName)
{
    public string NewName { get; set; } = newName;
}