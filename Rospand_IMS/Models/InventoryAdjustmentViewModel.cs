using System.ComponentModel.DataAnnotations;

namespace Rospand_IMS.Models
{
    public class InventoryAdjustmentViewModel
    {
        [Required]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [Required]
        [Display(Name = "Warehouse")]
        public int WarehouseId { get; set; }

        [Required]
        [Display(Name = "Adjustment Type")]
        public int AdjustmentType { get; set; } // 1 for increase, -1 for decrease

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [Required]
        [Display(Name = "Reason")]
        public string Reason { get; set; }

        [Display(Name = "Notes")]
        public string Notes { get; set; }
    }

    public class StockTransferViewModel
    {
        [Required]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [Required]
        [Display(Name = "From Warehouse")]
        public int FromWarehouseId { get; set; }

        [Required]
        [Display(Name = "To Warehouse")]
        public int ToWarehouseId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [Display(Name = "Reference Number")]
        public string ReferenceNumber { get; set; }

        [Display(Name = "Notes")]
        public string Notes { get; set; }
    }
}
