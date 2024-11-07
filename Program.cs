using Microsoft.Extensions.Options;
using ZD_Article_Grabber.Interfaces;
using ZD_Article_Grabber.Services;
using ZD_Article_Grabber.Types;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


//Defaults
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();

//HTTP
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

//Singeltons
builder.Services.AddSingleton<IConfigOptions>(sp => sp.GetRequiredService<IOptions<ConfigOptions>>().Value);
builder.Services.AddSingleton<IResourceFetcher, ResourceFetcher>();
builder.Services.Configure<ConfigOptions>(builder.Configuration);


//Transients
builder.Services.AddTransient<Fetch>();

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