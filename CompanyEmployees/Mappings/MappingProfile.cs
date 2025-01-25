using AutoMapper;
using Entities.DataTransferObjects;
using Entities.Models;

namespace WebApplication1.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Company, CompanyDto>() //source, destination
            .ForMember(c => //additional mapping rule for the FullAddress
                c.FullAddress,
            opt =>
                opt.MapFrom(x => x.Address + ' ' + x.Country));
        CreateMap<Employee, EmployeeDto>();
        CreateMap<CompanyForCreationDto, Company>();
        CreateMap<EmployeeForCreationDto, Employee>();
        CreateMap<EmployeeForUpdateDto, Employee>().ReverseMap();
        CreateMap<CompanyForUpdateDto, Company>();
    }
}