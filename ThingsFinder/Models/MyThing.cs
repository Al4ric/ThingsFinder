namespace ThingsFinder.Models;

public class MyThing(string name, string description, byte[] image, List<string> tags)
{
    public string Name { get; set; } = name;
    public string Description { get; set; } = description;
    public byte[] Image { get; set; } = image;
    public List<string> Tags { get; set; } = tags;
}