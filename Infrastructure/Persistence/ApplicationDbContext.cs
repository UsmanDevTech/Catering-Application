using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence
{
    public class ApplicationDbContext:IdentityDbContext<ApplicationUser>, IApplicationDbContext
    {
    
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext>options):base(options)
        {
                
        }
        public DbSet<Allergy> Allergies =>Set< Allergy > ();
        //DbSet<AppContent> AppContent { get; }
        public DbSet<AppContent>AppContents => Set<AppContent>();
        public DbSet<Cart>Carts => Set<Cart>();
        public DbSet<CartItem>CartItems => Set<CartItem>();
        public DbSet<Item>Items => Set<Item>();
        public DbSet<MealCategory>MealCategories => Set<MealCategory>();
        public DbSet<MealType>MealType => Set<MealType>();
        public DbSet<Notification>Notifications => Set<Notification>();
        public DbSet<Order>Orders => Set<Order>();
        public DbSet<OrderItem>OrderItems => Set<OrderItem>();
        public DbSet<OrderRating>OrderRating => Set<OrderRating>();
        public DbSet<Organization>Organizations => Set<Organization>();
        public DbSet<OrgMeal>OrgMeals => Set<OrgMeal>();
        public DbSet<OrgMealCatAndOrgMealitem> OrgMealCatAndOrgMealitems => Set<OrgMealCatAndOrgMealitem>();
        public DbSet<OrgMealCategory>OrgMealCategories => Set<OrgMealCategory>();
        public DbSet<OTPhistory>OTPhistories => Set<OTPhistory>();

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
