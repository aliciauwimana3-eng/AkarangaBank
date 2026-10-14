using MailKit.Security;
using MimeKit;

namespace AkarangaBank.Web.Services;

public sealed class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    public EmailService(IConfiguration configuration) => _configuration = configuration;

    public async Task SendAsync(string destination, string subject, string text, string? html, CancellationToken cancellationToken = default)
    {
        var host = _configuration["Email:SmtpHost"];
        var from = _configuration["Email:From"];
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(from)) throw new InvalidOperationException("Configuration SMTP incomplete.");
        var port = _configuration.GetValue("Email:SmtpPort", 587);
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(from));
        message.To.Add(MailboxAddress.Parse(destination));
        message.Subject = subject;
        message.Body = new BodyBuilder { TextBody = text, HtmlBody = html }.ToMessageBody();
        using var client = new MailKit.Net.Smtp.SmtpClient();
        var socketOptions = _configuration.GetValue("Email:UseSslOnConnect", false) ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;
        await client.ConnectAsync(host, port, socketOptions, cancellationToken);
        var username = _configuration["Email:SmtpUsername"];
        var password = _configuration["Email:SmtpPassword"];
        if (!string.IsNullOrWhiteSpace(username))
        {
            if (string.IsNullOrWhiteSpace(password)) throw new InvalidOperationException("Mot de passe SMTP absent.");
            await client.AuthenticateAsync(username, password, cancellationToken);
        }
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}
