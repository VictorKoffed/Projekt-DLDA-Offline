using DLDA.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ========================
// 🛠️ Service Registration
// ========================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "DLDA API",
        Version = "v1"
    });
});

// ========================
// 🗄️ Database Context
// ========================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("LocalConnection")));

// ========================
// 🌐 CORS Policy
// ========================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowGUI", policy =>
    {
        policy.WithOrigins("https://informatik3.ei.hv.se")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// ========================
// 🚦 Middleware Pipeline
// ========================
app.UseHttpsRedirection();
app.UseCors("AllowGUI");
app.UseAuthorization();
app.MapControllers();

// ========================
// 📄 Swagger Configuration (Supports server hosting behind proxy subpaths)
// ========================
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    // NOTE: Absolute or properly structured endpoint configuration required for correct subpath proxy resolution
    c.SwaggerEndpoint("v1/swagger.json", "DLDA API v1");
    c.RoutePrefix = "swagger"; // Ensures /DLDA.API/swagger correctly exposes the interactive documentation UI
});

// ========================
// 🚀 Automatic Database Migration & Seeding
// ========================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        
        // 1. Applies pending database migrations automatically and provisions schema if non-existent on startup
        context.Database.Migrate();

        // 2. Hook point for external seed initialization if test accounts or baseline questions are required
        // Example: DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database schema.");
    }
}

app.Run();