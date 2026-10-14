using AkarangaBank.Web.ViewModels;

namespace AkarangaBank.Web.Services;

public interface IReportingService
{
    Task<IReadOnlyList<JournalReportRow>> JournalAsync(int? agenceId, DateTime? from, DateTime? to, int take = 50000);
    Task<IReadOnlyList<BalanceReportRow>> BalanceAsync(int? agenceId, DateTime? from, DateTime? to);
    Task<AmlMonthlyReport> AmlAsync(int? agenceId, DateTime month);
    Task<DashboardReport> DashboardAsync(int? agenceId, DateTime today);
    Task<FinancialStatementReport> FinancialStatementAsync(int? agenceId, DateTime? from, DateTime? to);
    Task<IReadOnlyList<LoanPortfolioRow>> LoanPortfolioAsync(int? agenceId);
}
