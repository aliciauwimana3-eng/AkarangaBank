using AkarangaBank.Web.Models;

namespace AkarangaBank.Web.ViewModels;

public class JournalViewModel
{
    public IReadOnlyList<Operation> Operations { get; set; } = [];
    public DateTime? Du { get; set; }
    public DateTime? Au { get; set; }
    public string? TypeOperation { get; set; }
    public int Page { get; set; } = 1;
    public int TaillePage { get; set; } = 20;
    public int Total { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)Total / Math.Max(1, TaillePage));
}
