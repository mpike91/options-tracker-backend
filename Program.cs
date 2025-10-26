using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add Controllers (enables controller discovery)
builder.Services.AddControllers();

// Add DB (use a temp LocalDB for local testing; Azure later)
builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=AlyadTradingDb;Trusted_Connection=True;"));

// Add Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Add JWT auth (configure later)
builder.Services.AddAuthentication().AddJwtBearer();

// Add Authorization
builder.Services.AddAuthorization();

// Add CORS for frontend
builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Removed for local dev: app.UseHttpsRedirection();  // Re-enable in prod if using HTTPS
// app.UseHttpsRedirection();

// Map controllers (fixes 404 by registering attribute routes)
app.MapControllers();

app.Run();