using System.Text.RegularExpressions;

namespace OnlineDelivery.Api.Services.Auth;

/// <summary>Indian mobile numbers, stored as +91 and 10 digits: SPEC-0003.</summary>
public static partial class MobileNumber
{
    public static bool TryNormalize(string? input, out string normalized)
    {
        normalized = "";
        if (input is null)
        {
            return false;
        }

        var digits = input.Replace(" ", "").Replace("-", "");
        if (digits.StartsWith("+91", StringComparison.Ordinal))
        {
            digits = digits[3..];
        }
        else if (digits.Length == 12 && digits.StartsWith("91", StringComparison.Ordinal))
        {
            digits = digits[2..];
        }

        if (!TenDigits().IsMatch(digits))
        {
            return false;
        }

        normalized = "+91" + digits;
        return true;
    }

    [GeneratedRegex("^[6-9][0-9]{9}$")]
    private static partial Regex TenDigits();
}
