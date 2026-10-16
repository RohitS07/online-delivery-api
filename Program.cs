using Microsoft.AspNetCore.Cors.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.Run();

// Lets the in-memory test server start this API: ADR-0003.
public partial class Program;
