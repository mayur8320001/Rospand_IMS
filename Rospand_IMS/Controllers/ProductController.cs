using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rospand_IMS.Data;
using Rospand_IMS.Models;
using Rospand_IMS.Services;
using System.Linq;
using System.Threading.Tasks;

namespace Rospand_IMS.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ISKUGenerator _skuGenerator;

        public ProductController(ApplicationDbContext context, ISKUGenerator skuGenerator)
        {
            _context = context;
            _skuGenerator = skuGenerator;
        }
        // Add to ProductController.cs
        [HttpGet]
        public async Task<IActionResult> Index(string searchString, ProductType? productType, int? categoryId)
        {
            var products = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Unit)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p =>
                    p.Name.Contains(searchString) ||
                    p.SKU.Contains(searchString) ||
                    (p.Description != null && p.Description.Contains(searchString)));
            }

            if (productType.HasValue)
            {
                products = products.Where(p => p.Type == productType.Value);
            }

            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value);
            }

            var model = new ProductIndexViewModel
            {
                Products = await products.ToListAsync(),
                Categories = await _context.Categories.ToListAsync(),
                SearchString = searchString,
                SelectedType = productType,
                SelectedCategoryId = categoryId
            };

            return View(model);
        }


        // Add to ProductController.cs
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Unit)
                .Include(p => p.CostCategory)
                .Include(p => p.Components)
                    .ThenInclude(c => c.ComponentProduct)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }


        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new CreateProductViewModel
            {
                Units = await _context.Units.ToListAsync(),
                CostCategories = await _context.CostCategories.ToListAsync(),
                Categories = await _context.Categories.ToListAsync(),
                AvailableProducts = await _context.Products
                    .Where(p => !p.IsGroupedProduct)
                    .ToListAsync()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Units = await _context.Units.ToListAsync();
                model.CostCategories = await _context.CostCategories.ToListAsync();
                model.Categories = await _context.Categories.ToListAsync();
                model.AvailableProducts = await _context.Products
                    .Where(p => !p.IsGroupedProduct)
                    .ToListAsync();
                return View(model);
            }

            var product = new Product
            {
                Name = model.Name,
                Type = model.SelectedType,
                Description = model.Description,
                Length = model.Length,
                Width = model.Width,
                Height = model.Height,
                DimensionUnit = model.DimensionUnit,
                CategoryId = model.CategoryId,
                IsGroupedProduct = model.IsGroupedProduct,
                SalesPrice = model.SalesPrice,
                PurchasePrice = model.PurchasePrice
            };

            // SKU Generation
            product.SKU = model.AutoGenerateSKU
                ? await _skuGenerator.GenerateSKU(product)
                : model.SKU;

            if (model.SelectedType == ProductType.Goods)
            {
                product.UnitId = model.UnitId;
                product.CostCategoryId = model.CostCategoryId;
                product.SalesPrice = model.SalesPrice;
                product.PurchasePrice = model.PurchasePrice;
                product.Length = model.Length;
                product.Width = model.Width;
                product.Height = model.Height;
                product.Weight = model.Weight;
                product.WeightUnit = model.WeightUnit;
            }
            else
            {
                product.ServiceDuration = model.ServiceDuration;
            }
            if (model.SelectedType == ProductType.Goods)
            {
                if (model.UnitId == null)
                    ModelState.AddModelError(nameof(model.UnitId), "Unit is required for Goods.");
                // Add similar checks for CostCategoryId, SalesPrice, etc.
            }
            else if (model.SelectedType == ProductType.Services)
            {
                if (model.ServiceDuration == null)
                    ModelState.AddModelError(nameof(model.ServiceDuration), "Service Duration is required.");
            }

            // Handle grouped product components
            if (model.IsGroupedProduct)
            {
                foreach (var component in model.Components)
                {
                    product.Components.Add(new ProductComponent
                    {
                        ComponentProductId = component.ComponentProductId,
                        Quantity = component.Quantity,
                        Notes = component.Notes
                    });
                }
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = product.Id });
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products
                .Include(p => p.Components)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            var model = new EditProductViewModel
            {
                Id = product.Id,
                Name = product.Name,
                SKU = product.SKU,
                AutoGenerateSKU = false, // Assume existing SKU is not auto-generated
                SelectedType = product.Type ?? ProductType.Goods,
                Description = product.Description,
                UnitId = product.UnitId,
                CostCategoryId = product.CostCategoryId,
                SalesPrice = product.SalesPrice,
                PurchasePrice = product.PurchasePrice,
                Length = product.Length,
                Width = product.Width,
                Height = product.Height,
                DimensionUnit = product.DimensionUnit ?? DimensionUnit.Centimeter,
                Weight = product.Weight,
                WeightUnit = product.WeightUnit,
                ServiceDuration = product.ServiceDuration,
                ServiceDurationUnit = product.ServiceDurationUnit,
                CategoryId = product.CategoryId,
                IsGroupedProduct = product.IsGroupedProduct,
                Components = product.Components.Select(c => new ProductComponentViewModel
                {
                    ComponentProductId = c.ComponentProductId,
                    Quantity = c.Quantity,
                    Notes = c.Notes,
                    ProductName = c.ComponentProduct?.Name,
                    SKU = c.ComponentProduct?.SKU
                }).ToList(),
                Units = await _context.Units.ToListAsync(),
                CostCategories = await _context.CostCategories.ToListAsync(),
                Categories = await _context.Categories.ToListAsync(),
                AvailableProducts = await _context.Products
                    .Where(p => !p.IsGroupedProduct && p.Id != product.Id)
                    .ToListAsync()
            };

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Units = await _context.Units.ToListAsync();
                model.CostCategories = await _context.CostCategories.ToListAsync();
                model.Categories = await _context.Categories.ToListAsync();
                model.AvailableProducts = await _context.Products
                    .Where(p => !p.IsGroupedProduct && p.Id != model.Id)
                    .ToListAsync();
                return View(model);
            }

            var product = await _context.Products
                .Include(p => p.Components)
                .FirstOrDefaultAsync(p => p.Id == model.Id);

            if (product == null)
            {
                return NotFound();
            }

            product.Name = model.Name;
            product.Description = model.Description;
            product.Type = model.SelectedType;
            product.SKU = model.AutoGenerateSKU
                ? await _skuGenerator.GenerateSKU(product)
                : model.SKU;
            product.CategoryId = model.CategoryId;
            product.IsGroupedProduct = model.IsGroupedProduct;
            product.Length = model.Length;
            product.Width = model.Width;
            product.Height = model.Height;
            product.DimensionUnit = model.DimensionUnit;
            product.Weight = model.Weight;
            product.WeightUnit = model.WeightUnit;
            product.SalesPrice = model.SalesPrice;
            product.PurchasePrice = model.PurchasePrice;

            if (model.SelectedType == ProductType.Goods)
            {
                product.UnitId = model.UnitId;
                product.CostCategoryId = model.CostCategoryId;
            }
            else
            {
                product.ServiceDuration = model.ServiceDuration;
                product.ServiceDurationUnit = model.ServiceDurationUnit;
            }

            // Update Components
            product.Components.Clear(); // Remove old ones
            if (model.IsGroupedProduct)
            {
                foreach (var comp in model.Components)
                {
                    product.Components.Add(new ProductComponent
                    {
                        ComponentProductId = comp.ComponentProductId,
                        Quantity = comp.Quantity,
                        Notes = comp.Notes
                    });
                }
            }

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = product.Id });
        }


    }

}