using System;
using System.Collections.Generic;
using System.Text;

using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Persistence;

public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId);

        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasPrecision(18, 2);

        var electronicsId = Guid.Parse("d28888e9-2ba9-473a-a40f-e38cb54f9b35");
        var homeId = Guid.Parse("da2fd609-d754-4feb-8acd-c4f9ff13ba96");

        modelBuilder.Entity<Category>().HasData(
            new Category { Id = electronicsId, Name = "Электроника" },
            new Category { Id = homeId, Name = "Дом и сад" }
        );

        modelBuilder.Entity<Product>().HasData(
            new Product
            {
                Id = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d479"),
                Name = "Смартфон",
                Description = "Флагманский девайс",
                Price = 500000,
                StockQuantity = 10,
                CategoryId = electronicsId
            },
            new Product
            {
                Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440000"),
                Name = "Кофеварка",
                Description = "Варит отличный эспрессо",
                Price = 45000,
                StockQuantity = 5,
                CategoryId = homeId
            }
        );
    }
}
