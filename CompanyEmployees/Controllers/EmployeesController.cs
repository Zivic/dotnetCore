using AutoMapper;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.ActionFilters;
using Entities.RequestFeatures;
using Newtonsoft.Json;

namespace WebApplication1.Controllers;
[Route("api/companies/{companyId}/employees")]
[ApiController]
public class EmployeesController : ControllerBase
{
    private readonly ILoggerManager _logger;
    private readonly IRepositoryManager _repository;
    private readonly IMapper _mapper;
    private readonly IDataShaper<EmployeeDto> _dataShaper;

    public EmployeesController(IRepositoryManager repository, ILoggerManager logger, IMapper mapper, IDataShaper<EmployeeDto> dataShaper)
    {
        _repository = repository;
        _logger = logger;   
        _mapper = mapper;
        _dataShaper = dataShaper;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetEmployeesForCompany(Guid companyId, [FromQuery]EmployeeParameters employeeParameters)
    {
        if(!employeeParameters.ValidAgeRange)
            return BadRequest("Max age can't be less than min age.");
        
        var company = await _repository.Company.GetCompanyAsync(companyId, trackChanges: false);
        if (company == null)
        {
            _logger.LogInfo($"Company {companyId} does not exist in the database.");
            return NotFound();
        }
        var employeesFromDb = await _repository.Employee.GetEmployeesAsync(companyId, employeeParameters, trackChanges: false);
        Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(employeesFromDb.MetaData));
        
        var employeesDto = _mapper.Map<IEnumerable<EmployeeDto>>(employeesFromDb);
        return Ok(_dataShaper.ShapeData(employeesDto, employeeParameters.Fields));
    }

    [HttpGet("{id}", Name = "GetEmployeeForCompany")]
    public async Task<IActionResult> GetEmployeeForCompany(Guid companyId, Guid id)
    {
        var company = await _repository.Company.GetCompanyAsync(companyId, trackChanges: false);
        if (company == null)
        {
            _logger.LogInfo($"Company {companyId} does not exist in the database.");
            return NotFound();
        }
        var employeeFromDb = await _repository.Employee.GetEmployeeAsync(id: id, companyId: companyId, trackChanges: false);
        if (employeeFromDb == null)
        {
            _logger.LogInfo($"Employee {id} does not exist in the database.");
            return NotFound();
        }
        var employeeDto = _mapper.Map<EmployeeDto>(employeeFromDb);
        return Ok(employeeDto);
    }

    [HttpPost]
    [ServiceFilter<ValidationFilterAttribute>]
    public async Task<IActionResult> CreateEmployeeForCompany(Guid companyId, [FromBody] EmployeeForCreationDto employee)
    {
        var company = _repository.Company.GetCompany(companyId, trackChanges: false);
        if (company == null)
        {
            _logger.LogError($"Company with id {companyId} does not exist in the database.");
            return NotFound();
        }

        var employeeEntity = _mapper.Map<Employee>(employee);
        _repository.Employee.CreateEmployeeForCompany(companyId, employeeEntity);
        await _repository.SaveAsync();
        var employeeToReturn = _mapper.Map<EmployeeDto>(employeeEntity);
        return CreatedAtRoute("GetEmployeeForCompany", new { companyId, id = employeeToReturn.Id }, employeeToReturn);
    }

    [HttpDelete("{id}")]
    [ServiceFilter<ValidateEmployeeForCompanyExistsAttribute>]
    public async Task<IActionResult> DeleteEmployeeForCompany(Guid companyId, Guid id)
    {
        var employee = HttpContext.Items["employee"] as Employee;
        _repository.Employee.DeleteEmployee(employee);
        await _repository.SaveAsync();
        return NoContent();
    }

    [HttpPut("{id}")]
    [ServiceFilter<ValidationFilterAttribute>]
    public async Task<IActionResult> UpdateEmployeeForCompany(Guid companyId, Guid id, [FromBody] EmployeeForUpdateDto employee)
    {
        var employeeEntity = HttpContext.Items["employee"] as Employee;
       _mapper.Map(employee, employeeEntity);
       await _repository.SaveAsync();
       return NoContent();
    }

    [HttpPatch("{id}")]
    [ServiceFilter<ValidationFilterAttribute>]
    public async Task<IActionResult> PartiallyUpdateEmployeeForCompany(Guid companyId, Guid id,
        [FromBody] JsonPatchDocument<EmployeeForUpdateDto> patchDoc)
    {
        if (patchDoc == null)
        {
            _logger.LogError("EmployeeForUpdateDto object sent from the client is null.");
            return BadRequest("EmployeeForUpdateDto object is null.");
        }
        var employeeEntity = HttpContext.Items["employee"] as Employee;

        var employeeToPatch = _mapper.Map<EmployeeForUpdateDto>(employeeEntity);
        patchDoc.ApplyTo(employeeToPatch, modelState: ModelState);
        TryValidateModel(employeeToPatch);
        if(!ModelState.IsValid)
        {
            _logger.LogError("Invalid model state for the patch document");
            return UnprocessableEntity(ModelState);
        }
        
        _mapper.Map(employeeToPatch, employeeEntity);
        await _repository.SaveAsync();
        return NoContent();
    }   
}