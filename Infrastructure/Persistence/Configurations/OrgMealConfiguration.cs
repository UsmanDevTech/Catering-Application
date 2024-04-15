using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Configurations
{
    public sealed class OrgMealConfiguration: IEntityTypeConfiguration<OrgMeal>
    {
        public void Configure(EntityTypeBuilder<OrgMeal> builder)
        {
            builder.HasKey(u => u.Id);
            builder.HasOne(u => u.MealType)
                .WithMany(u => u.OrgMeals)
                .HasForeignKey(u => u.MealTypeId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(u => u.Organization)
                .WithMany(u => u.OrgMeals)
                .HasForeignKey(u => u.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
