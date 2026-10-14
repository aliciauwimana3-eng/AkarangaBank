using AkarangaBank.Web.Models;

namespace AkarangaBank.Web.ViewModels;

public class OperationListViewModel
{
    public IReadOnlyList<Operation> Operations { get; set; } = [];
    public bool PeutValider { get; set; }
}
