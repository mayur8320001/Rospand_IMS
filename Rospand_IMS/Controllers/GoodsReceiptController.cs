using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rospand_IMS.Data;
using Rospand_IMS.Models;

using System.Linq;
using System.Threading.Tasks;

namespace Rospand_IMS.Controllers
{
    [Authorize]
    public class GoodsReceiptController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GoodsReceiptController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: GoodsReceipt/Create/5 (for PO ID)
        public IActionResult Create(int purchaseOrderId)
        {
            var po = _context.PurchaseOrders
                .Include(po => po.Vendor)
                .FirstOrDefault(po => po.Id == purchaseOrderId);

            if (po == null)
            {
                return NotFound();
            }

            var viewModel = new GoodsReceiptCreateViewModel
            {
                PurchaseOrderId = po.Id,
                PONumber = po.PONumber,
                VendorName = po.Vendor.VendorDisplayName,
                GRNumber = GoodsReceipt.GenerateGRNumber(), // Set the GRNumber here
                ReceiptDate = DateTime.Today,
                Warehouses = _context.Warehouses.ToList(),
                Items = po.Items.Select(item => new GoodsReceiptItemViewModel
                {
                    PurchaseOrderItemId = item.Id,
                    ProductId = item.ProductId,
                    ProductName = item.Product.Name,
                    OrderedQuantity = item.Quantity,
                    ReceivedQuantity = item.Quantity - item.ReceivedQuantity,
                    UnitPrice = item.UnitPrice
                }).ToList()
            };

            return View(viewModel);
        }
        // POST: GoodsReceipt/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GoodsReceiptCreateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var goodsReceipt = new GoodsReceipt
                {
                    GRNumber = GoodsReceipt.GenerateGRNumber(), // Generate here
                    ReceiptDate = viewModel.ReceiptDate,
                    PurchaseOrderId = viewModel.PurchaseOrderId,
                    Notes = viewModel.Notes,
                    Status = GoodsReceiptStatus.Pending,
                    Items = viewModel.Items.Select(item => new GoodsReceiptItem
                    {
                        PurchaseOrderItemId = item.PurchaseOrderItemId,
                        QuantityReceived = item.QuantityReceived,
                        UnitPrice = item.UnitPrice,
                        BatchNumber = item.BatchNumber,
                        ExpiryDate = item.ExpiryDate,
                        Notes = item.Notes
                    }).ToList()
                };

                _context.Add(goodsReceipt);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Repopulate view model if validation fails
            viewModel.Warehouses = _context.Warehouses.ToList();
            return View(viewModel);
        }
        private async Task UpdateInventory(int productId, int quantity, int warehouseId,
            string referenceNumber, string batchNumber, DateTime? expiryDate)
        {
            // Find or create inventory record
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.ProductId == productId && i.WarehouseId == warehouseId);

            if (inventory == null)
            {
                inventory = new Inventory
                {
                    ProductId = productId,
                    WarehouseId = warehouseId,
                    QuantityOnHand = quantity,
                    QuantityReserved = 0
                };
                _context.Add(inventory);
            }
            else
            {
                inventory.QuantityOnHand += quantity;
                _context.Update(inventory);
            }

            // Create transaction record
            var transaction = new InventoryTransaction
            {
                ProductId = productId,
                WarehouseId = warehouseId,
                TransactionType = InventoryTransactionType.Purchase,
                Quantity = quantity,
                ReferenceNumber = referenceNumber,
                Notes = $"Batch: {batchNumber}, Expiry: {expiryDate?.ToShortDateString()}"
            };

            _context.Add(transaction);
        }

        // GET: GoodsReceipt/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var goodsReceipt = await _context.GoodsReceipts
                .Include(gr => gr.PurchaseOrder)
                    .ThenInclude(po => po.Vendor)
                .Include(gr => gr.Items)
                    .ThenInclude(i => i.PurchaseOrderItem)
                        .ThenInclude(poi => poi.Product)
                .FirstOrDefaultAsync(gr => gr.Id == id);

            if (goodsReceipt == null)
            {
                return NotFound();
            }

            return View(goodsReceipt);
        }
    }
}