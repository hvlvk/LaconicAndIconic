using LaconicAndIconic.DAL.Entities;
using Microsoft.AspNetCore.Identity;

namespace LaconicAndIconic.Web.Seeding;

public static class TestUserSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        const string testEmail = "test@example.com";
        const string testPassword = "Test1234!";

        if (await userManager.FindByEmailAsync(testEmail) is null)
        {
            var user = new ApplicationUser
            {
                UserName = testEmail,
                Email = testEmail,
                EmailConfirmed = true
            };

            await userManager.CreateAsync(user, testPassword);
        }
    }
}
