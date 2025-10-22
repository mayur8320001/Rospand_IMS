namespace Rospand_IMS.Models.Account
{
    public class SalesInvoice
    {
        public int InvoiceId { get; set; }
        public int CustomerId { get; set; }  // FK to Customer Table
        public string InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public decimal TotalAmount { get; set; }

        // Navigation Property
        public virtual ICollection<SalesInvoiceItem> Items { get; set; }
    }

    public class SalesInvoiceItem
    {
        public int InvoiceItemId { get; set; }
        public int InvoiceId { get; set; }
        public virtual SalesInvoice Invoice { get; set; }

        public int ProductId { get; set; }   // FK to Product Table
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }

}
