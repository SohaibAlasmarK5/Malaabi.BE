using Mal3abi.Core.Entites;
using Mal3abi.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static Mal3abi.Core.Constants.Enums;
using static Mal3abi.Core.Constants.Values;

namespace Mal3abi.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddScopeInjectableServices(
            this IServiceCollection services, params Assembly[] assemblies)
        {
            var allTypes = assemblies.SelectMany(a => a.GetTypes()).ToList();
            var marker = typeof(IScopeInjectable);

            var serviceInterfaces = allTypes
                .Where(t => t.IsInterface && t != marker && marker.IsAssignableFrom(t));

            foreach (var serviceInterface in serviceInterfaces)
            {
                var expectedName = serviceInterface.Name.TrimStart('I'); // IAuthService -> AuthService

                var implementation = allTypes.FirstOrDefault(t =>
                    t.IsClass && !t.IsAbstract &&
                    t.Name == expectedName &&
                    serviceInterface.IsAssignableFrom(t));

                if (implementation is null)
                    throw new InvalidOperationException(
                        $"No class named '{expectedName}' implementing '{serviceInterface.Name}' was found.");

                services.AddScoped(serviceInterface, implementation);
            }

            return services;
        }

    }
    public static class RoleSeeder
    {
        public static async Task SeedRolesAndSuperAdminAsync(this IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            foreach (var role in new[] { Roles.SuperAdmin, Roles.Admin, Roles.User })
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }

            var superEmail = config["SuperAdmin:Email"];
            var superPassword = config["SuperAdmin:Password"];
            if (string.IsNullOrEmpty(superEmail) || string.IsNullOrEmpty(superPassword)) return;

            var existing = await userManager.FindByEmailAsync(superEmail);
            if (existing is not null) return;

            var superUser = new User
            {
                UserName = superEmail,
                Email = superEmail,
                FullName = "Super Admin",
                EmailConfirmed = true,
            };
            var result = await userManager.CreateAsync(superUser, superPassword);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(superUser, Roles.SuperAdmin);
        }
    }
}