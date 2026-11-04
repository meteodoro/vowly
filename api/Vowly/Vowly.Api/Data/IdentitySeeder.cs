using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vowly.Api.Enums;
using Vowly.Api.Models;

namespace Vowly.Api.Data;

public static class IdentitySeeder
{
    public static async Task SemearAsync(IServiceProvider services, IConfiguration configuration)
    {
        using var escopo = services.CreateScope();
        var roleManager = escopo.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = escopo.ServiceProvider.GetRequiredService<UserManager<Usuario>>();
        var logger = escopo.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("IdentitySeeder");

        foreach (var perfil in new[] { Perfis.Casal, Perfis.Admin })
        {
            if (!await roleManager.RoleExistsAsync(perfil))
                await roleManager.CreateAsync(new IdentityRole(perfil));
        }

        await CriarAdminAsync(userManager, configuration, logger);
        await DarPerfilCasalAQuemNaoTemAsync(userManager);
    }

    private static async Task CriarAdminAsync(
        UserManager<Usuario> userManager, IConfiguration configuration, ILogger logger)
    {
        var email = configuration["Admin:Email"];
        var senha = configuration["Admin:Senha"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha))
        {
            logger.LogWarning("Admin:Email e Admin:Senha não configurados. Nenhum Admin foi criado. Use dotnet user-secrets.");
            return;
        }

        var admin = await userManager.FindByEmailAsync(email);

        if (admin is null)
        {
            admin = new Usuario { UserName = email, Email = email, Nome = "Administrador" };
            var resultado = await userManager.CreateAsync(admin, senha);

            if (!resultado.Succeeded)
            {
                logger.LogError("Não foi possível criar o Admin: {Erros}",
                    string.Join("; ", resultado.Errors.Select(e => e.Description)));
                return;
            }
        }

        if (!await userManager.IsInRoleAsync(admin, Perfis.Admin))
            await userManager.AddToRoleAsync(admin, Perfis.Admin);
    }

    // Só para usuários criados antes dos perfis existirem (dados de teste)
    private static async Task DarPerfilCasalAQuemNaoTemAsync(UserManager<Usuario> userManager)
    {
        foreach (var usuario in await userManager.Users.ToListAsync())
        {
            if ((await userManager.GetRolesAsync(usuario)).Count == 0)
                await userManager.AddToRoleAsync(usuario, Perfis.Casal);
        }
    }
}