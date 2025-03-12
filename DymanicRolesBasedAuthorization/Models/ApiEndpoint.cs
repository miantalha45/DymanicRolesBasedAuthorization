namespace DymanicRolesBasedAuthorization.Models
{
    public class ApiEndpoint
    {
        public int Id { get; set; }
        public string Path { get; set; } = null!;
        public string HttpMethod { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
    }
}
