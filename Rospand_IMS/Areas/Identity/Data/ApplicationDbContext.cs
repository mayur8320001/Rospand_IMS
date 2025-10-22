using Microsoft.EntityFrameworkCore;
using Rospand_IMS.Models;
using Rospand_IMS.Models.Account;  // adjust namespaces

namespace Rospand_IMS.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Your DbSets...
        public DbSet<Ledger> Ledgers { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<SalesInvoice> SalesInvoices { get; set; }
        public DbSet<SalesInvoiceItem> SalesInvoiceItems { get; set; }
        public DbSet<DailyExpense> DailyExpenses { get; set; }

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
        public DbSet<SalesOrder> SalesOrders { get; set; }
        public DbSet<SalesOrderItem> SalesOrderItems { get; set; }
        public DbSet<OutwardEntry> OutwardEntries { get; set; }
        public DbSet<OutwardEntryItem> OutwardEntryItems { get; set; }
        public DbSet<GoodsReceipt> GoodsReceipts { get; set; }

        // User Management DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<RoleMaster> RoleMasters { get; set; }
        public DbSet<PageAccess> PageAccesses { get; set; }
        public DbSet<Menu> Menus { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure account entities primary keys
            modelBuilder.Entity<DailyExpense>()
                .HasKey(d => d.ExpenseId);

            modelBuilder.Entity<Ledger>()
                .HasKey(l => l.LedgerId);

            modelBuilder.Entity<Transaction>()
                .HasKey(t => t.TransactionId);

            modelBuilder.Entity<SalesInvoice>()
                .HasKey(s => s.InvoiceId);

            modelBuilder.Entity<SalesInvoiceItem>()
                .HasKey(s => s.InvoiceItemId);

            // Configure relationships
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Ledger)
                .WithMany(l => l.LedgerTransactions)
                .HasForeignKey(t => t.LedgerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SalesInvoiceItem>()
                .HasOne(sii => sii.Invoice)
                .WithMany(si => si.Items)
                .HasForeignKey(sii => sii.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<State>()
                .HasIndex(s => s.StateId)
                .IsUnique();

            modelBuilder.Entity<City>()
                .HasOne(c => c.State)
                .WithMany(s => s.Cities)
                .HasForeignKey(c => c.StateId)
                .HasPrincipalKey(s => s.StateId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductComponent>()
                .HasOne(pc => pc.ParentProduct)
                .WithMany(p => p.Components)
                .HasForeignKey(pc => pc.ParentProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductComponent>()
                .HasOne(pc => pc.ComponentProduct)
                .WithMany()
                .HasForeignKey(pc => pc.ComponentProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Vendor>()
                .HasOne(v => v.BillingAddress)
                .WithMany(a => a.BillingVendors)
                .HasForeignKey(v => v.BillingAddressId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Vendor>()
                .HasOne(v => v.ShippingAddress)
                .WithMany(a => a.ShippingVendors)
                .HasForeignKey(v => v.ShippingAddressId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Customer>()
                .HasOne(c => c.BillingAddress)
                .WithMany(a => a.BillingCustomers)
                .HasForeignKey(c => c.BillingAddressId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Customer>()
                .HasOne(c => c.ShippingAddress)
                .WithMany(a => a.ShippingCustomers)
                .HasForeignKey(c => c.ShippingAddressId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Configure User Management entities
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).HasMaxLength(50).IsRequired();
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.Role).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(100);
            });

            modelBuilder.Entity<RoleMaster>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RoleName).HasMaxLength(50).IsRequired();
            });

            modelBuilder.Entity<PageAccess>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PageName).HasMaxLength(100).IsRequired();
                
                entity.HasOne(d => d.Role)
                    .WithMany(p => p.PageAccesses)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            
            // Configure Menu entity
            modelBuilder.Entity<Menu>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.ControllerName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.ActionName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.IconClass).HasMaxLength(50);
            });
        }
    }
}