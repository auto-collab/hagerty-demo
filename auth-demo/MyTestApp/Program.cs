using Microsoft.Identity.Client;
using System;
using System.Net.Http;
using System.Net.Http.Headers;

HttpClient client = new HttpClient();

var clientId = "782cac0f-a7d1-47a1-9c35-14f00fe34ffa";
var clientSecret = "";
var scopes = new[] {"api://b272ee7f-ee0c-4440-ba61-0913ab0225db/.default"};
var tenantId = "7cbc9feb-1d19-4186-a12a-b8d0ebc2bb1e";     //Use in workforce tenant configuration
var authority = $"https://login.microsoftonline.com/{tenantId}"; // Use "https://{tenantName}.ciamlogin.com" for external tenant configuration 

var app = ConfidentialClientApplicationBuilder
    .Create(clientId)
    .WithAuthority(authority)
    .WithClientSecret(clientSecret)
    .Build();

var result = await app.AcquireTokenForClient(scopes).ExecuteAsync();
Console.WriteLine($"Access Token: {result.AccessToken}");

client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
var response = await client.GetAsync("http://localhost:5285/api/todolist");
var content = await response.Content.ReadAsStringAsync();

Console.WriteLine("Your response is: " + response.StatusCode);
Console.WriteLine(content);