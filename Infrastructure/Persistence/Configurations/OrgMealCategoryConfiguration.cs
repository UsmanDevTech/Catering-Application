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
    public sealed class OrgMealCategoryConfiguration:IEntityTypeConfiguration<OrgMealCategory>
    {
        public void Configure(EntityTypeBuilder<OrgMealCategory> builder)
        {
            builder.HasKey(u=>u.Id);

            builder.HasOne(u => u.OrgMeal)
                .WithMany(u => u.OrgMealCategories)
                .HasForeignKey(u => u.OrgMealId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
