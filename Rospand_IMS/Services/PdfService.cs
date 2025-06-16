// Services/PdfService.cs
using System.IO;
using DinkToPdf;
using DinkToPdf.Contracts;

public class PdfService
{
    private readonly IConverter _converter;

    public PdfService(IConverter converter)
    {
        _converter = converter;
    }

    public byte[] GeneratePdfFromHtml(string htmlContent, string title)
    {
        var globalSettings = new GlobalSettings
        {
            ColorMode = ColorMode.Color,
            Orientation = Orientation.Portrait,
            PaperSize = PaperKind.A4,
            Margins = new MarginSettings { Top = 10, Bottom = 10, Left = 10, Right = 10 },
            DocumentTitle = title
        };

        var objectSettings = new ObjectSettings
        {
            PagesCount = true,
            HtmlContent = htmlContent,
            WebSettings = { DefaultEncoding = "utf-8" },
            HeaderSettings = { FontSize = 9, Right = "Page [page] of [toPage]", Line = true },
            FooterSettings = { FontSize = 9, Center = $"Generated on {DateTime.Now:yyyy-MM-dd}" }
        };

        var pdf = new HtmlToPdfDocument()
        {
            GlobalSettings = globalSettings,
            Objects = { objectSettings }
        };

        return _converter.Convert(pdf);
    }
}