using AlzaShopApi.Models.Database;
using AlzaShopApi.Toolkit;
using Microsoft.EntityFrameworkCore;

namespace AlzaShopApi.Database;

/// <summary>
/// Database context for the AlzaShop application.
/// </summary>
/// <remarks>
/// This class provides access to the database and defines the entity sets available for querying.
/// It also handles database seeding functionality for test and development purposes.
/// </remarks>
public class ApplicationDbContext(DbContextOptions dbContextOptions) : DbContext(dbContextOptions)
{
    /// <summary>
    /// Gets or sets the collection of Product entities in the database.
    /// </summary>
    /// <remarks>
    /// This DbSet represents the Products table in the database and allows for CRUD operations
    /// on product data.
    /// </remarks>
    public virtual DbSet<Product> Products { get; set; }

    /// <summary>
    /// Seeds the database with test product data if the minimum number of products doesn't exist.
    /// </summary>
    /// <remarks>
    /// This method checks if the database contains fewer products than the minimum required amount
    /// (defined in <see cref="Constants.Test.ProductsNo"/>). If so, it generates random product
    /// data and adds it to the database.
    /// 
    /// The method also ensures changes are persisted by calling SaveChanges after adding the data.
    /// </remarks>
    public void SeedData()
    {
        if (Products.Count() < Constants.Test.ProductsNo)
        {
            var delta = Constants.Test.ProductsNo - Products.Count();

            Products.AddRange(Enumerable.Range(1, delta).Select(i => Generator.GenerateRandomProduct()));
        }

        SaveChanges();
    }
}
