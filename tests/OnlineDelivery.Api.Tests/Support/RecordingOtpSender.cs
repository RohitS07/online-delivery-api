using System.Collections.Concurrent;
using OnlineDelivery.Api.Services.Auth;

namespace OnlineDelivery.Api.Tests.Support;

/// <summary>An OTP sender that keeps every code, so a test can read it.</summary>
public sealed class RecordingOtpSender : IOtpSender
{
    private readonly ConcurrentDictionary<string, List<string>> _sent = new();

    public Task SendAsync(string mobileNumber, string code, CancellationToken cancellationToken)
    {
        _sent.GetOrAdd(mobileNumber, _ => []).Add(code);
        return Task.CompletedTask;
    }

    public IReadOnlyList<string> SentTo(string mobileNumber) =>
        _sent.TryGetValue(mobileNumber, out var codes) ? codes : [];

    public string LastCodeFor(string mobileNumber) => SentTo(mobileNumber)[^1];

    public int TotalSent => _sent.Values.Sum(codes => codes.Count);
}
