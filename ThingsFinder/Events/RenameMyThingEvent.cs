namespace ThingsFinder.Events;

public class RenameMyThingEvent(string oldName, string newName)
{
    public string OldName { get; set; } = oldName;
    public string NewName { get; set; } = newName;
}