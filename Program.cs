using Microsoft.Extensions.Options;
using ZD_Article_Grabber.Interfaces;
using ZD_Article_Grabber.Helpers;
using ZD_Article_Grabber.Services;
using ZD_Article_Grabber.Builders;
using ZD_Article_Grabber.Types;

var builder = WebApplication.CreateBuilder(args);

//Defaults
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();

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

ConfigurationManager configuration = builder.Configuration;
IWebHostEnvironment environment = builder.Environment;

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                      .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true, reloadOnChange: true);


    builder.Services.AddCors(options =>
    {
        options.AddPolicy(name: "AllowSpecificOrigins",
        policy =>
        {
            policy
            .WithOrigins("https://bepio.net", "http://bepio.net",
            "https://compiqsolutions.zendesk.com", 
            "https://parsonious.github.io", "https://web.postman.co")
            .AllowAnyHeader()
            .AllowAnyMethod();
        });
        options.AddPolicy(name: "AllowAll",
            policy =>
            {
                policy
                .AllowAnyHeader()
                .AllowAnyOrigin()
                .AllowAnyMethod();
            });
        options.AddDefaultPolicy(
            policy =>
            {
                policy
                .AllowAnyHeader()
                .AllowAnyOrigin()
                .AllowAnyMethod();
            });
    });

builder.Services.AddControllers();
var app = builder.Build();


// Configure the HTTP request pipeline.
if ( app.Environment.IsDevelopment() )
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowSpecificOrigins");
app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.Run();