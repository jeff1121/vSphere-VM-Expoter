using Backend.Services;
using Backend.Services.InMemory;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

builder.Services.AddSingleton<ISessionStore, InMemorySessionStore>();
builder.Services.AddScoped<IVSphereService, VSphereService>();
builder.Services.AddScoped<IMinioService, MinioService>();
builder.Services.AddSingleton<IExportTaskStore, InMemoryExportTaskStore>();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
        }
        else
        {
            // Development fallback: allow all origins only when no origins are configured
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        }
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors();
app.MapControllers();

app.Run();

// Expose Program as partial class for test host (WebApplicationFactory)
public partial class Program { }
