namespace AkarangaBank.Web.ViewModels;

public sealed record JournalReportRow(int OperationId, DateTime Date, string Reference, string Client, string Account, string Type, decimal Debit, decimal Credit, string Currency, string Status);
public sealed record BalanceReportRow(string Account, string Label, string Currency, decimal Debit, decimal Credit)
{
    public decimal Balance => Debit - Credit;
}
public sealed record AmlClientReportRow(string ClientNumber, string Name, int AlertCount, decimal SuspiciousAmount);
public sealed record AmlMonthlyReport(DateTime Month, int TotalAlerts, int Low, int Medium, int High, int Critical, int DosCount, decimal SuspiciousAmount, IReadOnlyList<AmlClientReportRow> TopClients);
public sealed record DashboardReport(int Clients, int Accounts, decimal TotalBalance, int OperationsToday, int ActiveLoans, int OpenAmlAlerts);
public sealed record FinancialStatementReport(string Title, DateTime AsOf, decimal Assets, decimal Liabilities, decimal Equity, decimal Revenue, decimal Expenses, decimal Result, IReadOnlyList<BalanceReportRow> Rows);
public sealed record LoanPortfolioRow(string Reference, string Client, string Type, DateTime StartDate, decimal Principal, decimal Repaid, decimal Outstanding, string Status, string Currency);
public sealed record ReportFilter(DateTime? From, DateTime? To, int Page = 1, int PageSize = 50);
