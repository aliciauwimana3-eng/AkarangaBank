using System.Globalization;
using System.Text;
using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;
using AkarangaBank.Web.ViewModels;

namespace AkarangaBank.Web.Services;

public sealed class ReportingExportService
{
    public byte[] JournalExcel(IEnumerable<JournalReportRow> rows) => Excel("Journal", sheet =>
    {
        var headers = new[] { "Date", "Référence", "Client", "Compte", "Opération", "Débit", "Crédit", "Devise", "Statut" };
        for (var i = 0; i < headers.Length; i++) sheet.Cell(1, i + 1).Value = headers[i];
        var row = 2;
        foreach (var item in rows)
        {
            sheet.Cell(row, 1).Value = item.Date; sheet.Cell(row, 2).Value = item.Reference; sheet.Cell(row, 3).Value = item.Client;
            sheet.Cell(row, 4).Value = item.Account; sheet.Cell(row, 5).Value = item.Type; sheet.Cell(row, 6).Value = item.Debit;
            sheet.Cell(row, 7).Value = item.Credit; sheet.Cell(row, 8).Value = item.Currency; sheet.Cell(row++, 9).Value = item.Status;
        }
    });

    public byte[] BalanceExcel(IEnumerable<BalanceReportRow> rows) => Excel("Balance", sheet =>
    {
        string[] headers = ["Compte", "Libellé", "Devise", "Total débit", "Total crédit", "Solde débit - crédit"];
        for (var i = 0; i < headers.Length; i++) sheet.Cell(1, i + 1).Value = headers[i];
        var row = 2;
        foreach (var item in rows)
        {
            sheet.Cell(row, 1).Value = item.Account; sheet.Cell(row, 2).Value = item.Label; sheet.Cell(row, 3).Value = item.Currency;
            sheet.Cell(row, 4).Value = item.Debit; sheet.Cell(row, 5).Value = item.Credit; sheet.Cell(row++, 6).Value = item.Balance;
        }
    });

    public byte[] AmlExcel(AmlMonthlyReport report) => Excel("AML", sheet =>
    {
        sheet.Cell("A1").Value = "Rapport AML — " + report.Month.ToString("yyyy-MM", CultureInfo.InvariantCulture);
        sheet.Cell("A3").Value = "Total alertes"; sheet.Cell("B3").Value = report.TotalAlerts;
        sheet.Cell("A4").Value = "Faible"; sheet.Cell("B4").Value = report.Low;
        sheet.Cell("A5").Value = "Moyen"; sheet.Cell("B5").Value = report.Medium;
        sheet.Cell("A6").Value = "Élevé"; sheet.Cell("B6").Value = report.High;
        sheet.Cell("A7").Value = "Critique"; sheet.Cell("B7").Value = report.Critical;
        sheet.Cell("A8").Value = "DOS/CNRF enregistrées"; sheet.Cell("B8").Value = report.DosCount;
        sheet.Cell("A9").Value = "Montant suspect cumulé"; sheet.Cell("B9").Value = report.SuspiciousAmount;
        sheet.Cell("A11").Value = "N° client"; sheet.Cell("B11").Value = "Client"; sheet.Cell("C11").Value = "Alertes"; sheet.Cell("D11").Value = "Montant suspect";
        var row = 12;
        foreach (var item in report.TopClients) { sheet.Cell(row, 1).Value = item.ClientNumber; sheet.Cell(row, 2).Value = item.Name; sheet.Cell(row, 3).Value = item.AlertCount; sheet.Cell(row++, 4).Value = item.SuspiciousAmount; }
    });

    public byte[] LoanPortfolioExcel(IEnumerable<LoanPortfolioRow> rows) => Excel("Prêts", sheet =>
    {
        string[] headers = ["Référence", "Client", "Type", "Date de début", "Principal", "Remboursé", "Encours", "Statut", "Devise"];
        for (var i = 0; i < headers.Length; i++) sheet.Cell(1, i + 1).Value = headers[i];
        var row = 2;
        foreach (var item in rows)
        {
            sheet.Cell(row, 1).Value = item.Reference; sheet.Cell(row, 2).Value = item.Client; sheet.Cell(row, 3).Value = item.Type;
            sheet.Cell(row, 4).Value = item.StartDate; sheet.Cell(row, 5).Value = item.Principal; sheet.Cell(row, 6).Value = item.Repaid;
            sheet.Cell(row, 7).Value = item.Outstanding; sheet.Cell(row, 8).Value = item.Status; sheet.Cell(row++, 9).Value = item.Currency;
        }
    });

    public byte[] CsvJournal(IEnumerable<JournalReportRow> rows)
    {
        using var stream = new MemoryStream();
        using (var writer = new StreamWriter(stream, new UTF8Encoding(true), leaveOpen: true))
        using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.GetCultureInfo("fr-FR")) { Delimiter = ";", InjectionOptions = InjectionOptions.Escape }))
        {
            csv.WriteRecords(rows.Select(r => new { Date = r.Date, Reference = r.Reference, Client = r.Client, Compte = r.Account, Operation = r.Type, Debit = r.Debit, Credit = r.Credit, Devise = r.Currency, Statut = r.Status }));
        }
        return stream.ToArray();
    }

    private static byte[] Excel(string name, Action<IXLWorksheet> populate)
    {
        using var workbook = new XLWorkbook(); var sheet = workbook.Worksheets.Add(name); populate(sheet);
        var headerRow = name == "AML" ? 11 : 1;
        sheet.Row(headerRow).Style.Font.Bold = true;
        sheet.Row(headerRow).Style.Fill.BackgroundColor = XLColor.FromHtml("#1F3864");
        sheet.Row(headerRow).Style.Font.FontColor = XLColor.White;
        sheet.SheetView.FreezeRows(headerRow); sheet.Columns().AdjustToContents(8, 60);
        using var stream = new MemoryStream(); workbook.SaveAs(stream); return stream.ToArray();
    }
}
