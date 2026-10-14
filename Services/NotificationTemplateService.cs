using System.Net;
using System.Text.RegularExpressions;
using AkarangaBank.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace AkarangaBank.Web.Services;

public sealed class NotificationTemplateService : INotificationTemplateService
{
    private static readonly Regex Token = new(@"\{\{(?<key>[A-Za-z0-9_]+)\}\}", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private readonly AkarangaBankContext _db;

    public NotificationTemplateService(AkarangaBankContext db) => _db = db;

    public async Task<NotificationRendered?> RenderAsync(string code, string canal, string langue, IReadOnlyDictionary<string, string> variables, CancellationToken cancellationToken = default)
    {
        var template = await _db.NotificationTemplates.AsNoTracking()
            .Where(t => t.Code == code && t.Canal == canal && t.Langue == langue && t.EstActive)
            .OrderByDescending(t => t.Version).FirstOrDefaultAsync(cancellationToken);
        if (template is null) return null;
        string Replace(string source, bool htmlEncode) => Token.Replace(source, match =>
        {
            var key = match.Groups["key"].Value;
            return variables.TryGetValue(key, out var value) ? htmlEncode ? WebUtility.HtmlEncode(value) : value : match.Value;
        });
        return new NotificationRendered(Replace(template.Sujet, false), template.ContenuHtml is null ? null : Replace(template.ContenuHtml, true), Replace(template.ContenuTexte, false), template.Version);
    }
}
