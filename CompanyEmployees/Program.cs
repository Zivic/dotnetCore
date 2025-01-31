using Asp.Versioning;
using AspNetCoreRateLimit;
using Contracts;
using Entities.DataTransferObjects;
using Microsoft.AspNetCore.Mvc;
using NLog;
using Repository.DataShaping;
using WebApplication1.ActionFilters;
using WebApplication1.Extensions;
using WebApplication1.Utility;

var builder = WebApplication.CreateBuilder(args);

//startup (old version)
LogManager.Setup().LoadConfigurationFromFile(string.Concat(Directory.GetCurrentDirectory(), "/nlog.config"));

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

//Configure(old version) //IOC CONTAINER
builder.Services.AddOpenApi();
builder.Services.ConfigureCors();
builder.Services.ConfigureLoggerService();
builder.Services.ConfigureVersioning();
builder.Services.ConfigureResponseCaching();
builder.Services.ConfigureHttpCacheHeaders();

builder.Services.AddAuthentication();
builder.Services.ConfigureIdentity();
builder.Services.ConfigureJWT(builder.Configuration);
builder.Services.AddScoped<IAuthenticationManager, AuthenticationManager>();

builder.Services.AddMemoryCache();
builder.Services.ConfigureRateLimitingOptions();
builder.Services.AddHttpContextAccessor();

builder.Services.ConfigureSqlContext(builder.Configuration);//Configuration ?
builder.Services.ConfigureRepositoryManager();
builder.Services.AddAutoMapper(typeof(Program)); //Startup ?
builder.Services.AddScoped<ValidationFilterAttribute>();
builder.Services.AddScoped<ValidateCompanyExistsAttribute>();
builder.Services.AddScoped<ValidateEmployeeForCompanyExistsAttribute>();

builder.Services.AddScoped<EmployeeLinks>();

/*
 * Registers the IDataShaper interface with the DataShaper implementation.
 * When IDataShaper<EmployeeDto> is requested via dependency injection,
 * an instance of DataShaper<EmployeeDto> will be provided
 */
builder.Services.AddScoped<IDataShaper<EmployeeDto>,DataShaper<EmployeeDto>>();

//allow content negotiation (xml response)
builder.Services.AddControllers(config =>
{
    config.RespectBrowserAcceptHeader = true;
    config.ReturnHttpNotAcceptable = true;
    config.CacheProfiles.Add("120SecondsDuration", new CacheProfile
    {
        Duration = 120
    });
})
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    })
    .AddNewtonsoftJson()
    .AddXmlDataContractSerializerFormatters()
    .AddCustomCSVFormatter();
builder.Services.AddCustomMediaTypes();
builder.Services.AddScoped<ValidateMediaTypeAttribute>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
//modern way to pass arg into the old Configure function
var logger = app.Services.GetRequiredService<ILoggerManager>();
app.ConfigureExceptionHandler(logger);

app.UseCors();
app.UseHttpsRedirection();
app.MapControllers();
app.UseResponseCaching();
app.UseHttpCacheHeaders();
app.UseIpRateLimiting();

//Generally add things to the request pipeline before UseRouting
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
//app.UseEndpoints(endpoints => endpoints.MapControllers());

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast2", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}