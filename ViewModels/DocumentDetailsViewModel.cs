using AkarangaBank.Web.Models;

namespace AkarangaBank.Web.ViewModels;

public sealed class DocumentDetailsViewModel
{
    public required Document Document { get; init; }
    public IReadOnlyList<DocumentAcces> Acces { get; init; } = [];
}
