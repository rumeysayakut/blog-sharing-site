using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using RumyBlog.API.DTOs.Auth;

public class AccountController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AccountController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var client = _httpClientFactory.CreateClient();

        var json = JsonSerializer.Serialize(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(
            "https://localhost:44375/api/auth/login",
            content
        );

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.Error = "Email veya şifre hatalı";
            return View();
        }

        var result = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(result);

        string token = null;

        if (doc.RootElement.TryGetProperty("accessToken", out var t1))
            token = t1.GetString();
        else if (doc.RootElement.TryGetProperty("token", out var t2))
            token = t2.GetString();

        if (string.IsNullOrEmpty(token))
        {
            ViewBag.Error = "Token alınamadı";
            return View();
        }

      
        Response.Cookies.Append("access_token", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None
        });

        return RedirectToAction("Index", "Home");

    }




    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var client = _httpClientFactory.CreateClient();

        var json = JsonSerializer.Serialize(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(
            "https://localhost:44375/api/auth/register",
            content
        );

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.Error = "Kayıt başarısız";
            return View();
        }

        return RedirectToAction("Login");
    }
}
