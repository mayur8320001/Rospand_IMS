using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Rospand_IMS.Models
{
    public class SalesOrderCreateViewModel
    {
        public int Id { get; set; }

        [Required]
        public string SONumber { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        public DateTime? ExpectedDeliveryDate { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        public int CurrencyId { get; set; }

        public string Notes { get; set; }

        public List<SalesOrderItemViewModel> Items { get; set; } = new List<SalesOrderItemViewModel>();

        // Dropdowns
        public List<Customer> Customers { get; set; } = new List<Customer>();
        public List<Currency> Currencies { get; set; } = new List<Currency>();
        public List<InventoryViewModel> AvailableProducts { get; set; } = new List<InventoryViewModel>();
    }

    public class SalesOrderIndexViewModel
    {
        public List<SalesOrder> SalesOrders { get; set; }

        [Display(Name = "Search")]
        public string SearchString { get; set; }

        [Display(Name = "Status")]
        public SalesOrderStatus? Status { get; set; }
        public int PageIndex { get; set; }
        public int TotalPages { get; set; }
    }


    public class SalesOrderItemViewModel
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal TaxRate { get; set; }
        public string Notes { get; set; }
        public string ProductName { get; set; }
        public string ProductSKU { get; set; }
        public int QuantityAvailable { get; set; }
    }
}