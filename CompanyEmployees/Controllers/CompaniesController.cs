using AutoMapper;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.ModelBinders;

namespace WebApplication1;

[Route("api/companies")]
[ApiController]
public class CompaniesController : ControllerBase
{
    private readonly ILoggerManager _logger;
    private readonly IRepositoryManager _repository;
    private readonly IMapper _mapper;

    public CompaniesController(IRepositoryManager repository, ILoggerManager logger, IMapper mapper)
    {
        _repository = repository;
        _logger = logger;
        _mapper = mapper;
    }

    [HttpGet]
    public IActionResult GetCompanies()
    {
        //throw new Exception("Exception");
            var companies = _repository.Company.GetAllCompanies(trackChanges: false);
            /* //old version, before mapper
            var companiesDto = companies.Select(c => new CompanyDto
            {
                Id =  c.Id,
                Name = c.Name,
                FullAddress = string.Join(' ', c.Address, c.Country)
            }).ToList();
            */
            var companiesDto = _mapper.Map<IEnumerable<CompanyDto>>(companies);
            return Ok(companiesDto);
    }

    //TODO: it throws a format error when you don't have the exact number of characters instead of a 404, and doesn't even enter the function
    [HttpGet("{id}", Name = "CompanyById")]
    public IActionResult GetCompany(Guid id)
    {
        Console.WriteLine("[GetCompany] Received request. ");
        var company = _repository.Company.GetCompany(id, trackChanges: false);
        if (company == null)
        {
            _logger.LogInfo($"Company with id: {id} does not exist in the database.");
            return NotFound();
        }
        else
        {
            var companyDto = _mapper.Map<CompanyDto>(company);
            return Ok(companyDto);
        }
    }

    [HttpPost]
    public IActionResult CreateCompany([FromBody] CompanyForCreationDto company)
    {
        if (company == null)
        {
            _logger.LogError("CompanyForCreationDto object sent from the client is null.");
            return BadRequest("CompanyForCreationDto object is null.");
        }

        var companyEntity = _mapper.Map<Company>(company);
        _repository.Company.CreateCompany(companyEntity);
        _repository.Save();

        var companyToReturn = _mapper.Map<CompanyDto>(companyEntity);
        return CreatedAtRoute("CompanyById", new { id = companyToReturn.Id }, companyToReturn);
    }

    [HttpGet("collection/({ids})", Name = "CompanyCollection")]
    public IActionResult GetCompanyCollection([ModelBinder(BinderType = typeof(ArrayModelBinder))]IEnumerable<Guid> ids)
    {
        if (ids == null)
        {
            _logger.LogError("Parameter ids sent from the client is null.");
            return BadRequest("Parameter ids is null.");
        }
        var companyEntities = _repository.Company.GetByIds(ids, trackChanges: false);
        if (ids.Count() != companyEntities.Count())
        {
            _logger.LogError("Some ids are invalid in a collection.");
            return NotFound();
        }

        var companiesToReturn = _mapper.Map<IEnumerable<CompanyDto>>(companyEntities);
        return Ok(companiesToReturn);
    }

    //TODO: fix - "field employee is required"
    [HttpPost("collection")]
    public IActionResult CreateCompanyCollection([FromBody] IEnumerable<CompanyForCreationDto> companyCollection)
    {
        if (companyCollection == null)
        {
            _logger.LogError("Parameter companyCollection sent from the client is null.");
            return BadRequest();
        }

        var companyEntities = _mapper.Map<IEnumerable<Company>>(companyCollection);
        foreach (var company in companyEntities)
        {
            _repository.Company.CreateCompany(company);
        }
        _repository.Save();

        var companyCollectionToReturn = _mapper.Map<IEnumerable<CompanyDto>>(companyEntities);
        var ids = string.Join(',', companyCollectionToReturn.Select(e => e.Id));
        return CreatedAtRoute("CompanyCollection", new {ids}, companyCollectionToReturn);
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteCompany(Guid id)
    {
        var company = _repository.Company.GetCompany(id, trackChanges: false);
        if (company == null)
        {
            _logger.LogInfo($"Company with id: {id} does not exist in the database.");
            return NotFound();
        }
        _repository.Company.DeleteCompany(company);
        _repository.Save(); 
        return NoContent();
    }

    [HttpPut("{id}")]
    public IActionResult UpdateCompany(Guid id, [FromBody] CompanyForUpdateDto company)
    {
        if (company == null)
        {
            _logger.LogError("CompanyForUpdateDto object sent from the client is null.");
            return BadRequest();
        }
        var companyEntity = _repository.Company.GetCompany(id, trackChanges: true);
        if (companyEntity == null)
        {
            _logger.LogInfo($"Company with id: {id} does not exist in the database.");
            return NotFound();
        }
        _mapper.Map(company, companyEntity);
        _repository.Save();
        
        return NoContent();
    }
}
