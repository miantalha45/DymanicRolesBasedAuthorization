using DymanicRolesBasedAuthorization.Data;
using DymanicRolesBasedAuthorization.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DymanicRolesBasedAuthorization.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]  // Only admins can manage roles/permissions
    public class RolesManagementController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUsers> _userManager;
        private readonly ApplicationDbContext _dbContext;

        public RolesManagementController(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUsers> userManager,
            ApplicationDbContext dbContext)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _dbContext = dbContext;
        }

        // 1. Create a new role
        [HttpPost("create")]
        public async Task<IActionResult> CreateRole(string roleName)
        {
            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (roleExists)
            {
                return BadRequest(new { Message = "Role already exists" });
            }

            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
            if (result.Succeeded)
            {
                return Ok(new { Message = "Role created successfully" });
            }

            return BadRequest(new { Errors = result.Errors });
        }

        // 2. Assign role to user
        [HttpPost("assign")]
        public async Task<IActionResult> AssignRoleToUser(string email, string roleName)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                return NotFound(new { Message = "Role not found" });
            }

            var result = await _userManager.AddToRoleAsync(user, roleName);
            if (result.Succeeded)
            {
                return Ok(new { Message = "Role assigned successfully" });
            }

            return BadRequest(new { Errors = result.Errors });
        }

        // 3. Get all roles
        [HttpGet("GetAllRoles")]
        public IActionResult GetAllRoles()
        {
            var roles = _roleManager.Roles.Select(r => new { r.Id, r.Name }).ToList();
            return Ok(roles);
        }

        // 4. Assign API permission to role
        [HttpPost("permissions")]
        public async Task<IActionResult> AssignApiPermission(RoleApiPermissionDto dto)
        {
            var roleExists = await _roleManager.RoleExistsAsync(dto.RoleName);
            if (!roleExists)
            {
                return NotFound(new { Message = "Role not found" });
            }

            var permission = new RoleApiPermission
            {
                RoleName = dto.RoleName,
                Endpoint = dto.Endpoint,
                HttpMethod = dto.HttpMethod.ToUpper(),
                Description = dto.Description
            };

            // Check for duplicate
            var exists = await _dbContext.tblRoleApiPermissions
                .AnyAsync(p => p.RoleName == dto.RoleName &&
                             p.Endpoint == dto.Endpoint &&
                             p.HttpMethod == dto.HttpMethod);

            if (exists)
            {
                return BadRequest(new { Message = "Permission already exists" });
            }

            _dbContext.tblRoleApiPermissions.Add(permission);
            await _dbContext.SaveChangesAsync();

            return Ok(new { Message = "API permission assigned to role successfully" });
        }

        // 5. Get permissions for a role
        [HttpGet("permissions/{roleName}")]
        public async Task<IActionResult> GetRolePermissions(string roleName)
        {
            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                return NotFound(new { Message = "Role not found" });
            }

            var permissions = await _dbContext.tblRoleApiPermissions
                .Where(p => p.RoleName == roleName)
                .Select(p => new
                {
                    p.Id,
                    p.Endpoint,
                    p.HttpMethod,
                    p.Description
                })
                .ToListAsync();

            return Ok(permissions);
        }

        // 6. Delete permission
        [HttpDelete("permissions/{id}")]
        public async Task<IActionResult> RemovePermission(int id)
        {
            var permission = await _dbContext.tblRoleApiPermissions.FindAsync(id);
            if (permission == null)
            {
                return NotFound();
            }

            _dbContext.tblRoleApiPermissions.Remove(permission);
            await _dbContext.SaveChangesAsync();

            return Ok(new { Message = "Permission removed successfully" });
        }

        // 7. Get user roles
        [HttpGet("user/{email}")]
        public async Task<IActionResult> GetUserRoles(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            var roles = await _userManager.GetRolesAsync(user);
            return Ok(roles);
        }

        // 8. Remove role from user
        [HttpDelete("user/{email}/role/{roleName}")]
        public async Task<IActionResult> RemoveRoleFromUser(string email, string roleName)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            if (result.Succeeded)
            {
                return Ok(new { Message = "Role removed from user successfully" });
            }

            return BadRequest(new { Errors = result.Errors });
        }
    }

    public class RoleApiPermissionDto
    {
        public string RoleName { get; set; }
        public string Endpoint { get; set; }
        public string HttpMethod { get; set; }
        public string Description { get; set; }
    }
}
