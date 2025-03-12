using DymanicRolesBasedAuthorization.Data;
using DymanicRolesBasedAuthorization.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace DymanicRolesBasedAuthorization.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUsers> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public AccountController(UserManager<ApplicationUsers> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _context = context;
        }

        [HttpPost("SignUp")]
        public async Task<IActionResult> Register([FromBody] SignUp inParams)
        {
            try
            {
                if (inParams.Password.Count() < 6)
                {
                    return Ok(new { status_code = 0, status_message = "Password must be at least 6 characters long." });
                }

                var userExists = await _userManager.FindByEmailAsync(inParams.Email);
                if (userExists != null)
                {
                    return Ok(new { status_code = 0, status_message = "Email already exists." });
                }

                var user = new ApplicationUsers
                {
                    UserName = inParams.Email,
                    FullName = inParams.FullName,
                    Email = inParams.Email,
                    PhoneNumber = inParams.PhoneNumber,
                    IsActive = true,
                    IsDeleted = false,
                    UpdatedDate = DateTime.Now,
                    CreatedDate = DateTime.Now,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, inParams.Password);


                return Ok(new { status_message = "User Registered Successfully", status_code = 1, result, user });


            }
            catch (Exception e)
            {
                return Ok(new { status_message = "Sorry! Something went wrong..", status_code = 0, error = e.Message });
            }
        }

        [HttpPost("signin")]
        public async Task<IActionResult> SignIn([FromBody] SignIn model)
        {
            // Validate model
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    StatusCode = 0,
                    StatusMessage = "Invalid input data",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                });
            }

            // Find user by email
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return Ok(new
                {
                    StatusCode = 0,
                    StatusMessage = "Invalid email or password"
                });
            }

            // Check password
            if (!await _userManager.CheckPasswordAsync(user, model.Password))
            {
                return Ok(new
                {
                    StatusCode = 0,
                    StatusMessage = "Invalid email or password"
                });
            }

            // Get user roles
            var userRoles = await _userManager.GetRolesAsync(user);

            // Create claims for token
            var authClaims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Name, user.UserName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            // Add role claims
            authClaims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

            // Generate token
            var token = GenerateJwtToken(authClaims);

            // Store user ID in session for backup authentication method
            HttpContext.Session.SetString("UserId", user.Id);

            // Get accessible APIs based on user roles
            var accessibleApis = await _context.tblRoleApiPermissions
                .Where(p => userRoles.Contains(p.RoleName))
                .Select(p => new { p.Endpoint, p.HttpMethod })
                .Distinct()
                .ToListAsync();

            return Ok(new
            {
                StatusCode = 1,
                StatusMessage = "Login successful",
                Data = new
                {
                    Token = token,
                    User = new
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FullName = user.FullName,
                        UserName = user.UserName,
                        Roles = userRoles
                    },
                    AccessibleApis = accessibleApis
                }
            });
        }

        private string GenerateJwtToken(IEnumerable<Claim> claims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpiryMinutes"] ?? "60"));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Issuer"], // Using the same value for audience as in your Program.cs
                claims: claims,
                expires: expires,
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
