using Microsoft.EntityFrameworkCore;
using Rospand_IMS.Data;
using Rospand_IMS.Models;

namespace Rospand_IMS.Services
{
    public interface ISKUGenerator
    {
        Task<string> GenerateSKU(Product product);
    }

    public class SKUGenerator : ISKUGenerator
    {
        private readonly ApplicationDbContext _context;

        public SKUGenerator(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> GenerateSKU(Product product)
        {
            var categoryCode = product.CategoryId.HasValue
                ? (await _context.Categories.FindAsync(product.CategoryId))?.Name[..3].ToUpper() ?? "GEN"
                : "GEN";

            var typeCode = product.Type == ProductType.Goods ? "GDS" : "SRV";
            var lastId = await _context.Products.MaxAsync(p => (int?)p.Id) ?? 0;

            return $"{categoryCode}-{typeCode}-{(lastId + 1).ToString("D5")}";
        }
    }
}
