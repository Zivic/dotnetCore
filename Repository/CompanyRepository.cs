using Contracts;
using Entities;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Repository;

public class CompanyRepository : RepositoryBase<Company>, ICompanyRepository
{
    public CompanyRepository(RepositoryContext repositoryContext) : base(repositoryContext)
    {
    }

    public async Task<IEnumerable<Company>> GetAllCompaniesAsync(bool trackChanges) =>
    await FindAll(trackChanges)
        .OrderBy(c => c.Name)
        .ToListAsync();

    public IEnumerable<Company> GetAllCompanies(bool trackChanges) =>
        FindAll(trackChanges).OrderBy(c => c.Name).ToList();

    public async Task<Company> GetCompanyAsync(Guid companyId, bool trackChanges) =>
    await FindByCondition(c => 
            c.Id.Equals(companyId), trackChanges)
            .SingleOrDefaultAsync();
    public Company GetCompany(Guid companyId, bool trackChanges) =>
        FindByCondition(c => 
                c.Id.Equals(companyId), trackChanges)
                .SingleOrDefault();

    public void CreateCompany(Company company) => Create(company);
    
    public void DeleteCompany(Company company) => Delete(company);
    public async Task<IEnumerable<Company>> GetByIdsAsync(IEnumerable<Guid> ids, bool trackChanges) =>
    await FindByCondition(e => ids.Contains(e.Id), trackChanges).ToListAsync();

    public IEnumerable<Company> GetByIds(IEnumerable<Guid> ids, bool trackChanges) =>
        FindByCondition(e => ids.Contains(e.Id), trackChanges).ToList();
}