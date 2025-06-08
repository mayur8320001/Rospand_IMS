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

    public class EditProductViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public ProductType SelectedType { get; set; } = ProductType.Goods;

        [Required(ErrorMessage = "Product name is required")]
        [StringLength(100, ErrorMessage = "Product name cannot exceed 100 characters")]
        public string Name { get; set; }

        [Display(Name = "Generate SKU automatically")]
        public bool AutoGenerateSKU { get; set; } = false;

        [Display(Name = "SKU Code")]
        [StringLength(50, ErrorMessage = "SKU cannot exceed 50 characters")]
        public string? SKU { get; set; }

        // Goods fields
        [Display(Name = "Unit of Measurement")]
        public int? UnitId { get; set; }

        [Display(Name = "Cost Category")]
        public int? CostCategoryId { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Sales price must be positive")]
        [Display(Name = "Sales Price")]
        public decimal? SalesPrice { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Purchase price must be positive")]
        [Display(Name = "Purchase Price")]
        public decimal? PurchasePrice { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Length must be positive")]
        public decimal? Length { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Width must be positive")]
        public decimal? Width { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Height must be positive")]
        public decimal? Height { get; set; }

        [Display(Name = "Dimension Unit")]
        public DimensionUnit DimensionUnit { get; set; } = DimensionUnit.Centimeter;

        [Range(0, double.MaxValue, ErrorMessage = "Weight must be positive")]
        public decimal? Weight { get; set; }

        [Display(Name = "Weight Unit")]
        public WeightUnit? WeightUnit { get; set; }

        // Service fields
        [Range(0, double.MaxValue, ErrorMessage = "Service duration must be positive")]
        [Display(Name = "Service Duration")]
        public decimal? ServiceDuration { get; set; }

        [Display(Name = "Duration Unit")]
        public ServiceDurationUnit? ServiceDurationUnit { get; set; }

        // Common fields
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [Display(Name = "Category")]
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
}
