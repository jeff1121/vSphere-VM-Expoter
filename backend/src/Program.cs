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

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors();
app.MapControllers();

app.Run();
