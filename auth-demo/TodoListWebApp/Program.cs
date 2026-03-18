using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

var builder = WebApplication.CreateBuilder(args);

// Wire up authentication - reads from AzureAd section in appsettings.json
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
    .EnableTokenAcquisitionToCallDownstreamApi(
        builder.Configuration["TodoListApi:Scopes"]!.Split(' ') // request these scopes at login
    )
    .AddInMemoryTokenCaches();

// HTTP client for calling TodoListApi
builder.Services.AddHttpClient("TodoListApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["TodoListApi:BaseUrl"]!);
});

builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI(); // adds /MicrosoftIdentity/Account/SignIn and SignOut routes

builder.Services.AddRazorPages();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();