using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Moodyali.API.Data;
using Moodyali.API.Endpoints;
using Moodyali.API.Services;
using Moodyali.Core.Services;
using Moodyali.Core.Entities;

var builder = WebApplication.CreateBuilder(args);

// --- Configuration ---
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret not configured.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];
var dbConnectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DefaultConnection not configured.");

// --- Services ---

// 1. Database Context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    // Use SQL Server for production/Azure, but allow SQLite for local development if connection string is for SQLite
    if (dbConnectionString.Contains("Data Source") && !dbConnectionString.Contains(".db"))
    {
        options.UseSqlServer(dbConnectionString);
    }
    else
    {
        // Fallback to SQLite for local testing if SQL Server is not configured or connection string is for SQLite
        options.UseSqlite("Data Source=moodyali.db");
    }
});

// 2. Authentication
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
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
    };
});

builder.Services.AddAuthorization();

// 3. Application Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMoodService, MoodService>();

// 4. CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            // Allow all origins for simplicity in MVP, but should be restricted in production
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// 5. Minimal API and Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- Middleware ---

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

// Serve static files for the frontend (wwwroot)
app.UseDefaultFiles();
app.UseStaticFiles();

// --- Endpoints ---
app.MapAuthEndpoints();
app.MapMoodEndpoints();

// Apply migrations on startup (for local testing with SQLite)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

app.Run();
