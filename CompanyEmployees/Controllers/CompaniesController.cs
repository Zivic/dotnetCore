using AutoMapper;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Microsoft.AspNetCore.Mvc;

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
        try
        {
            
            var companies = _repository.Company.GetAllCompanies(trackChanges: false);
            /* //old version, before mapper
            var companiesDto = companies.Select(c => new CompanyDTO
            {
                Id =  c.Id,
                Name = c.Name,
                FullAddress = string.Join(' ', c.Address, c.Country)
            }).ToList();
            */
            var companiesDto = _mapper.Map<IEnumerable<CompanyDTO>>(companies);
            return Ok(companiesDto);
        }
        catch (Exception e)
        {
            _logger.LogError($"Something went wrong in the {nameof(GetCompanies)} method: {e.Message}");
            return StatusCode(500);
        }
    }   
}