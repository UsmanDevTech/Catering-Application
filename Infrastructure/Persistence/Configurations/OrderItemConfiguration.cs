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
    public sealed class OrderItemConfiguration:IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.HasKey(u => u.Id);
            builder.HasOne(u => u.Order)
                .WithMany(u => u.OrderItems)
                .HasForeignKey(u => u.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
