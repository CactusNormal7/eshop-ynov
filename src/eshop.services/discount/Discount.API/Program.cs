using Discount.API.Services;
using Discount.Grpc.Data;
using Discount.Grpc.Services;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Discount API",
        Version = "v1",
        Description = "API REST pour gérer les réductions et codes promo",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "eShop Team"
        }
    });
});

// Database
builder.Services.AddDbContext<DiscountContext>(options =>
    options.UseSqlite(configuration.GetConnectionString("DiscountConnection") ?? "Data Source=discountDatabase"));

// Services
builder.Services.AddScoped<AutomaticDiscountService>();
builder.Services.AddScoped<DiscountApplicationService>();

// Health checks
builder.Services.AddHealthChecks()
    .AddCheck("discount-api", () => HealthCheckResult.Healthy("Discount API is healthy"), 
        tags: new[] { "discount", "api" });

var app = builder.Build();

// Apply database migrations
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DiscountContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Discount API v1");
        c.RoutePrefix = "swagger"; // Swagger accessible via /swagger
    });
}

// Désactiver la redirection HTTPS en développement si nécessaire
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseAuthorization();
app.MapControllers();

// Health check endpoint
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();

