using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var chaveJwt = builder.Configuration["Jwt:Key"]
               ?? throw new InvalidOperationException("Jwt:Key não configurada. Use dotnet user-secrets.");

// O gateway confere o token antes de repassar a requisição (mesmos valores da API)
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("casal", policy => policy.RequireAuthenticatedUser().RequireRole("Casal"));
    options.AddPolicy("admin", policy => policy.RequireAuthenticatedUser().RequireRole("Admin"));
});

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// Marca as respostas que passaram pelo gateway (facilita provar isso nos testes)
app.Use(async (contexto, proximo) =>
{
    contexto.Response.Headers["X-Gateway"] = "Vowly.Gateway";
    await proximo();
});

// 401, 403 e falhas do proxy com mensagem em JSON, no mesmo formato da API
app.UseStatusCodePages(async contexto =>
{
    var resposta = contexto.HttpContext.Response;

    var mensagem = resposta.StatusCode switch
    {
        StatusCodes.Status401Unauthorized => "Você precisa entrar para acessar este recurso.",
        StatusCodes.Status403Forbidden => "Você não tem permissão para acessar este recurso.",
        StatusCodes.Status404NotFound => "Rota não encontrada.",
        StatusCodes.Status502BadGateway
            or StatusCodes.Status503ServiceUnavailable
            or StatusCodes.Status504GatewayTimeout =>
            "O servidor não está disponível no momento. Tente novamente em instantes.",
        _ => "Não foi possível processar a requisição."
    };

    await resposta.WriteAsJsonAsync(new { mensagem });
});

app.UseAuthentication();
app.UseAuthorization();

app.MapReverseProxy();

app.Run();