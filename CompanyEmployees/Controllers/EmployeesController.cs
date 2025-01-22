using AutoMapper;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers;
[Route("api/companies/{companyId}/employees")]
[ApiController]
public class EmployeesController : ControllerBase
{
    private readonly ILoggerManager _logger;
    private readonly IRepositoryManager _repository;
    private readonly IMapper _mapper;

    public EmployeesController(IRepositoryManager repository, ILoggerManager logger, IMapper mapper)
    {
        _repository = repository;
        _logger = logger;
        _mapper = mapper;
    }
    
    [HttpGet]
    public IActionResult GetEmployeesForCompany(Guid companyId)
    {
        var company = _repository.Company.GetCompany(companyId, trackChanges: false);
        if (company == null)
        {
            _logger.LogInfo($"Company {companyId} does not exist in the database.");
            return NotFound();
        }
        var employeesFromDb = _repository.Employee.GetEmployees(companyId, trackChanges: false);
        var employeesDto = _mapper.Map<IEnumerable<EmployeeDTO>>(employeesFromDb);
        return Ok(employeesDto);
    }
}