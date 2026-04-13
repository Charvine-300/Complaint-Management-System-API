using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Scalar.AspNetCore;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using ZenlyAPI.Domain.Config;
using ZenlyAPI.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
ZenlyConfig zenlyConfig = builder.Services.BindConfiguration(builder.Configuration);
builder.ConfigureSerilog(zenlyConfig.SerilogConfig);

builder.Services.RegisterDbContext(zenlyConfig.ConnectionString);
builder.Services.RegisterServices();

builder.Services.RegisterAuthentication();
builder.Services.AddControllers(x =>
{
    x.EnableEndpointRouting = false;
}).AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("fixed-by-ip", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString(),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1)
            }));
});

builder.Services.ConfigureSwagger(Assembly.GetExecutingAssembly().GetName().Name ?? "Zenly", builder.Environment.EnvironmentName);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapOpenApi();
app.MapScalarApiReference(options => options
   .WithTitle($"Zenly - {builder.Environment.EnvironmentName}")
    .WithTheme(ScalarTheme.BluePlanet)
    .WithDarkModeToggle(true)
    .WithDefaultHttpClient(ScalarTarget.JavaScript, ScalarClient.Fetch));


app.UseSwagger();
app.UseSwaggerUI(options =>
{
});

app.UseRateLimiter();
RouteGroupBuilder rateLimitedGroup = app.MapGroup("/api").RequireRateLimiting("fixed-by-ip");

app.UseHttpsRedirection();
app.UseCors(options =>
{
    options.WithOrigins(
        "http://localhost:3000",
        "http://localhost:4200");
    options.AllowAnyHeader();
    options.AllowAnyMethod();
    options.AllowCredentials();
});

app.UseAuthorization();

app.MapControllers();

app.Run();

