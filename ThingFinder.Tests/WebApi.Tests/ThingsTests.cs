using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Marten;
using Marten.Events;
using Npgsql;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using ThingsFinder;
using ThingsFinder.Models;
using ThingsFinder.Requests;

namespace ThingFinder.Tests.WebApi.Tests;

public class ThingsTests : IAsyncLifetime
{
    private readonly HttpClient _client;
    private static readonly List<Activity> CollectedSpans = [];
    private readonly CustomWebAppFactory<Program> _webApp;

    public ThingsTests()
    {
        _webApp = new CustomWebAppFactory<Program>(CollectedSpans);
        _client = _webApp.CreateClient();
        var testSpan = _webApp.TestTracer.StartRootSpan("Test started");
        
        _client.DefaultRequestHeaders.Add("traceparent", 
            $"00-{testSpan.Context.TraceId.ToString()}-{testSpan.Context.SpanId.ToString()}-01");
    }
    
    [Fact]
    public async Task CreateMyThingAsync_ShouldReturnOkAndProperResult()
    {
        const string name = "Test Thing";
        const string description = "Test Description";
        var image = new byte[10];
        var tags = new List<string> {"Test Tag", "Test Tag 2"};
        var myThing = new CreateMyThingRequest(name, description, image, tags);
        
        var response = await _client.PostAsJsonAsync("createMyThing", myThing);
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<MyThing>();
        result.Should().NotBeNull();
        result!.Name.Should().Be(name);
        result.Description.Should().Be(description);
        result.Image.Should().BeEquivalentTo(image);
        result.Tags.Should().BeEquivalentTo(tags);
    }

    [Fact]
    public async Task CreateMyThingAsync_ShouldLogErrorWhenSaveChangesFails()
    {
        var mockStore = Substitute.For<IDocumentStore>();
        var mockSession = Substitute.For<IDocumentSession>();
        var mockEvents = Substitute.For<IEventStore>();
        var myThing = new CreateMyThingRequest("Test Thing", "Test Description", new byte[10],
            ["Test Tag", "Test Tag 2"]);

        mockStore.LightweightSession().Returns(mockSession);
        mockSession.Events.Returns(mockEvents);
        mockSession.SaveChangesAsync().ThrowsForAnyArgs(new NpgsqlException("Connection refused"));

        await Assert.ThrowsAsync<NpgsqlException>(() =>
            MyThingsMethods.CreateMyThingAsync(mockStore, myThing));

        var foundActivity = CollectedSpans.FirstOrDefault(
            a =>
                a.DisplayName.Contains("Error_While_Creating_MyThing"));
        Assert.NotNull(foundActivity);
    }

    [Fact]
    public async Task GetMyThingByIdAsync_ShouldReturnOkAndProperResult()
    {
        // Arrange
        var myThingRequest =
            new CreateMyThingRequest("name", "description", [], ["tag1", "tag2"]);
        var myThingResponse = await _client.PostAsJsonAsync("createMyThing", myThingRequest);
        var myThing = await myThingResponse.Content.ReadFromJsonAsync<MyThing>();

        // Act
        var response = await _client.GetAsync($"getMyThing/{myThing?.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<MyThing>();
        result.Should().NotBeNull();
        result!.Id.ToString().Should().Be(myThing?.Id.ToString());
        result.Name.Should().Be(myThing?.Name);
        result.Description.Should().Be(myThing?.Description); // Add null check
        result.Image.Should().BeEquivalentTo(myThing?.Image); // Add null check
        result.Tags.Should().BeEquivalentTo(myThing?.Tags); // Add null check
    }

    public Task InitializeAsync()
    {
        CollectedSpans.RemoveAll(_ => true);
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _webApp.DisposeAsync();
    }
}