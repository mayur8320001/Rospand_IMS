/*using iText.Kernel.Pdf;
using iText.Layout.Element;
using iText.Layout.Properties;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Previewer;
using Rospand_IMS.Models;



namespace Rospand_IMS.Services
{
    public interface IPdfService
    {
        Task<byte[]> GenerateInvoicePdf(Invoice invoice);
    }

    public class PdfService : IPdfService
    {
        public async Task<byte[]> GenerateInvoicePdf(Invoice invoice)
        {
            using var memoryStream = new MemoryStream();
            var writer = new PdfWriter(memoryStream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            // Add content
            document.Add(new Paragraph($"INVOICE #{invoice.InvoiceNumber}")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(24)
                .SetBold());

            // Add invoice details
            var table = new Table(new float[] { 1, 1 });
            table.AddCell("Date: " + invoice.InvoiceDate.ToString("dd/MM/yyyy"));
            table.AddCell("Customer: " + invoice.SalesOrder.Customer.CustomerDisplayName");
    
            table.AddCell("Due Date: " + invoice.DueDate.ToString("dd/MM/yyyy"));
            table.AddCell("Sales Order: " + invoice.SalesOrder.SONumber);
            document.Add(table);

            // Add line items table
            var itemsTable = new Table(new float[] { 1, 3, 1, 1, 1, 1 });
            itemsTable.AddHeaderCell("#");
            itemsTable.AddHeaderCell("Product");
            itemsTable.AddHeaderCell("Qty").SetTextAlignment(TextAlignment.RIGHT);
            // ... add more headers and data rows ...

            document.Add(itemsTable);

            document.Close();
            return memoryStream.ToArray();
        }
    }
}*/