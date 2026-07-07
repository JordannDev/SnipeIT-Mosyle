// Copyright (c) Jordan Molenaar (jordanm.cloud). All rights reserved.

using System.Globalization;
using RestSharp;

namespace jordanm.cloud.mosyle.Clients;

internal static class ApiRetryPolicy
{
    public const int DefaultPageDelayMilliseconds = 250;
    public const int DefaultMaxRateLimitRetries = 3;

    public static TimeSpan GetPageDelay(int pageDelayMilliseconds)
    {
        return TimeSpan.FromMilliseconds(Math.Max(0, pageDelayMilliseconds));
    }

    public static async Task DelayBetweenPagesAsync(int pageDelayMilliseconds, CancellationToken cancellationToken)
    {
        var delay = GetPageDelay(pageDelayMilliseconds);
        if (delay > TimeSpan.Zero)
            await Task.Delay(delay, cancellationToken);
    }

    public static TimeSpan GetRetryDelay(
        IEnumerable<HeaderParameter>? headers,
        int pageDelayMilliseconds,
        int retryAttempt)
    {
        var retryAfter = headers?
            .FirstOrDefault(header => string.Equals(header.Name, "Retry-After", StringComparison.OrdinalIgnoreCase))
            ?.Value?.ToString();

        if (!string.IsNullOrWhiteSpace(retryAfter))
        {
            if (int.TryParse(retryAfter, NumberStyles.Integer, CultureInfo.InvariantCulture, out var seconds))
                return TimeSpan.FromSeconds(Math.Max(0, seconds));

            if (DateTimeOffset.TryParse(
                    retryAfter,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal,
                    out var retryAt))
            {
                var delay = retryAt - DateTimeOffset.UtcNow;
                return delay > TimeSpan.Zero ? delay : TimeSpan.Zero;
            }
        }

        var baseDelayMilliseconds = Math.Max(1, pageDelayMilliseconds);
        var multiplier = Math.Pow(2, Math.Max(0, retryAttempt - 1));
        return TimeSpan.FromMilliseconds(baseDelayMilliseconds * multiplier);
    }
}
