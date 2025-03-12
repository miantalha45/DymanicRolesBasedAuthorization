using System.Text.Json.Serialization;

namespace DymanicRolesBasedAuthorization.Models
{
    public class SignUp
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        [JsonIgnore]
        public bool IsActive { get; set; }
        [JsonIgnore]
        public bool IsDeleted { get; set; }
        [JsonIgnore]
        public DateTime CreatedDate { get; set; }
    }
}
