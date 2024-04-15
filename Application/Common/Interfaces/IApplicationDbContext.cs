using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<AppContent> AppContents { get; }
        DbSet<Allergy> Allergies { get; }
        DbSet<Cart> Carts { get; }
        DbSet<CartItem> CartItems { get; }
        DbSet<Item> Items { get; }
        DbSet<MealCategory> MealCategories { get; }
        DbSet<MealType> MealType { get; }
        DbSet<Notification> Notifications { get; }
        DbSet<Order> Orders { get; }
        DbSet<OrderItem> OrderItems { get; }
        DbSet<OrderRating> OrderRating { get; }
        DbSet<Organization> Organizations { get; }
        DbSet<OrgMeal> OrgMeals { get; }
        DbSet<OrgMealCatAndOrgMealitem> OrgMealCatAndOrgMealitems { get; }
        DbSet<OrgMealCategory> OrgMealCategories { get; }
        DbSet<OTPhistory> OTPhistories { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
