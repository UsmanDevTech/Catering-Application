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

    namespace Infrastructure.Persistence.Configurations
    {
        public class OrderConfiguration : IEntityTypeConfiguration<Order>
        {
            public void Configure(EntityTypeBuilder<Order> builder)
            {
                builder.HasKey(x => x.Id);

                // Configure the relationship with OrderRating as the dependent and Order as the principal
                builder.HasOne(x => x.Rating)
                    .WithOne(x => x.Order)
                    .HasForeignKey<OrderRating>(x => x.OrderId)  // Foreign key property in OrderRating
                    .OnDelete(DeleteBehavior.Cascade);
            }
        }

        public class OrderRatingConfiguration : IEntityTypeConfiguration<OrderRating>
        {
            public void Configure(EntityTypeBuilder<OrderRating> builder)
            {
                builder.HasKey(x => x.Id);
            }
        }
    }


}
