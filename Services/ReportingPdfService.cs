using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AkarangaBank.Web.Services;

public sealed class ReportingPdfService
{
    static ReportingPdfService() => QuestPDF.Settings.License = LicenseType.Community;

    public byte[] Generate(string title, string subtitle, IReadOnlyList<string> headers, IEnumerable<IReadOnlyList<string>> rows, bool a5 = false)
    {
        var data = rows.ToList();
        return Document.Create(document => document.Page(page =>
        {
            page.Size(a5 ? PageSizes.A5 : PageSizes.A4);
            page.Margin(a5 ? 22 : 30);
            page.DefaultTextStyle(s => s.FontFamily("Arial").FontSize(9));
            page.Header().Column(column =>
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().Text("AKARANGA BANK").FontSize(10).Bold().FontColor("#C9A227");
                    row.ConstantItem(90).AlignRight().Text(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                });
                column.Item().PaddingTop(7).Text(title).FontSize(18).Bold().FontColor("#1F3864");
                column.Item().Text(subtitle).FontColor("#555555");
                column.Item().PaddingTop(8).LineHorizontal(1).LineColor("#C9A227");
            });
            page.Content().PaddingVertical(14).Table(table =>
            {
                table.ColumnsDefinition(columns => { foreach (var _ in headers) columns.RelativeColumn(); });
                table.Header(header =>
                {
                    foreach (var h in headers)
                        header.Cell().Background("#1F3864").Padding(5).Text(h).FontColor(Colors.White).Bold();
                });
                foreach (var item in data)
                    foreach (var value in item)
                        table.Cell().BorderBottom(0.5f).BorderColor("#D9E0EA").Padding(5).Text(value ?? string.Empty);
            });
            page.Footer().AlignCenter().DefaultTextStyle(s => s.FontSize(8).FontColor("#666666")).Text(text =>
            {
                text.Span("Document généré par AkarangaBank — Page ");
                text.CurrentPageNumber(); text.Span(" / "); text.TotalPages();
            });
        })).GeneratePdf();
    }

    public static string MoneyInWords(decimal amount, string currency)
    {
        var value = decimal.Round(Math.Abs(amount), 2, MidpointRounding.AwayFromZero);
        var whole = (long)decimal.Truncate(value);
        var cents = (int)Math.Round((value - whole) * 100, MidpointRounding.AwayFromZero);
        return $"{(amount < 0 ? "moins " : "")}{FrenchInteger(whole)} {currency}{(cents > 0 ? $" et {FrenchInteger(cents)} centimes" : "")}";
    }

    private static string FrenchInteger(long value)
    {
        if (value == 0) return "zéro";
        if (value < 0) return "moins " + FrenchInteger(-value);
        if (value >= 1_000_000_000) return FrenchInteger(value / 1_000_000_000) + " milliard" + (value / 1_000_000_000 > 1 ? "s " : " ") + (value % 1_000_000_000 == 0 ? "" : FrenchInteger(value % 1_000_000_000));
        if (value >= 1_000_000) return FrenchInteger(value / 1_000_000) + " million" + (value / 1_000_000 > 1 ? "s " : " ") + (value % 1_000_000 == 0 ? "" : FrenchInteger(value % 1_000_000));
        if (value >= 1000) return (value / 1000 == 1 ? "mille" : FrenchInteger(value / 1000) + " mille") + (value % 1000 == 0 ? "" : " " + FrenchInteger(value % 1000));
        if (value >= 100) return (value / 100 == 1 ? "cent" : FrenchInteger(value / 100) + " cent" + (value / 100 > 1 && value % 100 == 0 ? "s" : "")) + (value % 100 == 0 ? "" : " " + FrenchInteger(value % 100));
        if (value < 17) return new[] { "", "un", "deux", "trois", "quatre", "cinq", "six", "sept", "huit", "neuf", "dix", "onze", "douze", "treize", "quatorze", "quinze", "seize" }[(int)value];
        if (value < 20) return "dix-" + FrenchInteger(value - 10);
        if (value < 70)
        {
            var tens = new[] { "", "", "vingt", "trente", "quarante", "cinquante", "soixante" }[(int)value / 10]; var rest = (int)value % 10;
            return tens + (rest == 1 ? " et un" : rest > 1 ? "-" + FrenchInteger(rest) : "");
        }
        if (value < 80) return "soixante-" + FrenchInteger(value - 60);
        return "quatre-vingt" + (value == 80 ? "s" : "-" + FrenchInteger(value - 80));
    }
}
