using Microsoft.EntityFrameworkCore;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using EcommerceDemoV1.Application;
using EcommerceDemoV1.Infrastructure;
using EcommerceDemoV1.Api.Extensions;
using Microsoft.OpenApi.Models;
using DotNetEnv;
using EcommerceDemoV1.Application.Common;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;


Env.Load();
var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddAutoMapper(typeof(AutoMapperConfig).Assembly);

// Add layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Đăng ký Background Job dọn dẹp đơn hàng
builder.Services.AddHostedService<EcommerceDemoV1.Infrastructure.BackgroundJobs.OrderCleanupService>();
builder.Services.Configure<AhamoveSettings>(builder.Configuration.GetSection("Ahamove"));
builder.Services.AddMemoryCache();

// JWT
var jwtKey = builder.Configuration["Jwt:Key"]
        ?? Environment.GetEnvironmentVariable("JWT_KEY");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var key = Encoding.UTF8.GetBytes(jwtKey!);

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Cookies.ContainsKey("AccessToken"))
            {
                context.Token = context.Request.Cookies["AccessToken"];
            }
            return Task.CompletedTask;
        },

        OnTokenValidated = context =>
        {
            var expClaim = context.Principal?.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp)?.Value;

            if (long.TryParse(expClaim, out var exp))
            {
                var expirationTime = DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;
                var timeRemaining = expirationTime - DateTime.UtcNow;

                // Nếu Token chỉ còn sống DƯỚI 5 PHÚT -> Tự động cấp mới
                if (timeRemaining < TimeSpan.FromMinutes(1) && timeRemaining > TimeSpan.Zero)
                {
                    var jwtService = context.HttpContext.RequestServices.GetRequiredService<IJwtService>();

                    // Trích xuất lại thông tin User từ Token cũ
                    var userIdStr = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                    var email = context.Principal?.FindFirstValue(ClaimTypes.Email) ?? "";
                    var role = context.Principal?.FindFirstValue(ClaimTypes.Role) ?? "";

                    if (int.TryParse(userIdStr, out var userId))
                    {
                        //ạo Access Token mới tinh (lại có tuổi thọ 15 phút)
                        var newAccessToken = jwtService.GenerateToken(userId, email, role);

                        //Ghi đè Cookie mới vào Response để gửi về cho Frontend
                        var cookieOptions = new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Strict,
                            Expires = DateTime.UtcNow.AddMinutes(15)
                        };
                        context.HttpContext.Response.Cookies.Append("AccessToken", newAccessToken, cookieOptions);
                    }
                }
            }
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                success = false,
                message = "Bạn chưa đăng nhập hoặc phiên đăng nhập đã hết hạn."
            });

            return context.Response.WriteAsync(result);
        },
        OnForbidden = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";

            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                success = false,
                message = "Bạn không có quyền truy cập vào chức năng này."
            });

            return context.Response.WriteAsync(result);
        }
    };
});


builder.Services.AddAuthorization(option =>
{
    option.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    option.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Affiliate API",
        Version = "v1",
        Description = "API documentation for Affiliate.Api"
    });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập token JWT vào đây. Ví dụ: Bearer {token}"
    };

    c.AddSecurityDefinition("Bearer", securityScheme);

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Middleware
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapEndpoints();

app.Run();