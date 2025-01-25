using Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApplication1.ActionFilters;

public class ValidationFilterAttribute : IActionFilter
{
    private readonly ILoggerManager _logger;

    public ValidationFilterAttribute(ILoggerManager logger)
    {
        _logger = logger;
    }
    
    public void OnActionExecuting(ActionExecutingContext context)
    {
        var action = context.RouteData.Values["action"];
        var controller = context.RouteData.Values["controller"];

        //param = company (CompanyForCreationDto object), 
        var param = context.ActionArguments
            .SingleOrDefault(x => x.Value.ToString().Contains("Dto")).Value;

        if (param == null)
        {
            _logger.LogError($"Object sent from client is null. Controller: {controller}, action: {action}");
            context.Result = new BadRequestObjectResult($"Object is null. Controller: {controller}, action: {action}");
        }

        if (!context.ModelState.IsValid)
        {
            _logger.LogError($"Invalid model state for the object sent from client. Controller: {controller}, action: {action}");
            context.Result = new UnprocessableEntityObjectResult(context.ModelState);
        }
        
        
        /* What we refactored, from the controller
        if (company == null)
        {
            _logger.LogError("CompanyForCreationDto object sent from the client is null.");
            return BadRequest("CompanyForCreationDto object is null.");
        }

        if (!ModelState.IsValid)
        {
            _logger.LogError("Invalid model state for CompanyForCreationDto.");
            return UnprocessableEntity(ModelState);
        }
        */
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}