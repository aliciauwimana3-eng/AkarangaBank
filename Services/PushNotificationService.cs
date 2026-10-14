using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;

namespace AkarangaBank.Web.Services;

public sealed class PushNotificationService : IPushNotificationService
{
    private static readonly object Sync = new();
    private readonly IConfiguration _configuration;
    public PushNotificationService(IConfiguration configuration) => _configuration = configuration;

    public async Task SendAsync(string token, string title, string message, CancellationToken cancellationToken = default)
    {
        var credentialPath = _configuration["Firebase:ServiceAccountPath"];
        if (string.IsNullOrWhiteSpace(credentialPath) || !File.Exists(credentialPath))
            throw new InvalidOperationException("Le compte de service Firebase n'est pas configure.");
        FirebaseApp app;
        lock (Sync)
        {
            try { app = FirebaseApp.GetInstance("AkarangaBankPush"); }
            catch (ArgumentException)
            {
                app = FirebaseApp.Create(new AppOptions { Credential = CredentialFactory.FromFile<ServiceAccountCredential>(credentialPath).ToGoogleCredential() }, "AkarangaBankPush");
            }
        }
        await FirebaseMessaging.GetMessaging(app).SendAsync(new Message
        {
            Fid = token,
            Notification = new FirebaseAdmin.Messaging.Notification { Title = title, Body = message }
        }, cancellationToken);
    }
}
