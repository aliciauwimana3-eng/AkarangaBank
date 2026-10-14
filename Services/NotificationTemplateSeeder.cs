using AkarangaBank.Web.Data;
using AkarangaBank.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace AkarangaBank.Web.Services;

public static class NotificationTemplateSeeder
{
    private static readonly (string Code, string[] Values)[] Events =
    [
        ("VIREMENT_RECU", ["Un virement de {{Montant}} {{Devise}} a ete credite sur votre compte.", "A transfer of {{Montant}} {{Devise}} was credited to your account.", "Amafaranga {{Montant}} {{Devise}} yashizwe kuri konti yawe.", "Uhamisho wa {{Montant}} {{Devise}} umewekwa kwenye akaunti yako."]),
        ("PRET_VALIDE", ["Votre pret {{Reference}} a ete approuve pour {{Montant}} BIF.", "Your loan {{Reference}} was approved for {{Montant}} BIF.", "Inguzanyo {{Reference}} ya {{Montant}} BIF yemejwe.", "Mkopo {{Reference}} wa {{Montant}} BIF umeidhinishwa."]),
        ("RAPPEL_ECHEANCE", ["Bonjour {{NomClient}}, une echeance de {{Montant}} BIF est due le {{DateEcheance}}.", "Hello {{NomClient}}, a payment of {{Montant}} BIF is due on {{DateEcheance}}.", "{{NomClient}}, ikiringo ca {{Montant}} BIF kizotangwa kw'igenekerezo rya {{DateEcheance}}.", "{{NomClient}}, malipo ya {{Montant}} BIF yanastahili tarehe {{DateEcheance}}."]),
        ("ALERTE_AML_CRITIQUE", ["Alerte AML critique a examiner : {{Reference}}.", "Critical AML alert requires review: {{Reference}}.", "Iburira rikomeye rya AML risaba gusuzumwa: {{Reference}}.", "Tahadhari muhimu ya AML inahitaji ukaguzi: {{Reference}}."])
    ];
    private static readonly string[] Languages = ["fr-BI", "en-US", "rn-BI", "sw-KE"];

    public static async Task SeedAsync(AkarangaBankContext db, CancellationToken cancellationToken = default)
    {
        var keys = Events.SelectMany(e => Languages.SelectMany(language => new[] { (e.Code, "Email", language), (e.Code, "SMS", language), (e.Code, "Push", language) })).ToArray();
        var existing = await db.NotificationTemplates.AsNoTracking().Select(t => new { t.Code, t.Canal, t.Langue }).ToListAsync(cancellationToken);
        var missing = keys.Where(k => !existing.Any(t => t.Code == k.Code && t.Canal == k.Item2 && t.Langue == k.language)).ToList();
        foreach (var key in missing)
        {
            var evt = Events.First(e => e.Code == key.Code);
            var text = evt.Values[Array.IndexOf(Languages, key.language)];
            db.NotificationTemplates.Add(new NotificationTemplate
            {
                Code = key.Code,
                Canal = key.Item2,
                Langue = key.language,
                Sujet = Sujet(key.Code, Array.IndexOf(Languages, key.language)),
                ContenuTexte = text,
                ContenuHtml = key.Item2 == "Email" ? $"<p>{text}</p>" : null,
                Version = 1,
                EstActive = true
            });
        }
        if (missing.Count > 0) await db.SaveChangesAsync(cancellationToken);
    }

    private static string Sujet(string code, int languageIndex) => code switch
    {
        "VIREMENT_RECU" => new[] { "Virement recu", "Transfer received", "Amafaranga yashitse", "Uhamisho umepokelewa" }[languageIndex],
        "PRET_VALIDE" => new[] { "Pret approuve", "Loan approved", "Inguzanyo yemejwe", "Mkopo umeidhinishwa" }[languageIndex],
        "RAPPEL_ECHEANCE" => new[] { "Rappel d'echeance", "Payment due reminder", "Icibutsa c'igihe co kuriha", "Kikumbusho cha malipo" }[languageIndex],
        _ => new[] { "Alerte AML critique", "Critical AML alert", "Iburira rikomeye rya AML", "Tahadhari muhimu ya AML" }[languageIndex]
    };
}
