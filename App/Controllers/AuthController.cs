using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase {
    private readonly UserManager<IdentityUser> _userManager;

    public AuthController(UserManager<IdentityUser> userManager) {
        _userManager = userManager;
    }

    [HttpPost("signup")]
    public async Task<IActionResult> SignUp([FromBody] SignUpModel model) {
        var user = new IdentityUser { UserName = model.Email, Email = model.Email };
        var result = await _userManager.CreateAsync(user, model.Password ?? "");
        if (result.Succeeded) return Ok("User created");
        return BadRequest(result.Errors);
    }

    // [HttpPost("login")] - Add later with SignInManager and JWT generation

    public class SignUpModel {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}