using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rospand_IMS.Models
{
    public class OutwardEntryCreateViewModel
    {
        [Required(ErrorMessage = "Sales Order ID is required")]
        public int SalesOrderId { get; set; }

        [Required(ErrorMessage = "Sales Order Number is required")]
        [StringLength(20, ErrorMessage = "Sales Order Number cannot exceed 20 characters")]
        public string SONumber { get; set; }

        [Required(ErrorMessage = "Customer Name is required")]
        [StringLength(100, ErrorMessage = "Customer Name cannot exceed 100 characters")]
        public string CustomerDisplayName { get; set; }

        [Required(ErrorMessage = "Outward Number is required")]
        [StringLength(20, ErrorMessage = "Outward Number cannot exceed 20 characters")]
        public string OutwardNumber { get; set; }

        [Required(ErrorMessage = "Outward Date is required")]
        [DataType(DataType.Date)]
        public DateTime OutwardDate { get; set; }

        [Required(ErrorMessage = "Warehouse is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid warehouse")]
        public int WarehouseId { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string Notes { get; set; }

        [Required(ErrorMessage = "At least one item is required")]
        [MinLength(1, ErrorMessage = "At least one item is required")]
        public List<OutwardEntryItemViewModel> Items { get; set; } = new List<OutwardEntryItemViewModel>();

        [BindNever]
        public SelectList? Warehouses { get; set; }

    }

    public class OutwardEntryItemViewModel
    {
        [Required(ErrorMessage = "Product ID is required")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Product Name is required")]
        [StringLength(100, ErrorMessage = "Product Name cannot exceed 100 characters")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Product SKU is required")]
        [StringLength(50, ErrorMessage = "Product SKU cannot exceed 50 characters")]
        public string ProductSKU { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity Ordered must be greater than 0")]
        public int QuantityOrdered { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Quantity Dispatched cannot be negative")]
        public int QuantityDispatched { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Quantity Available cannot be negative")]
        public int QuantityAvailable { get; set; }

        [Required(ErrorMessage = "Dispatch Quantity is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Dispatch Quantity cannot be negative")]
        public int Quantity { get; set; }
  

        [StringLength(200, ErrorMessage = "Notes cannot exceed 200 characters")]
        public string Notes { get; set; }
    }
    public class SalesOrderDetailsViewModel
    {
        public SalesOrder SalesOrder { get; set; }
        public int TotalOrdered { get; set; }
        public int TotalDispatched { get; set; }
        public int RemainingToDispatch { get; set; }
    }


    public class MarkAsDeliveredViewModel
    {
        public int OutwardEntryId { get; set; }
        public string OutwardNumber { get; set; }
        public DateTime DeliveryDate { get; set; } = DateTime.Today;
        public string Notes { get; set; }
       // public string DeliveredBy { get; set; }
    }

}
