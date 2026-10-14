using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace AkarangaBank.Web.Services;

public sealed class SmsService : ISmsService
{
    private readonly IConfiguration _configuration;
    public SmsService(IConfiguration configuration) => _configuration = configuration;

    public async Task SendAsync(string destination, string message, CancellationToken cancellationToken = default)
    {
        var sid = _configuration["Twilio:AccountSid"];
        var token = _configuration["Twilio:AuthToken"];
        var from = _configuration["Twilio:From"];
        if (string.IsNullOrWhiteSpace(sid) || string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(from))
            throw new InvalidOperationException("Configuration Twilio incomplete.");
        TwilioClient.Init(sid, token);
        await MessageResource.CreateAsync(to: new PhoneNumber(destination), from: new PhoneNumber(from), body: message);
    }
}
