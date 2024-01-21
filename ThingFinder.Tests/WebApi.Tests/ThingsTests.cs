using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using ThingsFinder;
using ThingsFinder.Models;
using ThingsFinder.Requests;

namespace ThingFinder.Tests.WebApi.Tests;

public class ThingsTests : IClassFixture<CustomWebAppFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly IDocumentStore _store;
    
    public ThingsTests(CustomWebAppFactory<Program> factory)
    {
        _client = factory.CreateClient();
        var server = factory.Server;
        using var scope = server.Services.CreateScope();
        _store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
    }
    
    [Fact]
    public async Task CreateMyThingAsync_ShouldReturnOkAndProperResult()
    {
        // Arrange
        const string name = "Test Thing";
        const string description = "Test Description";
        var image = new byte[10];
        var tags = new List<string> {"Test Tag", "Test Tag 2"};
        var myThing = new CreateMyThingRequest(name, description, image, tags);
        
        // Act
        var response = await _client.PostAsJsonAsync("createMyThing", myThing);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<MyThing>();
        result.Should().NotBeNull();
        result!.Name.Should().Be(name);
        result.Description.Should().Be(description);
        result.Image.Should().BeEquivalentTo(image);
        result.Tags.Should().BeEquivalentTo(tags);
    }

    [Fact]
    public async Task CreateMyThingAsync_ShouldLogProperInformation()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<MyThing>>();
        var myThing = new CreateMyThingRequest("Test Thing", "Test Description", new byte[10], new List<string> {"Test Tag", "Test Tag 2"});


        // Act
        await MyThingsMethods.CreateMyThingAsync(_store, myThing, mockLogger.Object);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.StartsWith("New MyThing created with id")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!));
    }
}