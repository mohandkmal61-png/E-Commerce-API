using ECommerce.Domin.Contracte;
using ECommerce.Domin.Entitie;
using ECommerce.Domin.Entitie.ProductModule;
using ECommerce.Domin.Entitie.Identitymodule;
using ECommerce.Domin.Entitie.Orders;
using ECommerce.Persistence.Data.Dbcontexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace ECommerce.Persistence.DataSeed
{
    public class DataInitilizer : IDataInitilizer
    {
        private readonly StoreDbcontext _dbcontext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DataInitilizer(StoreDbcontext dbcontext, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
           _dbcontext = dbcontext;
           _userManager = userManager;
           _roleManager = roleManager;
        }

        public async Task InitilizeAsync()
        {
            try
            {
                var hasProducts = await _dbcontext.Products.AnyAsync();
                var hasBrands =await _dbcontext.ProductBrands.AnyAsync();
                var hasTypes =  await _dbcontext.ProductTypes.AnyAsync();
                var hasDeliveryMethods = await _dbcontext.DeliveryMethods.AnyAsync();

                if (!hasBrands)
                  await  SeedDataFromJsonAsync<ProductBrand, int>("brands.json", _dbcontext.ProductBrands);

                if (!hasTypes)
                  await  SeedDataFromJsonAsync<ProductType, int>("types.json", _dbcontext.ProductTypes);

                if (!hasDeliveryMethods)
                    await SeedDeliveryMethodsFromJsonAsync("delivery.json");

                _dbcontext.SaveChanges();  

                if (!hasProducts)
                   await SeedDataFromJsonAsync<Product, int>("products.json", _dbcontext.Products);
              
                _dbcontext.SaveChanges();

                // Identity seed (roles + admin user)
                await SeedIdentityAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Data seed failing : {ex}");
                throw;
            }
        }

        private async Task SeedIdentityAsync()
        {
            try
            {
                // Create roles
                var adminRole = "Admin";
                var superAdminRole = "Super Admin";
                var userRole = "User";

                if (!await _roleManager.RoleExistsAsync(adminRole))
                    await _roleManager.CreateAsync(new IdentityRole(adminRole));

                if (!await _roleManager.RoleExistsAsync(superAdminRole))
                    await _roleManager.CreateAsync(new IdentityRole(superAdminRole));

                if (!await _roleManager.RoleExistsAsync(userRole))
                    await _roleManager.CreateAsync(new IdentityRole(userRole));

                // Seed default users
                var users = new List<(string UserName, string Email, string Password, string Role)>
                {
                    ("admin", "admin@ecommerce.com", "Admin@123", adminRole),
                    ("mohamed", "mohamed@ecommerce.com", "Mohamed@123", adminRole),
                    ("salma", "salma@ecommerce.com", "Salma@123", superAdminRole)
                };

                foreach (var (userName, email, password, role) in users)
                {
                    var user = await _userManager.FindByNameAsync(userName);
                    if (user == null)
                    {
                        user = new ApplicationUser
                        {
                            UserName = userName,
                            Email = email,
                            EmailConfirmed = true
                        };

                        var createResult = await _userManager.CreateAsync(user, password);
                        if (createResult.Succeeded)
                        {
                            await _userManager.AddToRoleAsync(user, role);
                            Console.WriteLine($"User '{userName}' created successfully with role '{role}'");
                        }
                        else
                        {
                            Console.WriteLine($"Failed creating user '{userName}': {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                        }
                    }
                    else
                    {
                        // Ensure role assigned
                        if (!await _userManager.IsInRoleAsync(user, role))
                        {
                            await _userManager.AddToRoleAsync(user, role);
                            Console.WriteLine($"Role '{role}' assigned to user '{userName}'");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Identity seed failing: {ex}");
                throw;
            }
        }
        private static async Task SeedDataFromJsonAsync<T, TKey>(string filename, DbSet<T> dbSet) where T : BaseEntity<TKey>
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var filePath = Path.Combine(baseDirectory, "DataSeed", "JsonFiles", filename);
            
            // If file not found in base directory, try relative path
            if (!File.Exists(filePath))
            {
                var currentDir = Directory.GetCurrentDirectory();
                filePath = Path.Combine(currentDir, "DataSeed", "JsonFiles", filename);
            }
            
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File {filename} not found in expected locations!");
            }
            
            try
            {
                using var dataStream = File.OpenRead(filePath);
                var data = JsonSerializer.Deserialize<List<T>>(dataStream, new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true,
                });
                
                if (data != null && data.Count > 0)
                {
                   await dbSet.AddRangeAsync(data);
                }
            }
            catch (Exception ex)  
            {
                throw new Exception($"Error reading data from JSON file {filename}: {ex.Message}", ex);
            }
        }

        private async Task SeedDeliveryMethodsFromJsonAsync(string filename)
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var filePath = Path.Combine(baseDirectory, "DataSeed", "JsonFiles", filename);

            if (!File.Exists(filePath))
            {
                var currentDir = Directory.GetCurrentDirectory();
                filePath = Path.Combine(currentDir, "DataSeed", "JsonFiles", filename);
            }

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File {filename} not found in expected locations!");

            using var dataStream = File.OpenRead(filePath);
            var data = await JsonSerializer.DeserializeAsync<List<DaliveryMethod>>(
                dataStream,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (data is not null && data.Count > 0)
                await _dbcontext.DeliveryMethods.AddRangeAsync(data);
        }

    }
}
