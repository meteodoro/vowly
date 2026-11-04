using Microsoft.AspNetCore.Authorization;
using Vowly.Api.Enums;

namespace Vowly.Api.Controllers;

[Authorize(Roles = Perfis.Casal)]
public abstract class CasalControllerBase : VowlyControllerBase
{
    
}