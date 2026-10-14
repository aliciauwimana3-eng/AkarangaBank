namespace AkarangaBank.Web.Services;

public sealed class DocumentPurgeHangfireJob
{
    private readonly IDocumentService _documents;
    private readonly IConfiguration _configuration;
    public DocumentPurgeHangfireJob(IDocumentService documents, IConfiguration configuration) { _documents = documents; _configuration = configuration; }
    public Task<int> RunAsync() => _documents.PurgerAsync(Math.Clamp(_configuration.GetValue("Documents:DeletedRetentionDays", 90), 1, 3650));
}
