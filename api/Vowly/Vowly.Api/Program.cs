using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Vowly.Api.Data;
using Vowly.Api.Models;
using Vowly.Api.Services;

namespace Vowly.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddIdentityCore<Usuario>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();

        var chaveJwt = builder.Configuration["Jwt:Key"]
                       ?? throw new InvalidOperationException("Jwt:Key não configurada. Use dotnet user-secrets.");

        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chaveJwt))
                };
            });

        builder.Services.Configure<OpcoesPlano>(builder.Configuration.GetSection(OpcoesPlano.Secao));
        builder.Services.Configure<OpcoesEmpresa>(builder.Configuration.GetSection(OpcoesEmpresa.Secao));


        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddScoped<ICasamentoService, CasamentoService>();
        builder.Services.AddScoped<ICategoriaService, CategoriaService>();
        builder.Services.AddScoped<IDespesaService, DespesaService>();
        builder.Services.AddScoped<IParcelaService, ParcelaService>();
        builder.Services.AddScoped<IAssinaturaService, AssinaturaService>();
        builder.Services.AddScoped<IResumoFinanceiroService, ResumoFinanceiroService>();
        builder.Services.AddScoped<ICompromissoService, CompromissoService>();
        builder.Services.AddScoped<IRelatorioEmpresaService, RelatorioEmpresaService>();
        

        builder.Services.AddControllers()
            .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(
                new JsonStringEnumConverter(allowIntegerValues: false)));        
        builder.Services.AddOpenApi();

        var app = builder.Build();
        
        await IdentitySeeder.SemearAsync(app.Services, app.Configuration);

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}