using Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApplication1.ActionFilters;

public class ValidateCompanyExistsAttribute : IAsyncActionFilter
{
    private readonly ILoggerManager _logger;
    private readonly IRepositoryManager _repository;
    
    public ValidateCompanyExistsAttribute(ILoggerManager logger, IRepositoryManager repository)
    {
        _logger = logger;
        _repository = repository;
    }
    
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var companyId = (Guid)context.ActionArguments["id"];
        var trackChanges = context.HttpContext.Request.Method.Equals("PUT");
        var company = await _repository.Company.GetCompanyAsync(companyId, trackChanges);
        if (company == null)
        {
            _logger.LogInfo($"Company with id: {companyId} does not exist in the database.");
            context.Result =  new NotFoundResult();
        }
        //Since we already fetched the company here, we don't want to duplicate that in the controller as well
        else
        {
            context.HttpContext.Items.Add("company", company);
            await next();
        }
        
        /*  What we replaced, from the controller :
         * var company = await _repository.Company.GetCompanyAsync(id, trackChanges: false);
            if (company == null)
            {
                _logger.LogInfo($"Company with id: {id} does not exist in the database.");
                return NotFound();
            }
         */
    }
}