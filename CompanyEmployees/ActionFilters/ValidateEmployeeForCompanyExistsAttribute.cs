using Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApplication1.ActionFilters;

public class ValidateEmployeeForCompanyExistsAttribute : IAsyncActionFilter
{
    private readonly ILoggerManager _logger;
    private readonly IRepositoryManager _repository;
    public ValidateEmployeeForCompanyExistsAttribute(ILoggerManager logger, IRepositoryManager repository)
    {
        _logger = logger;
        _repository = repository;
    }
    
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var method = context.HttpContext.Request.Method;
        //TODO: test if this works:
        var trackChanges = method.Equals("PUT") || method.Equals("PATCH");
        var companyId = (Guid)context.ActionArguments["companyId"];
        var company = await _repository.Company.GetCompanyAsync(companyId, false); //specifically false
        if (company == null)
        {
            _logger.LogInfo($"Company with id {companyId} does not exist in the database.");
            context.Result = new NotFoundResult();
            return;
        }

        var id = (Guid)context.ActionArguments["id"];
        var employee = await _repository.Employee.GetEmployeeAsync(companyId, id, trackChanges);
        if (employee == null)
        {
            _logger.LogInfo($"Employee {id} does not exist in the database.");
            context.Result = new NotFoundResult();
        }
        else
        {
            context.HttpContext.Items.Add("employee", employee);
            await next();
        }
    }
}

/*var company = await _repository.Company.GetCompanyAsync(companyId, trackChanges: false);
if (company == null)
{
    _logger.LogInfo($"Company with id {companyId} does not exist in the database.");
    return NotFound();
}
var employee = await _repository.Employee.GetEmployeeAsync(companyId: companyId, id: id,  trackChanges: false);
if (employee == null)
{
    _logger.LogInfo($"Employee {id} does not exist in the database.");
    return NotFound();
}*/