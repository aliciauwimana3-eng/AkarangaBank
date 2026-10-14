namespace AkarangaBank.Web.Models;

public class NotificationTemplate
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Canal { get; set; } = "Email";
    public string Langue { get; set; } = "fr-BI";
    public string Sujet { get; set; } = string.Empty;
    public string? ContenuHtml { get; set; }
    public string ContenuTexte { get; set; } = string.Empty;
    public int Version { get; set; } = 1;
    public bool EstActive { get; set; } = true;
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;
}
