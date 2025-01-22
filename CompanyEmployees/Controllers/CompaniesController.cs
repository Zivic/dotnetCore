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
        //throw new Exception("Exception");
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

    //TODO: it throws a format error when you don't have the exact number of characters instead of a 404, and doesn't even enter the function
    [HttpGet("{id}")]
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
            var companyDto = _mapper.Map<CompanyDTO>(company);
            return Ok(companyDto);
        }
    }
}