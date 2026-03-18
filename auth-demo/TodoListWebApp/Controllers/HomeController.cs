using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;
using System.Net.Http.Headers;

namespace TodoListWebApp.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ITokenAcquisition _tokenAcquisition;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public HomeController(
        ITokenAcquisition tokenAcquisition,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _tokenAcquisition = tokenAcquisition;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<IActionResult> Index()
    {
        // Pull claims straight from HttpContext.User
        var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
        ViewBag.Claims = claims;
        ViewBag.UserName = User.Identity?.Name;
        ViewBag.ObjectId = User.GetObjectId();

        // Acquire a token scoped for TodoListApi
        var scopes = _configuration["TodoListApi:Scopes"]!.Split(' ');
        var accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(scopes);
        ViewBag.AccessToken = accessToken;

        // Call the API
        var client = _httpClientFactory.CreateClient("TodoListApi");
        client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.GetAsync("/api/todolist");

        if (response.IsSuccessStatusCode)
        {
            var todos = await response.Content.ReadAsStringAsync();
            ViewBag.TodoItems = todos;
        }
        else
        {
            ViewBag.TodoItems = $"API returned {response.StatusCode}";
        }

        return View();
    }

    [AllowAnonymous]
    public IActionResult Error()
    {
        return View();
    }
}