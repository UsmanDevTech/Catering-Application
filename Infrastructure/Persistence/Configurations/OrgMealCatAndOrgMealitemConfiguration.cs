using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Configurations
{
    public sealed class OrgMealCatAndOrgMealitemConfiguration:IEntityTypeConfiguration<OrgMealCatAndOrgMealitem>
    {
        public void Configure(EntityTypeBuilder<OrgMealCatAndOrgMealitem> builder)
        {
            builder.HasKey(u => u.Id);
            builder.HasOne(u => u.OrgMealCategories)
                .WithMany(u => u.OrgMealCatAndOrgMealitems)
                .HasForeignKey(u => u.OrgMealCategoryId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(u=>u.OrgMealCategories)
                .WithMany(u=>u.OrgMealCatAndOrgMealitems)
                .HasForeignKey(u=>u.ItemId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
