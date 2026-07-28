using Dsw2026Tpi.CrossCutting.Identity;
using Dsw2026Tpi.Data.Identity;
using Microsoft.AspNetCore.Identity;

namespace Dsw2026Tpi.Api.Configurations;

public static class AdminSeed
{
    public static async Task SeedAsync(
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        var adminEmail = configuration["AdminSeed:Email"];
        var adminPassword = configuration["AdminSeed:Password"];

        if (string.IsNullOrWhiteSpace(adminEmail) ||
            string.IsNullOrWhiteSpace(adminPassword))
        {
            throw new Exception("AdminSeed no configurado");
        }

        // Crear rol administrador si no existe
        if (!await roleManager.RoleExistsAsync(Roles.Administrator))
        {
            await roleManager.CreateAsync(new IdentityRole(Roles.Administrator));
        }

        // Verificar si existe el usuario
        var admin = await userManager.FindByEmailAsync(adminEmail);

        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Deleted = false
            };

            var result = await userManager.CreateAsync(admin, adminPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ",
                    result.Errors.Select(e => e.Description));

                throw new Exception($"Error creando admin: {errors}");
            }
        }

        // Asignar rol administrador
        if (!await userManager.IsInRoleAsync(admin, Roles.Administrator))
        {
            await userManager.AddToRoleAsync(admin, Roles.Administrator);
        }
    }
}