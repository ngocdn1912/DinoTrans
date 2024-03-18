using DinoTrans.IdentityManagerServerAPI.Services.Implements;
using DinoTrans.Shared.Data;
using DinoTrans.Shared.Entities;
using DinoTrans.Shared.Repositories.Implements;
using DinoTrans.Shared.Repositories.Interfaces;
using DinoTrans.Shared.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Text;
using DinoTrans.IdentityManagerServerAPI.SignalR;
using DinoTrans.IdentityManagerServerAPI.BackgroundWorker;
using DinoTrans.IdentityManagerServerAPI.ServiceFactory;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//start
builder.Services.AddDbContext<DinoTransDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                           ?? throw new InvalidOperationException("ConnectionString is not found");

    options.UseSqlServer(connectionString, b => b.MigrationsAssembly("DinoTrans.IdentityManagerServerAPI"));
});

//add Identity and JWT authentication
//Identity
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
    .AddEntityFrameworkStores<DinoTransDbContext>()
    .AddSignInManager()
    .AddRoles<ApplicationRole>();

//JWT 
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

//Add authentication to SwaggerUI
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    options.OperationFilter<SecurityRequirementsOperationFilter>();
});
//UnitOfWork
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

//Repositories
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();
builder.Services.AddScoped<ITenderRepository, TenderRepository>();
builder.Services.AddScoped<IConstructionMachineRepository, ConstructionMachineRepository>();
builder.Services.AddScoped<ITenderConstructionMachineRepository, TenderConstructionMachineRepository>();
builder.Services.AddScoped<ITenderBidRepository, TenderBidRepository>();


//Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITenderService, TenderService>();
builder.Services.AddScoped<IConstructionMachineService, ConstructionMachineService>();
builder.Services.AddScoped<ITenderBidService,  TenderBidService>();
builder.Services.AddSingleton<TenderServiceFactory>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();

builder.Services.AddHostedService<TenderBackgroundService>();
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors(policy =>
    {
    policy.WithOrigins("http://localhost:5230", "https://localhost:7111")
    .AllowAnyHeader()
    .AllowAnyMethod()
    .WithHeaders(HeaderNames.ContentType);

    });
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapHub<TenderOffersHub>("/tenderoffershub");

app.MapControllers();

var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();

app.Run();
