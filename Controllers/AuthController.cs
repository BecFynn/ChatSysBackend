using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using ChatSysBackend.Database.Models;

namespace ChatSysBackend.Controllers;


[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthController> _logger;
    
    public AuthController(SignInManager<User> signInManager, UserManager<User> userManager, IMapper mapper, ILogger<AuthController> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _mapper = mapper;
        _logger = logger;
    }
    

    [HttpGet("@me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDTO), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null) return Unauthorized();

        return Ok(_mapper.Map<UserDTO>(user));
    }
    
    [HttpGet("signin/{providerName}")]
    public IActionResult Login(string providerName, string? returnUrl = null)
    {
        var redirectUrl = Url.Action("ExternalCallback", new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(providerName, redirectUrl);
        return new ChallengeResult(providerName, properties);
    }

    [Authorize]
    [HttpGet("signout")]
    public async Task<IActionResult> SignOut(string? returnUrl = null)
    {
        await _signInManager.SignOutAsync();

        return returnUrl != null ? Redirect(returnUrl) : NoContent();
    }
    
    [AllowAnonymous]
    [HttpGet("external/callback")]
    public async Task<IActionResult> ExternalCallback(string? returnUrl = null)
    {
        if (User.Identity is { IsAuthenticated: true }) await _signInManager.SignOutAsync();
        Console.WriteLine("Das ist ein TEst 123");
        ExternalLoginInfo? info;
        try
        {
            info = await _signInManager.GetExternalLoginInfoAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting external login info");
            return StatusCode(500);
        }
        
        if (info == null)
        {   
            Console.WriteLine("apfel salat");
            _logger.LogInformation("Info is null");
            return BadRequest("info is null");
        }

        var providerKey = info.ProviderKey;
        if (info.LoginProvider == "bosch")
            providerKey = info.Principal.Claims.Single(x => x.Type == ClaimConstants.ObjectId).Value;

        var claims = info.Principal.Claims.ToList();

        var firstNameClaim = claims.SingleOrDefault(x => x.Type == ClaimTypes.GivenName)?.Value;
        var lastNameClaim = claims.SingleOrDefault(x => x.Type == ClaimTypes.Surname)?.Value;
        var displayNameClaim = claims.SingleOrDefault(x => x.Type == ClaimConstants.Name)?.Value;

        if (string.IsNullOrEmpty(firstNameClaim) || string.IsNullOrEmpty(lastNameClaim) ||
            string.IsNullOrEmpty(displayNameClaim)) return UnprocessableEntity();

        var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, providerKey, true, true);
        if (result.Succeeded)
        {
            var externalUser = await _userManager.FindByLoginAsync(info.LoginProvider, providerKey);

            if (externalUser == null) return StatusCode(500);

            var accessToken = info.AuthenticationTokens?.SingleOrDefault(x => x.Name == "access_token")?.Value;
            if (accessToken != null)
            {
                await _userManager.SetAuthenticationTokenAsync(externalUser, info.LoginProvider, "access_token",
                    accessToken);
            }


            externalUser.Name = firstNameClaim;
            externalUser.Surname = lastNameClaim;
            externalUser.DisplayName = displayNameClaim;

            await _userManager.UpdateAsync(externalUser);

            await _signInManager.SignOutAsync();
            await _signInManager.SignInAsync(externalUser, true, info.LoginProvider);

            // Success
            return returnUrl != null ? Redirect(returnUrl) : NoContent();
        }

        if (result.IsLockedOut) return BadRequest("Locked Out");

        // Check if we are already signed in (should never be the case)
        if (User.Identity is { IsAuthenticated: true })
        {
            return BadRequest();
        }
        Console.WriteLine(claims);
        string PreferredUserName = claims.SingleOrDefault(x => x.Type == ClaimConstants.PreferredUserName)?.Value;
        var user = new User()
        {
            Id = Guid.NewGuid(),
            UserName = PreferredUserName,
            Name = firstNameClaim,
            Surname = lastNameClaim,
            DisplayName = displayNameClaim,
            NtUser = PreferredUserName,
            Email = claims.Single(x => x.Type == ClaimTypes.Email).Value,
            Avatar = "https://api.dicebear.com/9.x/open-peeps/svg?seed=" + PreferredUserName
            //CreatedAt = DateTime.UtcNowtest
        };

        var userCreateResult = await _userManager.CreateAsync(user);
        if (!userCreateResult.Succeeded) return BadRequest();

        var addedLoginResult = await _userManager.AddLoginAsync(user,
            new UserLoginInfo(info.LoginProvider, providerKey, info.ProviderDisplayName));
        if (!addedLoginResult.Succeeded) return BadRequest();

        await _signInManager.SignInAsync(user, true, info.LoginProvider);
        return returnUrl != null ? Redirect(returnUrl) : NoContent();
    }
}