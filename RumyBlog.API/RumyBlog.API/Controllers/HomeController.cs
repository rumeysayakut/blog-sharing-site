using Microsoft.AspNetCore.Mvc;
using RumyBlog.API.DTOs;

using System.Text.Json;

public class HomeController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public HomeController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IActionResult> Index()
    {
        var client = _httpClientFactory.CreateClient();

        var response = await client.GetAsync("https://localhost:44375/api/posts");

        if (!response.IsSuccessStatusCode)
            return View(new List<PostListDto>());

        var json = await response.Content.ReadAsStringAsync();

        var posts = JsonSerializer.Deserialize<List<PostListDto>>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );

        return View(posts);
    }
}
