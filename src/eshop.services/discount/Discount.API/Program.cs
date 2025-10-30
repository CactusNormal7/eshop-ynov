using Discount.Grpc;
using Grpc.Net.Client;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// gRPC client configuration
var grpcAddress = configuration.GetValue<string>("GrpcSettings:DiscountUrl") ?? "http://localhost:5052";
builder.Services.AddSingleton(_ =>
{
    var channel = GrpcChannel.ForAddress(grpcAddress);
    return new DiscountProtoService.DiscountProtoServiceClient(channel);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.MapControllers();

app.Run();


