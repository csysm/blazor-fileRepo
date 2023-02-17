using Blazored.LocalStorage;
using FileRepoSys.Web;
using FileRepoSys.Web.ServiceExtension;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://43.140.215.157/api/") });
//http://43.140.215.157/api/
builder.Services.AddAuth();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAntDesign();
await builder.Build().RunAsync();
