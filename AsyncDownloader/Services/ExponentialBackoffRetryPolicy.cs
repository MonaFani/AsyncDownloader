using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsyncDownloader.Abstractions;

namespace AsyncDownloader.Services
{
    public sealed class ExponentialBackoffRetryPolicy : IRetryPolicy
    {
        private readonly int _maxRetries;
        private readonly TimeSpan _baseDelay;
        private readonly Random _rng = new Random();


        public ExponentialBackoffRetryPolicy(int maxRetries = 3, TimeSpan? baseDelay = null)
        {
            _maxRetries = Math.Max(0, maxRetries);
            _baseDelay = baseDelay ?? TimeSpan.FromMilliseconds(200);
        }


        public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken ct)
        {
            int attempt = 0;
            Exception? last = null;
            while (attempt <= _maxRetries)
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    return await action(ct);
                }
                catch (Exception ex) when (IsTransient(ex))
                {
                    last = ex;
                    if (attempt == _maxRetries) break;
                    var delay = ComputeBackoff(++attempt);
                    await Task.Delay(delay, ct);
                }
            }
            throw new HttpRequestException($"Operation failed after {_maxRetries + 1} attempts.", last);
        }


        private TimeSpan ComputeBackoff(int attempt)
        {
            var exp = Math.Pow(2, attempt - 1);
            var jitterMs = _rng.Next(0, 250);
            var delay = TimeSpan.FromMilliseconds(_baseDelay.TotalMilliseconds * exp + jitterMs);
            return delay > TimeSpan.FromSeconds(5) ? TimeSpan.FromSeconds(5) : delay;
        }


        private static bool IsTransient(Exception ex)
        {
            if (ex is TaskCanceledException) return true; // timeouts
            if (ex is HttpRequestException) return true;
            if (ex is AggregateException agg) return agg.InnerExceptions.All(IsTransient);
            return false;
        }
    }
}
