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
    public sealed class CartItemConfiguration:IEntityTypeConfiguration<CartItem>
    {
        public void Configure(EntityTypeBuilder<CartItem>builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasOne(u => u.Cart)
                .WithMany(u => u.CartItems)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
