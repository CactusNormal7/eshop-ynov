using Discount.API.Services;
using Discount.API.Validators;
using Discount.Grpc;
using FluentValidation;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddControllers();

// Configuration gRPC Client pour communiquer avec Discount.Grpc
var grpcUrl = configuration.GetValue<string>("GrpcSettings:DiscountUrl") ?? "http://localhost:5052";

builder.Services.AddGrpcClient<DiscountProtoService.DiscountProtoServiceClient>(options =>
{
    options.Address = new Uri(grpcUrl);
}).ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();
    
    if (builder.Environment.IsDevelopment())
    {
        handler.ServerCertificateCustomValidationCallback = 
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    }
    return handler;
});

// Register services
builder.Services.AddScoped<IDiscountService, DiscountService>();

// Register validators
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddValidatorsFromAssembly(typeof(CreateCouponDtoValidator).Assembly);

// Learn more about configuring OpenAPI/Swagger at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Discount API",
        Version = "v1",
        Description = "API REST pour la gestion des coupons de réduction et l'application des promotions.",
        Contact = new OpenApiContact
        {
            Name = "eShop Team",
            Email = "support@eshop.com"
        }
    });
    
    // Inclure les commentaires XML dans Swagger
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Health Checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Discount API V1");
        c.RoutePrefix = string.Empty; // Swagger UI à la racine
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Health check Endpoint
app.UseHealthChecks("/health", new HealthCheckOptions()
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();

