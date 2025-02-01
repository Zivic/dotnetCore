using Asp.Versioning;
using AutoMapper;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.ActionFilters;
using WebApplication1.ModelBinders;

namespace WebApplication1;

[ApiVersion("1.0")]
[Route("api/v1/companies")]
[ApiController]
[ApiExplorerSettings(GroupName = "v1")]
/*[ResponseCache(CacheProfileName = "120SecondsDuration")]*/
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
    
    [HttpOptions]
    public IActionResult GetCompaniesOptions()
    {
        Response.Headers.Append("Allow", "GET, OPTIONS, POST");
        return Ok();
    }

    [HttpGet(Name = "GetCompanies"), Authorize(Roles = "Manager")]
    public async Task<IActionResult> GetCompanies()
    {
        //throw new Exception("Exception");
            var companies = await _repository.Company.GetAllCompaniesAsync(trackChanges: false);
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
    [HttpCacheExpiration(CacheLocation = CacheLocation.Public, MaxAge = 60)]
    [HttpCacheValidation(MustRevalidate = false)]
    public async Task<IActionResult> GetCompany(Guid id)
    {
        Console.WriteLine("[GetCompany] Received request. ");
        var company = await _repository.Company.GetCompanyAsync(id, trackChanges: false);
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
    /// <summary>
    /// Creates a newly created company
    /// </summary>
    /// <param name="company"></param>
    /// <returns>A newly created company</returns>
    /// <response code="201">Returns the newly created item</response>
    /// <response code="400">If the item is null</response>
    /// <response code="422">If the model is invalid</response>
    [HttpPost(Name = "CreateCompany")]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(422)]
    [ServiceFilter<ValidationFilterAttribute>]
    public async Task<IActionResult> CreateCompany([FromBody] CompanyForCreationDto company)
    {
        var companyEntity = _mapper.Map<Company>(company);
        _repository.Company.CreateCompany(companyEntity);
        await _repository.SaveAsync();

        var companyToReturn = _mapper.Map<CompanyDto>(companyEntity);
        return CreatedAtRoute("CompanyById", new { id = companyToReturn.Id }, companyToReturn);
    }

    [HttpGet("collection/({ids})", Name = "CompanyCollection")]
    public async Task<IActionResult> GetCompanyCollection([ModelBinder(BinderType = typeof(ArrayModelBinder))]IEnumerable<Guid> ids)
    {
        if (ids == null)
        {
            _logger.LogError("Parameter ids sent from the client is null.");
            return BadRequest("Parameter ids is null.");
        }
        var companyEntities = await _repository.Company.GetByIdsAsync(ids, trackChanges: false);
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
    [ServiceFilter<ValidationFilterAttribute>]
    public async Task<IActionResult> CreateCompanyCollection([FromBody] IEnumerable<CompanyForCreationDto> companyCollection)
    {
        var companyEntities = _mapper.Map<IEnumerable<Company>>(companyCollection);
        foreach (var company in companyEntities)
        {
            _repository.Company.CreateCompany(company);
        }
        await _repository.SaveAsync();

        var companyCollectionToReturn = _mapper.Map<IEnumerable<CompanyDto>>(companyEntities);
        var ids = string.Join(',', companyCollectionToReturn.Select(e => e.Id));
        return CreatedAtRoute("CompanyCollection", new {ids}, companyCollectionToReturn);
    }

    [HttpDelete("{id}")]
    [ServiceFilter<ValidateCompanyExistsAttribute>]
    public async  Task<IActionResult> DeleteCompany(Guid id)
    {
        var company = HttpContext.Items["company"] as Company;
        _repository.Company.DeleteCompany(company);
        await _repository.SaveAsync(); 
        return NoContent();
    }

    [HttpPut("{id}")]
    [ServiceFilter<ValidationFilterAttribute>]
    [ServiceFilter<ValidateCompanyExistsAttribute>]
    public async Task<IActionResult> UpdateCompany(Guid id, [FromBody] CompanyForUpdateDto company)
    {
        var companyEntity = HttpContext.Items["company"] as Company;
        _mapper.Map(company, companyEntity);
        await _repository.SaveAsync();
        
        return NoContent();
    }
}
