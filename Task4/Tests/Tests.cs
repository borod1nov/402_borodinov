using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using FluentAssertions;
using Newtonsoft.Json;

namespace Api.IntegrationTests;

public class Tests
{
    [Fact]
    public async Task Correct_query_string_for_GET()
    {
        var client = new WebApplicationFactory<Program>().CreateClient();
        var response = await client.GetAsync("/initial?N=7&R=8&S=9");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Should().NotBeNull();
    }
    [Theory]
    [InlineData("")]
    [InlineData("/initial?")]
    [InlineData("/initial?N=7&S=9")]
    [InlineData("/initial?N=7&R=&S=9")]
    [InlineData("/abcdefghijklmnopqrstuvwxyz")]
    [InlineData("/next")]
    public async Task Incorrect_query_string_for_GET(string query_string)
    {
        var client = new WebApplicationFactory<Program>().CreateClient();
        var response = await client.GetAsync(query_string);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed);
    }
    [Theory]
    [InlineData(10, 6, 5)]
    [InlineData(7, 7, 7)]
    [InlineData(2, 1, 2)]
    [InlineData(8, 1, 12)]
    public async Task Content_check_for_GET(int N, int R, int S)
    {
        var client = new WebApplicationFactory<Program>().CreateClient();
        string query_string = "/initial?N=" + N.ToString() + "&R=" + R.ToString() + "&S=" + S.ToString();
        var response = await client.GetAsync(query_string);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Should().NotBeNull();

        var string_data = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<Request_type_1>(string_data);

        result.Should().NotBeNull();
        result.N.Should().Be(N);
        result.R.Should().Be(R);
        result.S.Should().Be(S);
        result.population.Should().HaveCount(10000);
        result.population[0].Should().HaveCount(R);
        result.population[0][0].Should().HaveCount(N);
    }
    [Fact]
    public async Task Correct_query_string_for_POST()
    {
        var client = new WebApplicationFactory<Program>().CreateClient();
        var get_response = await client.GetAsync("/initial?N=7&R=8&S=9");
        var string_data = await get_response.Content.ReadAsStringAsync();

        var post_response = await client.PostAsync("/next", new StringContent(string_data, System.Text.Encoding.UTF8, "application/json"));

        post_response.StatusCode.Should().Be(HttpStatusCode.OK);

    }
    [Theory]
    [InlineData("", true)]
    [InlineData("/nex", true)]
    [InlineData("/initial?N=7&R=8&S=9", true)]
    [InlineData("/previous", false)]
    [InlineData("/next", false)]
    public async Task Incorrect_query_for_POST(string query_string, bool correct_body)
    {
        var client = new WebApplicationFactory<Program>().CreateClient();
        var get_response = await client.GetAsync("/initial?N=7&R=8&S=9");
        var string_data = await get_response.Content.ReadAsStringAsync();
        if (correct_body)
        {
            var post_response = await client.PostAsync(query_string, new StringContent(string_data, System.Text.Encoding.UTF8, "application/json"));
            post_response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed);
        }
        else
        {
            var post_response = await client.PostAsync(query_string, new StringContent("abcdefg", System.Text.Encoding.UTF8, "application/json"));
            post_response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed);
        }
    }
    [Theory]
    [InlineData(3, 4, 5)]
    [InlineData(5, 3, 10)]
    [InlineData(2, 2, 2)]
    public async Task Content_check_for_POST(int N, int R, int S)
    {
        var client = new WebApplicationFactory<Program>().CreateClient();
        string query_string = "/initial?N=" + N.ToString() + "&R=" + R.ToString() + "&S=" + S.ToString();
        var get_response = await client.GetAsync(query_string);
        var get_string_data = await get_response.Content.ReadAsStringAsync();

        var post_response = await client.PostAsync("/next", new StringContent(get_string_data, System.Text.Encoding.UTF8, "application/json"));
        var post_string_data = await post_response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<Request_type_2>(post_string_data);

        result.Should().NotBeNull();
        result.N.Should().Be(N);
        result.R.Should().Be(R);
        result.S.Should().Be(S);
        result.best_score.Should().NotBe(0);
        result.best_schedule.Should().HaveCount(R);
        result.best_schedule[0].Should().HaveCount(N);
        result.population.Should().HaveCount(10000);
        result.population[0].Should().HaveCount(R);
        result.population[0][0].Should().HaveCount(N);
    }
}

