using Dsw2026Tpi.Data.Identity;
using Dsw2026Tpi.Data.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Dsw2026Tpi.Data.Extensions;

public static class DbContextExtensions
{
    public static void Seedwork<T>(this DbContext context, string dataSource) where T : class
    {
        if (context.Set<T>().Any()) return;

        var json = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, dataSource));
        var entities = JsonSerializer.Deserialize<List<T>>(json, JsonOptions.JsonSerializerOptions);

        if (entities == null || entities.Count == 0) return;

        context.Set<T>().AddRange(entities);
        context.SaveChanges();
    }

    public static void SeedAdminUser(this AuthenticationDbContext context, IConfiguration configuration)
    {
        var email = configuration["AdminSeed:Email"];
        var password = configuration["AdminSeed:Password"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("AdminSeed no configurado.");

        if (context.Users.Any(u => u.NormalizedEmail == email.ToUpperInvariant()))
            return;

        var adminRole = context.Roles
            .FirstOrDefault(r => r.NormalizedName == "ADMINISTRADOR");

        if (adminRole == null)
            return;

        var admin = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = email,
            NormalizedUserName = email.ToUpperInvariant(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString(),
            Deleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var hasher = new PasswordHasher<ApplicationUser>();
        admin.PasswordHash = hasher.HashPassword(admin, password);

        context.Users.Add(admin);
        context.SaveChanges();

        context.UserRoles.Add(new IdentityUserRole<string>
        {
            UserId = admin.Id,
            RoleId = adminRole.Id
        });

        context.SaveChanges();
    }
}