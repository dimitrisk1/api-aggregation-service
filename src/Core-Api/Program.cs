using Application.Extensions;
using Application.Telemetry;
using Core_Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"] ?? "development-signing-key-should-be-overridden";
var openTelemetrySection = builder.Configuration.GetSection("OpenTelemetry");
var serviceName = openTelemetrySection["ServiceName"] ?? "core-api-aggregator";
var consoleExporterEnabled = openTelemetrySection.GetValue("ConsoleExporterEnabled", true);
var otlpExporterEnabled = openTelemetrySection.GetValue("OtlpExporterEnabled", false);
var otlpEndpoint = openTelemetrySection["OtlpEndpoint"];

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
var openTelemetryBuilder = builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName));

openTelemetryBuilder.WithTracing(tracing =>
{
    tracing
        .AddSource(AggregationTelemetry.ActivitySourceName)
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation();

    if (consoleExporterEnabled)
    {
        tracing.AddConsoleExporter();
    }

    if (otlpExporterEnabled && !string.IsNullOrWhiteSpace(otlpEndpoint))
    {
        tracing.AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(otlpEndpoint);
            options.Protocol = OtlpExportProtocol.HttpProtobuf;
        });
    }
});

openTelemetryBuilder.WithMetrics(metrics =>
{
    metrics
        .AddMeter(AggregationTelemetry.MeterName)
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation();

    if (consoleExporterEnabled)
    {
        metrics.AddConsoleExporter();
    }

    if (otlpExporterEnabled && !string.IsNullOrWhiteSpace(otlpEndpoint))
    {
        metrics.AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(otlpEndpoint);
            options.Protocol = OtlpExportProtocol.HttpProtobuf;
        });
    }
});

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API Aggregation Service",
        Version = "v1"
    });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = JwtBearerDefaults.AuthenticationScheme
        }
    };

    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, securityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [securityScheme] = Array.Empty<string>()
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"]
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
