namespace DymanicRolesBasedAuthorization.Models
{
    public class Product
    {
        public int ID { get; set; }
        public string Name { get; set; } = null!;
        public string? Type { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
    }
}
