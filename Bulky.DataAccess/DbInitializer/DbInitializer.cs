using BulkyBook.DataAccess.Data;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {

        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;


        public DbInitializer(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext db)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _db = db;

        }
        public void Initialize()
        {
            const int maxRetries = 5;
            const int delaySeconds = 5;

            int attempt = 0;
            bool dbReady = false;

            while (attempt < maxRetries && !dbReady)
            {
                try
                {
                    if (_db.Database.GetPendingMigrations().Any())
                    {
                        _db.Database.Migrate();
                    }

                    dbReady = true; // Success
                }
                catch (Exception ex)
                {
                    attempt++;
                    Console.WriteLine($"[DbInitializer] Attempt {attempt} failed: {ex.Message}");

                    if (attempt == maxRetries)
                        throw new Exception("Could not connect to database after multiple retries.", ex);

                    Thread.Sleep(delaySeconds * 1000);
                }
            }

            // Now that DB is ready, check/create roles and admin user
            if (!_roleManager.RoleExistsAsync(SD.Role_Customer).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Company)).GetAwaiter().GetResult();

                var adminUser = new ApplicationUser
                {
                    UserName = "admin@admin.com",
                    Email = "admin@admin.com",
                    Name = "Ely pasaporte",
                    PhoneNumber = "11122233333",
                    StreetAddress = "test",
                    State = "IL",
                    PostalCode = "23422",
                    City = "Chicago"
                };

                _userManager.CreateAsync(adminUser, "Admin123*").GetAwaiter().GetResult();
                var user = _db.ApplicationUsers.FirstOrDefault(u => u.Email == "admin@admin.com");

                if (user != null)
                {
                    _userManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();
                }
            }
        }
    }
}
