using Microsoft.AspNetCore.Identity;

namespace DymanicRolesBasedAuthorization.Data
{
    public class ApplicationUsers : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
