using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.EntityFrameworkCore;
using OnlineDelivery.Api.Data;
using OnlineDelivery.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// PostgreSQL through EF Core; the connection string is a secret: ADR-0004.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<ReferenceDataService>();

// Allowed browser origins come from configuration, and an empty list allows none: ADR-0001.
const string AppCorsPolicy = "App";
builder.Services.AddCors();
builder.Services.AddOptions<CorsOptions>().Configure<IConfiguration>((options, configuration) =>
{
    var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
    options.AddPolicy(AppCorsPolicy, policy => policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Only Development migrates on start-up; elsewhere it is a deployment step: ADR-0004.
    await using (var scope = app.Services.CreateAsyncScope())
    {
        await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
    }

    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Development serves plain HTTP on localhost: ADR-0002.
    app.UseHttpsRedirection();
}

// CORS before authorization, so refused calls still carry CORS headers: ADR-0001.
app.UseCors(AppCorsPolicy);

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();

// Lets the in-memory test server start this API: ADR-0003.
public partial class Program;
