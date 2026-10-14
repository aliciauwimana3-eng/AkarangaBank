using AkarangaBank.Web.Models;

namespace AkarangaBank.Web.ViewModels;

public class PretListViewModel
{
    public IReadOnlyList<Pret> Prets { get; set; } = [];
    public bool PeutValider { get; set; }
}
