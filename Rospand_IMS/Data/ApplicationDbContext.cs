using Microsoft.EntityFrameworkCore;
using Rospand_IMS.Models;

using System.Data;


namespace Rospand_IMS.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
           : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Vendor> Vendors { get; set; }


        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }

        public DbSet<Payment> Payments { get; set; }



        public DbSet<PaymentTerm> PaymentTerms { get; set; }
        public DbSet<Tax> Taxes { get; set; }
   
        public DbSet<Unit> Units { get; set; }
        public DbSet<CostCategory> CostCategories { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<State> States { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<TaxType> TaxTypes { get; set; }
        public DbSet<Address> Addresses { get; set; }
        
       public DbSet<Inventory> Inventories { get; set; }
       public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
       public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }
       public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
       public DbSet<Warehouse> Warehouses { get; set; }


        /*   public DbSet<DeliveryChallan> DeliveryChallans { get; set; }
         *   
         *   public DbSet<DeliveryChallanItem> DeliveryChallanItems { get; set; }*/
        public DbSet<SalesOrder> SalesOrders { get; set; }

        public DbSet<SalesOrderItem> SalesOrderItems { get; set; }


        public DbSet<OutwardEntry> OutwardEntries { get; set; }
        public DbSet<OutwardEntryItem> OutwardEntryItems { get; set; }

        public DbSet<GoodsReceipt> GoodsReceipts { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ProductComponent>()
                .HasOne(pc => pc.ParentProduct)
                .WithMany(p => p.Components)
                .HasForeignKey(pc => pc.ParentProductId)
                .OnDelete(DeleteBehavior.Restrict); // or .Cascade / .NoAction depending on your preference

            modelBuilder.Entity<ProductComponent>()
                .HasOne(pc => pc.ComponentProduct)
                .WithMany()
                .HasForeignKey(pc => pc.ComponentProductId)
                .OnDelete(DeleteBehavior.Restrict); // prevent cycles or multiple cascade paths
        }

    }
}
