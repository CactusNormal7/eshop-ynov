using BuildingBlocks.Messaging.MassTransit;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

builder.Services.AddMessageBroker(configuration, Assembly.GetExecutingAssembly());

var app = builder.Build();

app.Run();