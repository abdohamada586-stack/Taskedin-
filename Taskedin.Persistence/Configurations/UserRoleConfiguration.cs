using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskedin.Domain.Entities;

namespace Taskedin.Persistence.Configurations;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRoles");

        builder.HasKey(ur => new { ur.UserId, ur.RoleId });

        builder
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(ur => ur.UserId);

        builder
            .HasOne<Role>()
            .WithMany()
            .HasForeignKey(ur => ur.RoleId);
    }
}
