namespace DymanicRolesBasedAuthorization.Models
{
    public class RoleApiPermission
    {
        public int Id { get; set; }
        public string RoleName { get; set; } = null!;
        public string Endpoint { get; set; } = null!;
        public string HttpMethod { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
    }
}
