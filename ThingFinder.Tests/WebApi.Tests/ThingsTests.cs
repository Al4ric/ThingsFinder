using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ThingsFinder.Requests;

namespace ThingFinder.Tests.WebApi.Tests;

public class ThingsTests : IClassFixture<CustomWebAppFactory<Program>>
{
    private readonly HttpClient _client;
    
    public ThingsTests(CustomWebAppFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }
    
    [Fact]
    public async Task CreateMyThingAsync_ShouldReturnOkAndProperResult()
    {
        // Arrange
        var name = "Test Thing";
        var description = "Test Description";
        var image = new byte[10];
        var tags = new List<string> {"Test Tag", "Test Tag 2"};
        var myThing = new CreateMyThingRequest(name, description, image, tags);
        
        // Act
        var response = await _client.PostAsJsonAsync("createMyThing", myThing);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}