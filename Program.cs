var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpClient();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();


ConfigurationManager configuration = builder.Configuration;
IWebHostEnvironment environment = builder.Environment;

builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                      .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true, reloadOnChange: true);


    builder.Services.AddCors(options =>
    {
        options.AddPolicy(name: "AllowSpecificOrigins",
        policy =>
        {
            policy
            .WithOrigins("https://bepio.net", "http://bepio.net", 
            "https://compiqsolutions.zendesk.com", "https://compiqsolutions.zendesk.com/", 
            "https://parsonious.github.io", "https://web.postman.co/")
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

var app = builder.Build();

app.UseCors(app.Environment.IsDevelopment() ? "AllowAll" : "AllowSpecificOrigins"); //if app env is dev cors == allowall else == allowspecificorigins
//app.UseCors("AllowAll");
// Configure the HTTP request pipeline.
if ( app.Environment.IsDevelopment() )
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.Run();