using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsyncDownloader.Abstractions;
using AsyncDownloader.Domain;

namespace AsyncDownloader.Services.Decorators
{
    public class RetryingDownloadService : IDownloadService
    {
        private readonly IDownloadService _downloader;
        private readonly int _maxRetries;
        private readonly TimeSpan _baseDelay;
        public RetryingDownloadService(IDownloadService downloader, int maxRetries = 3, TimeSpan? baseDelay = null)
        {
            _downloader = downloader;
            _maxRetries = Math.Max(0, maxRetries);
            _baseDelay = baseDelay ?? TimeSpan.FromMilliseconds(200);
        }
        public async Task<PageContent> DownloadAsync(PageRequest request, CancellationToken ct)
        {
            int attempt = 0;
            Exception? last = null;
            while (attempt <= _maxRetries)
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    return await _downloader.DownloadAsync(request,ct);
                }
                catch (Exception ex) when (IsTransient(ex))
                {
                    last = ex;
                    if (attempt == _maxRetries) break;
                    var delay = ComputeBackoff(++attempt);
                    Console.WriteLine($"[Retry {attempt}/{_maxRetries}] Transient error: {ex.Message}. " +
                      $"Waiting {delay.TotalMilliseconds:N0} ms before next attempt...");

                    await Task.Delay(delay, ct);
                }
            }
            throw new HttpRequestException($"Operation failed after {_maxRetries + 1} attempts.", last);
        }
        private TimeSpan ComputeBackoff(int attempt)
        {
            var exp = Math.Pow(2, attempt - 1);
            var jitterMs = Random.Shared.Next(0, 250);
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
