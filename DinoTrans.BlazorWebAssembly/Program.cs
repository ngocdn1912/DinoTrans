using Blazored.LocalStorage;
using DinoTrans.BlazorWebAssembly;
using DinoTrans.BlazorWebAssembly.Authentication;
using DinoTrans.BlazorWebAssembly.Services.Implements;
using DinoTrans.Shared.Repositories.Implements;
using DinoTrans.Shared.Repositories.Interfaces;
using DinoTrans.Shared.Services.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Http;
using MudBlazor.Services;
using Radzen;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7017/") });

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();

builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddScoped<IUserService, UserClientService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<ITenderService, TenderClientService>();
builder.Services.AddScoped<IConstructionMachineService, ConstructionMachineClientService>();
builder.Services.AddScoped<IUnitOfWork,UnitOfWork>();
builder.Services.AddScoped<ITenderBidService, TenderBidClientService>();
builder.Services.AddScoped<IDashboardService, DashboardServiceClient>();
builder.Services.AddScoped<ICompanyService, CompanyClientService>();
builder.Services.AddScoped<IVnPayService, VnPayClientService>();


builder.Services.AddMudServices();
builder.Services.AddRadzenComponents();
builder.Services.AddBlazorBootstrap();

await builder.Build().RunAsync();

