using DymanicRolesBasedAuthorization.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DymanicRolesBasedAuthorization.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUsers>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<RoleApiPermission> tblRoleApiPermissions { get; set; }
        public DbSet<ApiEndpoint> tblApiEndpoints { get; set; }
        public DbSet<Product> tblProduct { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<RoleApiPermission>()
                .HasIndex(r => new { r.RoleName, r.Endpoint, r.HttpMethod })
                .IsUnique();
        }
    }
}
