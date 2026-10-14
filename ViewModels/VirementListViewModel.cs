using AkarangaBank.Web.Models;

namespace AkarangaBank.Web.ViewModels;

public class VirementListViewModel
{
    public IReadOnlyList<Virement> Virements { get; set; } = [];
    public bool PeutValider { get; set; }
}
