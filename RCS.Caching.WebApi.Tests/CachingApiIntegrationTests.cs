using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using RCS.Caching.Tests.Models;
using Xunit.Abstractions;

namespace RCS.Caching.WebApi.Tests;

public class CachingApiIntegrationTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Theory]
    [InlineData("memory")]
    [InlineData("sql")]
    [InlineData("redis")]
    public async Task Post_Then_Get_ShouldReturnExpectedDto(string store)
    {
        var key = $"test:{Guid.NewGuid()}";
        var dto = new SomeTestDto { Name = "integration", Count = 5 };

        var postResponse = await _client.PostAsJsonAsync($"/cache/{store}/{key}", dto);
        var postError = await postResponse.Content.ReadAsStringAsync();
        output.WriteLine($"POST {store} → {postResponse.StatusCode}\n{postError}");
        postResponse.EnsureSuccessStatusCode();

        var getResponse = await _client.GetAsync($"/cache/{store}/{key}");
        var getError = await getResponse.Content.ReadAsStringAsync();
        output.WriteLine($"GET {store} → {getResponse.StatusCode}\n{getError}");
        getResponse.EnsureSuccessStatusCode();

        var result = await getResponse.Content.ReadFromJsonAsync<SomeTestDto>();
        result.Should().NotBeNull();
        result.Name.Should().Be(dto.Name);
        result.Count.Should().Be(dto.Count);
    }
}