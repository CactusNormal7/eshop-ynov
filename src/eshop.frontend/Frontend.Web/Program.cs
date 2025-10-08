using Frontend.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configure HttpClient for Catalog API
var baseUrl = Environment.GetEnvironmentVariable("CATALOG_API_BASE_URL")
              ?? builder.Configuration["CatalogApi:BaseUrl"]
              ?? "http://localhost:5240"; // default fallback

builder.Services.AddHttpClient("CatalogApi", client =>
{
    client.BaseAddress = new Uri(baseUrl);
}).ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();
    if (builder.Environment.IsDevelopment())
    {
        // In development, ignore SSL certificate errors
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
    }
    return handler;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
