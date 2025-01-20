using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models;

public class Company
{
    [Column("CompanyID")]
    public Guid Id { get; set; }
    
    [Required(ErrorMessage = "Company Name is a required field.")]
    [MaxLength(60, ErrorMessage = "Company Name cannot be longer than 60 characters.")]
    public string Name { get; set; }
    
    [Required(ErrorMessage = "Address is a required field.")]
    [MaxLength(60, ErrorMessage = "Address cannot be longer than 60 characters.")]
    public string Address { get; set; }
    
    public string Country { get; set; }
    
    public ICollection<Employee> Employees { get; set; }
}