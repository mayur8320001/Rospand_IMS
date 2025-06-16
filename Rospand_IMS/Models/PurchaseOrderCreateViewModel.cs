using Rospand_IMS.Models;
using System.ComponentModel.DataAnnotations;

namespace Rospand_IMS.Models
{


    public class PurchaseOrderIndexViewModel
    {
        public List<PurchaseOrder> PurchaseOrders { get; set; }
        public int PageIndex { get; set; }
        public int TotalPages { get; set; }
        public string Status { get; set; }
        public string SortBy { get; set; }
        public string SortOrder { get; set; }
        public string SearchString { get; set; }

    }


    public class PurchaseOrderCreateViewModel
    {
        public int Id { get; set; }

        [Required]
        public string PONumber { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime OrderDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime ExpectedDeliveryDate { get; set; }

        [Required]
        [Display(Name = "Vendor")]
        public int VendorId { get; set; }

        [Required]
        [Display(Name = "Currency")]
        public int CurrencyId { get; set; }

        public string Notes { get; set; }

        public List<Category> Categories { get; set; } = new List<Category>();

        // Add this property for selected category filter
        public int? SelectedCategoryId { get; set; }
        public List<PurchaseOrderItemViewModel> Items { get; set; } = new List<PurchaseOrderItemViewModel>();

        // Dropdown options
        public List<Vendor> Vendors { get; set; } = new List<Vendor>();
        public List<Currency> Currencies { get; set; } = new List<Currency>();
        public List<Product> Products { get; set; } = new List<Product>();
    }

    public class PurchaseOrderItemViewModel
    {
        public int Id { get; set; }
        [Display(Name = "Category")]
        public int CategoryId { get; set; }
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        [Range(0, 100)]
        public decimal DiscountPercent { get; set; }

        public decimal TaxRate { get; set; }

        public string Notes { get; set; }
    }
}