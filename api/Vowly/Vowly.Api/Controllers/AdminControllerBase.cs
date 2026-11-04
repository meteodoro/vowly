using Microsoft.AspNetCore.Authorization;
using Vowly.Api.Enums;

namespace Vowly.Api.Controllers;

[Authorize(Roles = Perfis.Admin)]
public abstract class AdminControllerBase : VowlyControllerBase
{
    
}