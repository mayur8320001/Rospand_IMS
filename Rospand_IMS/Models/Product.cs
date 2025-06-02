using System.ComponentModel.DataAnnotations;

namespace Rospand_IMS.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        [StringLength(100, ErrorMessage = "Product name cannot exceed 100 characters")]
        public string Name { get; set; }

        [Display(Name = "SKU Code")]
        [StringLength(50, ErrorMessage = "SKU cannot exceed 50 characters")]
        public string SKU { get; set; }

        [Required(ErrorMessage = "Product type is required")]
        public ProductType? Type { get; set; } = ProductType.Goods; // Enum instead of string

        // Goods-specific properties
        [Display(Name = "Unit of Measurement")]
        public int? UnitId { get; set; }
        public Unit? Unit { get; set; }

        [Display(Name = "Cost Category")]
        public int? CostCategoryId { get; set; }
        public CostCategory? CostCategory { get; set; }

        // Dimensions (optional)
        [Display(Name = "Length")]
        [Range(0, double.MaxValue, ErrorMessage = "Length must be positive")]
        public decimal? Length { get; set; }

        [Display(Name = "Width")]
        [Range(0, double.MaxValue, ErrorMessage = "Width must be positive")]
        public decimal? Width { get; set; }

        [Display(Name = "Height")]
        [Range(0, double.MaxValue, ErrorMessage = "Height must be positive")]
        public decimal? Height { get; set; }

        [Display(Name = "Dimension Unit")]
        public DimensionUnit? DimensionUnit { get; set; }
      

        [Display(Name = "Weight")]
        [Range(0, double.MaxValue, ErrorMessage = "Weight must be positive")]
        public decimal? Weight { get; set; }
       
        [Display(Name = "Weight Unit")]
        public WeightUnit? WeightUnit { get; set; } // Default to kg

        // Pricing
        [Display(Name = "Sales Price")]
        [Range(0, double.MaxValue, ErrorMessage = "Sales price must be positive")]
        public decimal? SalesPrice { get; set; }

        [Display(Name = "Purchase Price")]
        [Range(0, double.MaxValue, ErrorMessage = "Purchase price must be positive")]
        public decimal? PurchasePrice { get; set; }

        // Service-specific
         [Display(Name = "Service Duration")]
        [Range(0, double.MaxValue, ErrorMessage = "Duration must be positive")]
        public decimal? ServiceDuration { get; set; }

        [Display(Name = "Duration Unit")]
        public ServiceDurationUnit? ServiceDurationUnit { get; set; }

        // Common properties
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [Display(Name = "Category")]
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }


        public bool IsInventoryTracked { get; set; } = true;

        // Grouped product
        public bool IsGroupedProduct { get; set; }
        public ICollection<ProductComponent> Components { get; set; } = new List<ProductComponent>();

        // Automatic fields
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }

    public enum ProductType
    {
        Goods,
        Services
    }
    public enum DimensionUnit
    {
        [Display(Name = "cm")]
        Centimeter,
        [Display(Name = "m")]
        Meter,
        [Display(Name = "ft")]
        Feet,
        [Display(Name = "in")]
        Inch
    }
    public enum ServiceDurationUnit
    {
        [Display(Name = "Hours")]
        Hours,
        [Display(Name = "Days")]
        Days,
        [Display(Name = "Weeks")]
        Weeks,
        [Display(Name = "Months")]
        Months,
        [Display(Name = "Years")]
        Years
    }

    public enum WeightUnit
    {
        [Display(Name = "kg")]
        Kg,
        [Display(Name = "g")]
        Gram,
        [Display(Name = "lb")]
        Pound,
        [Display(Name = "oz")]
        Ounce
    }

    public class ProductComponent
    {
        public int Id { get; set; }
        public int ParentProductId { get; set; }
        public Product ParentProduct { get; set; }
        public int ComponentProductId { get; set; }
        public Product ComponentProduct { get; set; }
        public int Quantity { get; set; } = 1;
        public string Notes { get; set; }
    }



}