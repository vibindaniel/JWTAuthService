using DuploAuth.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
using static DuploAuth.Helpers.Constants.Strings;

namespace DuploAuth.Repository
{
    public static class DefaultInitializer
    {
        public static void SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            if (!roleManager.RoleExistsAsync(JwtClaims.User).Result)
            {
                IdentityRole role = new IdentityRole();
                role.Name = JwtClaims.User;
                IdentityResult roleResult = roleManager.
                CreateAsync(role).Result;
            }
            if (!roleManager.RoleExistsAsync(JwtClaims.Admin).Result)
            {
                IdentityRole role = new IdentityRole();
                role.Name = JwtClaims.Admin;
                IdentityResult roleResult = roleManager.
                CreateAsync(role).Result;
            }
            if (!roleManager.RoleExistsAsync(JwtClaims.SuperAdmin).Result)
            {
                IdentityRole role = new IdentityRole();
                role.Name = JwtClaims.SuperAdmin;
                IdentityResult roleResult = roleManager.
                CreateAsync(role).Result;
            }
        }

        public static void SeedUsers(UserManager<AppUser> userManager)
        {
            var default_user_email = Environment.GetEnvironmentVariable("DEFAULT_USER_EMAIL");
            var default_user_name = Environment.GetEnvironmentVariable("DEFAULT_USER_NAME");
            var Name = "Admin";
            if (!string.IsNullOrEmpty(default_user_name))
            {
                Name = default_user_name;
            }
            if (!string.IsNullOrEmpty(default_user_email))
            {
                try
                {
                    var adminUser = userManager.Users.SingleOrDefaultAsync(x => x.Role == JwtClaims.SuperAdmin).Result;
                    if (adminUser.Email != default_user_email || adminUser.Name != Name)
                    {
                        adminUser.Email = default_user_email;
                        adminUser.UserName = default_user_email;
                        adminUser.Name = Name;
                        var result = userManager.UpdateAsync(adminUser).Result;
                        Debug.WriteLine(result);
                        if (result.Succeeded)
                        {
                            userManager.AddToRoleAsync(adminUser, JwtClaims.SuperAdmin).Wait();
                        }
                    }
                }
                catch
                {
                    var user = new AppUser()
                    {
                        Name = Name,
                        Email = default_user_email,
                        Role = JwtClaims.SuperAdmin,
                        UserName = default_user_email
                    };

                    var result = userManager.CreateAsync(user).Result;
                    Debug.WriteLine(result);
                    if (result.Succeeded)
                    {
                        userManager.AddToRoleAsync(user, JwtClaims.SuperAdmin).Wait();
                    }
                }
            }
        }
    }
}