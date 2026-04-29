using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ProductManagement.API.Middleware;
using ProductManagement.API.Validators;
using ProductManagement.Repository;
using ProductManagement.Services;
using ProductManagement.Services.DTOs;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- Serilog (FIRST — before anything else) ---
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .WriteTo.Console(
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}",
        retainedFileCountLimit: 30)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// --- Authentication (JWT Bearer — placeholder, disabled by default via [AllowAnonymous]) ---
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "placeholder-secret-key-replace-in-production-min-32-chars!!";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "ProductManagement",
        ValidAudience = jwtSettings["Audience"] ?? "ProductManagement",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddAuthorization();

// --- Repository & Services Layers ---
builder.Services.AddRepositoryServices(builder.Configuration);
builder.Services.AddServicesLayer();

// --- Validators ---
builder.Services.AddScoped<IValidator<CreateProductDTO>, CreateProductDTOValidator>();
builder.Services.AddScoped<IValidator<UpdateProductDTO>, UpdateProductDTOValidator>();

// --- Controllers & Swagger ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Product Management API", Version = "v1" });
});

var app = builder.Build();

// --- Middleware Pipeline (ORDER MATTERS) ---
app.UseMiddleware<ExceptionHandlingMiddleware>();    // FIRST — wraps everything
app.UseMiddleware<RequestLoggingMiddleware>();       // SECOND — bookends the request

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }