using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RumyBlog.API.DTOs;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using RumyBlog.API.DTOs;
using System.Net.Http.Headers;

public class PostsMvcController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public PostsMvcController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }



    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var token = Request.Cookies["access_token"];
        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login", "Account");

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await client.DeleteAsync(
            $"https://localhost:44375/api/posts/{id}"
        );

        return RedirectToAction("MyPosts");
    }


    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var token = Request.Cookies["access_token"];
        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login", "Account");

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var post = await client.GetFromJsonAsync<PostUpdateDto>(
            $"https://localhost:44375/api/posts/{id}");

        return View(post);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(PostUpdateDto dto)
    {
        var token = Request.Cookies["access_token"];
        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login", "Account");

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var formData = new MultipartFormDataContent();
        formData.Add(new StringContent(dto.Title), "Title");
        formData.Add(new StringContent(dto.Content), "Content");

        var response = await client.PutAsync(
            $"https://localhost:44375/api/posts/{dto.Id}",
            formData
        );

        if (!response.IsSuccessStatusCode)
            return View(dto);

        return RedirectToAction("MyPosts");
    }


    public async Task<IActionResult> MyPosts()
    {
        var token = Request.Cookies["access_token"];
        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login", "Account");

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync(
            "https://localhost:44375/api/posts/my"
        );

        if (!response.IsSuccessStatusCode)
            return RedirectToAction("Login", "Account");

        var json = await response.Content.ReadAsStringAsync();

        var posts = JsonSerializer.Deserialize<List<PostListDto>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return View(posts);
    }



    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }


    [HttpPost]
    public async Task<IActionResult> Create(CreatePostDto dto)
    {
        var token = Request.Cookies["access_token"];
        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login", "Account");

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var formData = new MultipartFormDataContent();
        formData.Add(new StringContent(dto.Title), "Title");
        formData.Add(new StringContent(dto.Content), "Content");

        if (dto.File != null && dto.File.Length > 0)
        {
            var fileContent = new StreamContent(dto.File.OpenReadStream());
            fileContent.Headers.ContentType =
                new MediaTypeHeaderValue(dto.File.ContentType);

            formData.Add(fileContent, "File", dto.File.FileName);
        }

        var response = await client.PostAsync(
            "https://localhost:44375/api/posts",
            formData
        );

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.Error = "Blog paylaşılırken hata oluştu";
            return View();
        }

        return RedirectToAction("MyPosts");
    }
}
