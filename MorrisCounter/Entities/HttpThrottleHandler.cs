using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MorrisCounter.Entities
{
    public class HttpThrottleHandler : DelegatingHandler
    {
        public HttpThrottleHandler(HttpMessageHandler innerHandler) : base(innerHandler)
        {
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            if ((int)response.StatusCode == 429)
            {
                // Get the throttle duration from the headers
                int throttleDuration = Convert.ToInt32(response.Headers.RetryAfter.ToString()) * 1000;

                Console.WriteLine($@"Throttled for {throttleDuration}ms");

                Thread.Sleep(throttleDuration);

                await this.SendAsync(request, cancellationToken);
            }

            return response;
        }
    }
}
