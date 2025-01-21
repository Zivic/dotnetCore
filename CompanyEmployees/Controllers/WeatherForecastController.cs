using Contracts;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers;

[Route("[controller]")]
[ApiController]
public class WeatherForecastController : ControllerBase
{
    private ILoggerManager _logger;
    private readonly IRepositoryManager _repository;

    public WeatherForecastController(ILoggerManager logger, IRepositoryManager repository)
    {
        _logger = logger;
        _repository = repository;
    }

    [HttpGet]
    public IEnumerable<string> Get()
    {
        _logger.LogInfo("Here is the Info message from our values controller.");
        _logger.LogDebug("Here is the Debug message from our values controller.");
        _logger.LogWarn("Here is the Warn message from our values controller.");
        _logger.LogError("Here is the Error message from our values controller.");
        
        //_repository.Company.anymethod();
        //_repository.Employee.anymethod();
        
        return new string[] { "value1", "value2" };
    }
}