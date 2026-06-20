using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Pos.Sales.Api.Data;
using Pos.Sales.Api.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("SalesDb")
    ?? "Host=localhost;Port=5432;Database=salesdb;Username=pos;Password=pos";

builder.Services.AddDbContext<SalesDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql => npgsql.EnableRetryOnFailure()));

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
    c.SwaggerDoc("v1", new() { Title = "POS Sales API", Version = "v1" }));

builder.Services.AddHealthChecks()
    .AddDbContextCheck<SalesDbContext>("sales-db", tags: ["ready"]);

// Typed client for the Catalog service. Base URL is injected via configuration so the
// same image runs unchanged under docker-compose and Kubernetes.
var catalogBaseUrl = builder.Configuration["Services:CatalogBaseUrl"] ?? "http://localhost:5001";
builder.Services.AddHttpClient<ICatalogClient, CatalogClient>(client =>
{
    client.BaseAddress = new Uri(catalogBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(10);
});

const string FrontendCors = "frontend";
builder.Services.AddCors(options =>
    options.AddPolicy(FrontendCors, policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

await DatabaseStartup.MigrateAsync(app);

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "POS Sales API v1"));

app.UseCors(FrontendCors);

app.MapControllers();

app.MapHealthChecks("/health/live", new() { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new() { Predicate = check => check.Tags.Contains("ready") });

app.Run();

public partial class Program;
