using Entities.Models;
using Entities.RequestFeatures;

namespace Contracts;

public interface IEmployeeRepository
{
    Task<IEnumerable<Employee>> GetEmployeesAsync(Guid companyId, EmployeeParameters employeeParameters, bool trackChanges);
    IEnumerable<Employee> GetEmployees(Guid companyId, bool trackChanges);
    Task<Employee> GetEmployeeAsync(Guid companyId, Guid id, bool trackChanges);
    Employee GetEmployee(Guid companyId, Guid id, bool trackChanges);
    
    void CreateEmployeeForCompany(Guid companyId, Employee employee);
    void DeleteEmployee(Employee employee);
}