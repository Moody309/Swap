using Microsoft.Extensions.Logging;

namespace Swap.API.Services;

public class MockSmsService(ILogger<MockSmsService> logger) : ISmsService
{
    public Task SendAsync(string phoneNumber, string message)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("SMS to {PhoneNumber}: {Message}", phoneNumber, message);
        }

        return Task.CompletedTask; 
    }
}
