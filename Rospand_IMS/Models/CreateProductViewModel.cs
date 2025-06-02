using System.ComponentModel.DataAnnotations;

namespace Rospand_IMS.Models
{
    public class CreateProductViewModel
    {
        [Required]
        public ProductType SelectedType { get; set; } = ProductType.Goods;

        [Required]
        public string Name { get; set; }

        [Display(Name = "Generate SKU automatically")]
        public bool AutoGenerateSKU { get; set; } = true;

        [Display(Name = "SKU Code")]
        public string? SKU { get; set; }

        // Goods fields
        public int? UnitId { get; set; }
        public int? CostCategoryId { get; set; }
        public decimal? SalesPrice { get; set; }
        public decimal? PurchasePrice { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }

        [Display(Name = "Dimension Unit")]
        public DimensionUnit DimensionUnit { get; set; } = DimensionUnit.Centimeter;
        public decimal? Weight { get; set; }
        public WeightUnit? WeightUnit { get; set; }

        // Service fields
       
        public decimal? ServiceDuration { get; set; }

        [Display(Name = "Duration Unit")]
        public ServiceDurationUnit? ServiceDurationUnit { get; set; }
        // Common fields
        public string? Description { get; set; }
        public int? CategoryId { get; set; }

        // Grouped product
        public bool IsGroupedProduct { get; set; }
        public List<ProductComponentViewModel> Components { get; set; } = new();

        // Dropdown options
        public List<Unit> Units { get; set; } = new();
        public List<CostCategory> CostCategories { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public List<Product> AvailableProducts { get; set; } = new();
    }
    
    public class ProductComponentViewModel
    {
        public int ComponentProductId { get; set; }
        public int Quantity { get; set; } = 1;
        public string Notes { get; set; }
        public string ProductName { get; set; }
        public string SKU { get; set; }
    }
    public class ProductIndexViewModel
    {
        public List<Product> Products { get; set; }
        public List<Category> Categories { get; set; }
        public string SearchString { get; set; }
        public ProductType? SelectedType { get; set; }
        public int? SelectedCategoryId { get; set; }
    }
}
