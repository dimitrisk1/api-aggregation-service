// Core-Api/Program.cs
using Application.Interfaces;
using Application.Services;
using Core_Api.Extensions;
using Core_Infrastructure.BackgroundServices;
using Core_Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Auth
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("Your_Super_Secret_Key_For_Assignment_Make_It_Long!")),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

// 2. Add DI & Services (From Steps 1, 2, and 3)
builder.Services.AddSingleton<IApiMetricsService, ApiMetricsService>(); 
builder.Services.AddScoped<IAggregationService, AggregationService>();
builder.Services.AddSingleton<JwtIdentityService>();

// 3. Add External Typed Clients with Polly (From Step 1)
builder.Services.AddExternalProvidersWithResilience();

// 4. Add the Background Monitor (From Step 5)
builder.Services.AddHostedService<MetricsMonitorService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication(); // Must be before Authorization
app.UseAuthorization();

app.MapControllers(); // To protect your endpoints, just add [Authorize] to your AggregationController

app.Run();