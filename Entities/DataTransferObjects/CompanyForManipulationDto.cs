using System.ComponentModel.DataAnnotations;

namespace Entities.DataTransferObjects;

public abstract class CompanyForManipulationDto
{
    [Required(ErrorMessage = "Company Name is a required field.")]
    [MaxLength(60, ErrorMessage = "Company Name cannot be longer than 60 characters.")]
    public string Name { get; set; }
    
    [Required(ErrorMessage = "Address is a required field.")]
    [MaxLength(60, ErrorMessage = "Address cannot be longer than 60 characters.")]
    public string Address { get; set; }
    
    public string Country { get; set; }
    
    public IEnumerable<EmployeeForCreationDto> Employees { get; set; }
}