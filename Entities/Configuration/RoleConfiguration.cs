using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Entities.Models;

public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole>
{
    public void Configure(EntityTypeBuilder<IdentityRole> builder)
    {
        builder.HasData(
            // Needed to hardcode the ids , otherwise it throws an exception: 
            // The model for context 'RepositoryContext' changes each time it is built.
            // This is usually caused by dynamic values used in a 'HasData' call
            new IdentityRole
            {
                Id = "1",
                Name = "Manager",
                NormalizedName = "MANAGER"
            },
            new IdentityRole
            {
                Id = "2",
                Name = "Administrator",
                NormalizedName = "ADMINISTRATOR"
            }
        );
    }
}