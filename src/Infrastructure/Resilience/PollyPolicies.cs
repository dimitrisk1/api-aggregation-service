using Polly;
using Polly.Extensions.Http;//check again
using System.Net;
namespace Infrastructure.Resilience
{
    public static class PollyPolicies
    {
        public static IAsyncPolicy<HttpResponseMessage> RetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError() 
                .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt =>
                        TimeSpan.FromMilliseconds(200 * Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        Console.WriteLine($"Retry {retryAttempt} after {timespan.TotalMilliseconds}ms");
                    });
        }

        public static IAsyncPolicy<HttpResponseMessage> CircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 5,
                    durationOfBreak: TimeSpan.FromSeconds(30));
        }

        public static IAsyncPolicy<HttpResponseMessage> TimeoutPolicy()
        {
            return Policy.TimeoutAsync<HttpResponseMessage>(3);
        }
    }
}
