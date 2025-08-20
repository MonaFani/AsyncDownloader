using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsyncDownloader.Abstractions;

namespace AsyncDownloader.Core
{
    public sealed class DownloadManager
    {
        private readonly IUrlSource _source;
        private readonly IDownloadService _downloader;
        private readonly IStorage _storage;
        private readonly IProgressSink _progress;
        private readonly int _maxDegreeOfParallelism;


        public DownloadManager(
        IUrlSource source,
        IDownloadService downloader,
        IStorage storage,
        IProgressSink progress,
        int maxDegreeOfParallelism = 8)
        {
            _source = source;
            _downloader = downloader;
            _storage = storage;
            _progress = progress;
            _maxDegreeOfParallelism = Math.Max(1, maxDegreeOfParallelism);
        }


        public async Task RunAsync(CancellationToken ct)
        {
            var urls = await _source.GetUrlsAsync(ct);
            _progress.OnStarted(urls.Count);
            var swTotal = System.Diagnostics.Stopwatch.StartNew();


            using var semaphore = new SemaphoreSlim(_maxDegreeOfParallelism, _maxDegreeOfParallelism);
            var tasks = new List<Task>();


            foreach (var req in urls)
            {
                await semaphore.WaitAsync(ct);
                tasks.Add(Task.Run(async () =>
                {
                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    try
                    {
                        var content = await _downloader.DownloadAsync(req, ct);
                        await _storage.SaveAsync(content, ct);
                        _progress.OnSuccess(req, sw.Elapsed);
                    }
                    catch (Exception ex)
                    {
                        _progress.OnFailure(req, ex, sw.Elapsed);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }, ct));
            }


            await Task.WhenAll(tasks);
            _progress.OnCompleted(swTotal.Elapsed);
        }
    }
}
