using DymanicRolesBasedAuthorization.Data;
using DymanicRolesBasedAuthorization.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DymanicRolesBasedAuthorization.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public ProductController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpPost("AddProduct")]
        public async Task<ActionResult<Product>> AddProduct(Product product)
        {
            try
            {
                if (product == null)
                {
                    return Ok(new { status_message = "Product Data is Null", status_code = 0 });
                }

                product.IsActive = true;
                product.IsDeleted = false;
                product.CreatedDate = DateTime.Now;
                product.UpdatedDate = DateTime.Now;

                await context.tblProduct.AddAsync(product);
                await context.SaveChangesAsync();

                return Ok(new { status_message = "Product Added Successfully.", status_code = 1, product });
            }
            catch (Exception e)
            {
                return Ok(new { status_code = 0, status_message = "Sorry! Something went wrong.", e.Message });
            }
        }

        [HttpGet("GetProductBy/{id}")]
        public async Task<ActionResult<Product>> GetRecord(int id)
        {
            try
            {
                var data = await context.tblProduct.Where(e => e.IsDeleted == false && e.ID == id).FirstOrDefaultAsync();
                if (data == null)
                {
                    return Ok(new { status_code = 0, status_message = "Product Not Found." });
                }
                return Ok(new { ProductData = data, status_code = 1 });
            }
            catch (Exception e)
            {
                return Ok(new { status_code = 0, status_message = "Sorry! Something went wrong.", e.Message });
            }
        }

        [HttpGet("GetProducts")]
        public async Task<ActionResult<Product>> GetProducts()
        {
            try
            {
                var data = await context.tblProduct.Where(e => e.IsDeleted == false).ToListAsync();
                if (data == null)
                {
                    return Ok(new { status_code = 0, status_message = "Product Not Found." });
                }
                return Ok(new { ProductData = data, status_code = 1 });
            }
            catch (Exception e)
            {
                return Ok(new { status_code = 0, status_message = "Sorry! Something went wrong.", e.Message });
            }
        }
    }
}
