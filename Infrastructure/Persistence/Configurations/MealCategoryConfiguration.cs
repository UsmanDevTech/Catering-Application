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
    public sealed class MealCategoryConfiguration: IEntityTypeConfiguration<MealCategory>
    {
        public void Configure(EntityTypeBuilder<MealCategory> builder)
        {
            builder.HasKey(u => u.MealTypeId);
                
            builder.HasOne(u => u.MealType)
                .WithMany(u=>u.MealCategories)
                .HasForeignKey(u=>u.MealTypeId)
                .OnDelete(DeleteBehavior.NoAction);
                
        }
    }
}
