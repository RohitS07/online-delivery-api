using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using OnlineDelivery.Api.Data;
using OnlineDelivery.Api.Services;
using OnlineDelivery.Api.Services.Auth;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// The API reads the time only through this, so tests can set it: ADR-0007.
builder.Services.AddSingleton(TimeProvider.System);

// PostgreSQL through EF Core; the connection string is a secret: ADR-0004.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<ReferenceDataService>();

// Mobile OTP sign-in with a JWT bearer token: ADR-0006, SPEC-0003.
builder.Services.AddOptions<JwtOptions>().Bind(builder.Configuration.GetSection("Jwt")).ValidateDataAnnotations().ValidateOnStart();
builder.Services.AddSingleton<IOtpSender, MockOtpSender>();
builder.Services.AddSingleton<OtpCodeGenerator>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<OtpService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtOptions>, TimeProvider>((options, jwt, time) =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwt.Value.Issuer,
            ValidAudience = jwt.Value.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(jwt.Value.SigningKeyBytes),
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            ValidateIssuerSigningKey = true,
            RequireSignedTokens = true,
            NameClaimType = JwtRegisteredClaimNames.Sub,
            RoleClaimType = "role",
            // Token lifetimes follow the injected clock, like everything else: ADR-0007.
            LifetimeValidator = (notBefore, expires, _, _) =>
            {
                var now = time.GetUtcNow().UtcDateTime;
                return expires > now && (notBefore is null || notBefore <= now.AddMinutes(1));
            },
        };
        options.Events = new JwtBearerEvents
        {
            // A registration token only allows sign-up: SPEC-0003.
            OnTokenValidated = context =>
            {
                if (context.Principal?.HasClaim(TokenService.PurposeClaim, TokenService.RegistrationPurpose) == true)
                {
                    context.Fail("A registration token is not an access token.");
                }

                return Task.CompletedTask;
            },
        };
    });
builder.Services.AddAuthorization();

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

await AuthSeeder.SeedAsync(app.Services, app.Environment);

// CORS before authorization, so refused calls still carry CORS headers: ADR-0001.
app.UseCors(AppCorsPolicy);

app.UseAuthentication();
app.UseMiddleware<ActiveUserMiddleware>();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();

// Lets the in-memory test server start this API: ADR-0003.
public partial class Program;
