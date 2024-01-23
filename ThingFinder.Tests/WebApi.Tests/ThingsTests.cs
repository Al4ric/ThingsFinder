using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Marten;
using Marten.Events;
using Moq;
using Npgsql;
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
        var mockStore = new Mock<IDocumentStore>();
        var mockSession = new Mock<IDocumentSession>();
        var mockEvents = new Mock<IEventStore>();
        var myThing = new CreateMyThingRequest("Test Thing", "Test Description", new byte[10],
            ["Test Tag", "Test Tag 2"]);

        mockStore.Setup(x => x.LightweightSession(System.Data.IsolationLevel.ReadCommitted)).Returns(mockSession.Object);
        mockSession.Setup(x => x.Events).Returns(mockEvents.Object);
        mockSession.Setup(x => x.SaveChangesAsync(default)).ThrowsAsync(new NpgsqlException("Connection refused"));
        
        await Assert.ThrowsAsync<NpgsqlException>(() =>
            MyThingsMethods.CreateMyThingAsync(mockStore.Object, myThing));
        
        var foundActivity = CollectedSpans.FirstOrDefault(
            a =>
                a.DisplayName.Contains("Error_While_Creating_MyThing"));
        Assert.NotNull(foundActivity);
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