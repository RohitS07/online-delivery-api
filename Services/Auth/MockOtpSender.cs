namespace OnlineDelivery.Api.Services.Auth;

/// <summary>Sends nothing. In Development it logs the code instead: ADR-0009.</summary>
public partial class MockOtpSender(IHostEnvironment environment, ILogger<MockOtpSender> logger) : IOtpSender
{
    private readonly ILogger _logger = logger;

    public Task SendAsync(string mobileNumber, string code, CancellationToken cancellationToken)
    {
        if (environment.IsDevelopment())
        {
            LogCode(mobileNumber, code);
        }

        return Task.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Mock OTP for {MobileNumber}: {Code}")]
    private partial void LogCode(string mobileNumber, string code);
}
