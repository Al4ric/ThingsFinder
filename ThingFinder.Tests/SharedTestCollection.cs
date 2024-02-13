namespace ThingFinder.Tests
{
    [CollectionDefinition(nameof(SharedTestCollection))]
    public class SharedTestCollection : ICollectionFixture<CustomWebAppFactory>
    {

    }
}
