using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MX.TalkWithTiles.Web.Controllers;

/// <summary>
/// Development-only controller for authenticating test users via cookie scheme.
/// Only registered when Testing:Enabled is true in Development environment.
/// </summary>
[ApiController]
[Route("api/test")]
[AllowAnonymous]
public class TestAuthController(IWebHostEnvironment environment, IConfiguration configuration) : ControllerBase
{
    [HttpPost("signin")]
    public async Task<IActionResult> SignIn([FromBody] TestSignInRequest request)
    {
        if (!IsTestingEnabled())
        {
            return NotFound();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, request.UserId),
            new(ClaimTypes.Name, request.UserName),
            new(ClaimTypes.Email, request.Email)
        };

        if (!string.IsNullOrEmpty(request.Role))
        {
            claims.Add(new Claim(ClaimTypes.Role, request.Role));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties { IsPersistent = true });

        return Ok(new { message = "Signed in", userId = request.UserId, userName = request.UserName });
    }

    [HttpPost("signout")]
    public async Task<IActionResult> SignOutUser()
    {
        if (!IsTestingEnabled())
        {
            return NotFound();
        }

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok(new { message = "Signed out" });
    }

    private bool IsTestingEnabled() =>
        environment.IsDevelopment()
        && string.Equals(configuration["Testing:Enabled"], "true", StringComparison.OrdinalIgnoreCase);
}

public class TestSignInRequest
{
    public required string UserId { get; set; }
    public required string UserName { get; set; }
    public required string Email { get; set; }
    public string? Role { get; set; }
}
