﻿using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Rospand_IMS.Models
{
    public class ReceivePurchaseOrderViewModel
    {
        public int PurchaseOrderId { get; set; }
        public string PONumber { get; set; }
        public List<ReceiveItemViewModel> Items { get; set; } = new List<ReceiveItemViewModel>();
    }

    public class ReceiveOrderViewModel
    {
        public int? VendorId { get; set; }
        public int? PurchaseOrderId { get; set; }
        public int? WarehouseId { get; set; } // New field for warehouse selection
        public List<SelectListItem> Vendors { get; set; }
        public List<SelectListItem> PurchaseOrders { get; set; }
        public List<SelectListItem> Warehouses { get; set; } // New dropdown for warehouses
        public PurchaseOrder SelectedPurchaseOrder { get; set; }
        public Dictionary<int, int> ReceivedQuantities { get; set; }
    }
    public class ReceiveItemViewModel
    {
        public int ItemId { get; set; }
        public string ProductName { get; set; }
        public int OrderedQuantity { get; set; }
        public int PreviouslyReceived { get; set; }
        public int RemainingToReceive { get; set; }
        [Range(0, int.MaxValue)]
        public int ReceivedQuantity { get; set; }
    }
}
