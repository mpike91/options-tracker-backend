using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
// Add this for Swagger UI (install via NuGet: dotnet add package Swashbuckle.AspNetCore)
using Microsoft.OpenApi.Models;
using ProverbsTrading.Services.Background;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

// Add Controllers (enables controller discovery)
builder.Services.AddControllers();

// Add DB (use a temp LocalDB for local testing; Azure later)
builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=ProverbsTradingDb;Trusted_Connection=True;"));

// Add Services
builder.Services.AddScoped<TradierService>();
builder.Services.AddScoped<CboeService>();
builder.Services.AddHttpClient<CboeService>();

// Add Background Services
builder.Services.AddHostedService<WeeklyCboeFetcher>();

// Add Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Add JWT auth (configure later, e.g., with options for key/issuer)
builder.Services.AddAuthentication().AddJwtBearer();

// Add Authorization
builder.Services.AddAuthorization();

// Add CORS for frontend
builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// Suggestion: Add Swagger for API testing (free, enhances dev experience)
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Proverbs Trading API", Version = "v1" });
});

var app = builder.Build();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    // Suggestion: Enable Swagger UI in dev for browsing/testing endpoints
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/openapi/v1.json", "Proverbs Trading API v1"));
}

// Removed for local dev: app.UseHttpsRedirection();  // Re-enable in prod if using HTTPS
// app.UseHttpsRedirection();

// Map controllers (fixes 404 by registering attribute routes)
app.MapControllers();

app.Run();