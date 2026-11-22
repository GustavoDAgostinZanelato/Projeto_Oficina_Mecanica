using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using OrdemServicoApp.Data;
using OrdemServicoApp.Servicos;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddScoped<ServOrdemServico>();

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    });
});

builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient("Clientes", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Microservices:ClientesUrl"] ?? "http://localhost:5298");
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddHttpClient("Veiculos", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Microservices:VeiculosUrl"] ?? "http://localhost:5296");
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddHttpClient("Pecas", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Microservices:PecasUrl"] ?? "http://localhost:5297");
    client.Timeout = TimeSpan.FromSeconds(30);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();
    dataContext.Database.EnsureCreated();
}

var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
forwardedHeadersOptions.KnownIPNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeadersOptions);

app.MapOpenApi();
app.MapScalarApiReference();
app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.UseCors();
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
