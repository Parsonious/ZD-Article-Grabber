using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ZD_Article_Grabber.Interfaces;
using ZD_Article_Grabber.Helpers;
using ZD_Article_Grabber.Services;
using ZD_Article_Grabber.Builders;
using ZD_Article_Grabber.Types;
using System.Text;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

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

//Custom
builder.Services.AddMemoryCache();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
                var jwtKey = builder.Configuration["Jwt:TokenKey"] ?? throw new InvalidOperationException("JWT Key is not configured.");

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ValidateIssuer = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidateAudience = false,
                    ValidateLifetime = false,
                    ClockSkew = TimeSpan.Zero, //strict expiration validation
                };
    });
builder.Services.AddAuthorization();


//Config
builder.Services.Configure<ZD_Article_Grabber.Config.ConfigOptions>(builder.Configuration);

//HTTP
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

//Singeltons
builder.Services.AddSingleton<IConfigOptions>(sp => sp.GetRequiredService<IOptions<ZD_Article_Grabber.Config.ConfigOptions>>().Value);
builder.Services.AddSingleton<IResourceFetcher, ResourceFetcher>();
builder.Services.AddSingleton<ICacheHelper, CacheHelper>();
builder.Services.AddSingleton<Dependencies>();

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
    IEnumerable<Claim> claims = httpContext?.User.Claims ?? [];
    return new ResourceInstructions(claims);
});

ConfigurationManager configuration = builder.Configuration;
IWebHostEnvironment environment = builder.Environment;

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                      .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true, reloadOnChange: true);


builder.Services.AddControllers();
var app = builder.Build();


// Configure the HTTP request pipeline.
if ( app.Environment.IsDevelopment() )
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();