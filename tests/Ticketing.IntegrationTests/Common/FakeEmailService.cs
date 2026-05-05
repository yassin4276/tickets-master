using Ticketing.Application.Interfaces.Email;

namespace Ticketing.IntegrationTests.Common;

public class FakeEmailService : IEmailService
{
    public Task SendEmailAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}