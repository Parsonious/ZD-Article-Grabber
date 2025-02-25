using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text;
using System.Text.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using ZD_Article_Grabber.Interfaces;
using ZD_Article_Grabber.Helpers;
using ZD_Article_Grabber.Services;
using ZD_Article_Grabber.Builders;
using ZD_Article_Grabber.Types;

var builder = WebApplication.CreateBuilder(args);

ConfigurationManager configuration = builder.Configuration;

IWebHostEnvironment environment = builder.Environment;

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                      .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true, reloadOnChange: true);



if ( environment.IsDevelopment() )
{
    builder.Configuration.AddUserSecrets<Program>();
}
else
{
    builder.Configuration.AddEnvironmentVariables();
}
//validate JWT configuration
var jwtConfigSection = configuration.GetSection("Jwt");
var jwtConfig = jwtConfigSection.Get<ZD_Article_Grabber.Config.JwtConfig>();

if ( jwtConfig == null ||
    string.IsNullOrEmpty(jwtConfig.TokenKey) ||
    string.IsNullOrEmpty(jwtConfig.ApiKey) ||
    string.IsNullOrEmpty(jwtConfig.Issuer) )
{
    throw new InvalidOperationException("JWT configuration is missing or incomplete.");
}

#if DEBUG
//Defaults - left for debug
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    // For API Key (used by /a/gt/get-token)
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "API Key Authentication",
        Name = "bak", // Header name for API key
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });

    // For JWT (used by /a/p)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    // Apply security requirements globally
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey" // For API key-protected endpoints
                }
            },
            Array.Empty<string>()
        },
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer" // For JWT-protected endpoints
                }
            },
            Array.Empty<string>()
        }
    });
});
#endif

//Custom
builder.Services.AddHealthChecks()
    .AddCheck<KeyManagementHealthCheck>("KeyManagement");
builder.Services.AddMemoryCache();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        string publicKeyPath = Path.Combine(configuration["KeyManagement:KeyFolder"] ?? "Keys", "current.pub.pem");
        string pubKey = File.ReadAllText(publicKeyPath);

        ECDsa eCDsa = ECDsa.Create();
        eCDsa.ImportFromPem(pubKey);

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new ECDsaSecurityKey(eCDsa),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero, //strict expiration validation
        };
    });
builder.Services.AddAuthorization();


//Config
builder.Services.Configure<ZD_Article_Grabber.Config.ConfigOptions>(builder.Configuration);

//HTTP
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

//Hosted
builder.Services.AddHostedService<KeyRotationService>();


//Singeltons
builder.Services.AddSingleton<IConfigOptions>(sp => sp.GetRequiredService<IOptions<ZD_Article_Grabber.Config.ConfigOptions>>().Value);
builder.Services.AddSingleton<IResourceFetcher, ResourceFetcher>();
builder.Services.AddSingleton<ICacheHelper, CacheHelper>();
builder.Services.AddSingleton<Dependencies>();
builder.Services.AddSingleton<IKeyHistoryService, KeyHistoryService>();

//Transients
builder.Services.AddTransient<IContentFetcher, ContentFetcher>();
builder.Services.AddTransient<IPathHelper, PathHelper>();
builder.Services.AddTransient<INodeBuilder, NodeBuilder>();
builder.Services.AddTransient<IPageBuilder, PageBuilder>();
builder.Services.AddTransient<IArticle, Article>();

//Scoped
builder.Services.AddScoped<IResourceInstructions>(provider =>
{
    HttpContext? httpContext = provider.GetRequiredService<IHttpContextAccessor>().HttpContext;
    IEnumerable<Claim> claims = httpContext?.User.Claims ?? Array.Empty<Claim>();
    return new ResourceInstructions(claims);
});


builder.Services.AddControllers();
var app = builder.Build();

#if DEBUG
// Configure the HTTP request pipeline.
    app.UseSwagger();
    app.UseSwaggerUI();
#endif

app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        
        var response = new
        {
            Status = report.Status.ToString(),
            KeyManagement = report.Entries.ToDictionary(
                entry => entry.Key,
                entry => new
                {
                    Status = entry.Value.Status.ToString(),
                    Description = entry.Value.Description,
                    Duration = entry.Value.Duration
                })
        };
        
        await JsonSerializer.SerializeAsync(
            context.Response.Body,
            response,
            new JsonSerializerOptions { WriteIndented = true }
        );
    }
});
app.MapControllers();
app.Run();