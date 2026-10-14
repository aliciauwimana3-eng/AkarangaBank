using AkarangaBank.Web.Models;

namespace AkarangaBank.Web.ViewModels;

public class AlerteAMLListViewModel
{
    public IReadOnlyList<AlerteAML> Alertes { get; set; } = [];
    public string? Statut { get; set; }
    public string? Niveau { get; set; }
}
