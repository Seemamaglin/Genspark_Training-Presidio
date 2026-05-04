using BusBookingSystem.Data;
using BusBookingSystem.DTOs.Requests;
using BusBookingSystem.DTOs.Responses;
using BusBookingSystem.Models;
using BusBookingSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusBookingSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _context = context;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest model)
    {
        if (await _userManager.FindByEmailAsync(model.Email) != null)
        {
            return BadRequest("Email is already registered.");
        }

        var user = new ApplicationUser
        {
            Email = model.Email,
            UserName = model.Email,
            Name = model.Name,
            PhoneNumber = model.PhoneNumber,
            Age = model.Age,
            Proof = model.Proof,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors.Select(e => e.Description));
        }

        if (!await _roleManager.RoleExistsAsync("User"))
        {
            await _roleManager.CreateAsync(new IdentityRole("User"));
        }

        await _userManager.AddToRoleAsync(user, "User");

        var token = JwtTokenService.CreateToken(user, new List<string> { "User" }, _configuration);

        return Ok(new AuthResponse
        {
            Token = token,
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            Role = "User",
            UserName = user.Name,
            Roles = new List<string> { "User" }
        });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null) return Unauthorized("Invalid credentials.");

        if (!await _userManager.CheckPasswordAsync(user, model.Password))
        {
            return Unauthorized("Invalid credentials.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = JwtTokenService.CreateToken(user, roles, _configuration);

        return Ok(new AuthResponse
        {
            Token = token,
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            Role = roles.FirstOrDefault() ?? string.Empty,
            UserName = user.Name,
            Roles = roles
        });
    }

    [HttpPost("request-operator-upgrade")]
    [Authorize(Policy = "User")]
    public async Task<IActionResult> RequestOperatorUpgrade([FromBody] RequestOperatorUpgradeRequest model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        if (string.IsNullOrWhiteSpace(model.Source) || string.IsNullOrWhiteSpace(model.Destination))
            return BadRequest("Source and destination are required.");

        if (model.Source.Trim().Equals(model.Destination.Trim(), StringComparison.OrdinalIgnoreCase))
            return BadRequest("Source and destination must be different.");

        // Remove any previous pending request for this user
        var existing = await _context.BusOperators
            .FirstOrDefaultAsync(o => o.UserId == user.Id && !o.IsEnabled);
        if (existing != null)
        {
            existing.Source = model.Source.Trim();
            existing.Destination = model.Destination.Trim();
        }
        else
        {
            _context.BusOperators.Add(new BusOperator
            {
                UserId = user.Id,
                Source = model.Source.Trim(),
                Destination = model.Destination.Trim(),
                IsEnabled = false
            });
        }

        user.IsBusOperatorRequest = true;
        await _userManager.UpdateAsync(user);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Operator upgrade request submitted." });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var roles = await _userManager.GetRolesAsync(user);
        return Ok(new
        {
            user.Id,
            user.Name,
            user.Email,
            user.PhoneNumber,
            user.Age,
            user.Proof,
            user.IsBusOperatorRequest,
            user.IsApprovedBusOperator,
            Roles = roles
        });
    }
}
