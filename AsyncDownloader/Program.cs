using AsyncDownloader.Abstractions;
using AsyncDownloader.Core;
using AsyncDownloader.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (s, e) => { e.Cancel = true; cts.Cancel(); };



var urls = args.Length > 0 ? args : new[]
{
"https://example.com/",
"https://www.wikipedia.org/",
"https://httpbin.org/json",
"https://httpbin.org/status/503",
"https://www.bbc.com/"
};
var services = new ServiceCollection();

services.AddSingleton<IFileNameStrategy, SafeFileNameStrategy>();
services.AddSingleton<IRetryPolicy>(sp => new ExponentialBackoffRetryPolicy(maxRetries: 3, baseDelay: TimeSpan.FromMilliseconds(250)));

services.AddSingleton<IStorage>(sp =>
{
    var namer = sp.GetRequiredService<IFileNameStrategy>();
    var root = Path.Combine(Environment.CurrentDirectory, "downloads");
    return new FileSystemStorage(root, namer);
});

services.AddSingleton<IUrlSource>(_ => new StaticUrlSource(urls));

services.AddHttpClient<IDownloadService, HttpDownloadService>(http =>
{
    http.Timeout = TimeSpan.FromSeconds(10);
    http.DefaultRequestHeaders.UserAgent.ParseAdd("AsyncDownloader/1.0");
});

services.AddSingleton<IProgressSink, ConsoleProgressSink>();

services.AddTransient<DownloadManager>(sp =>
    new DownloadManager(
        sp.GetRequiredService<IUrlSource>(),
        sp.GetRequiredService<IDownloadService>(),
        sp.GetRequiredService<IStorage>(),
        sp.GetRequiredService<IProgressSink>(),
        maxDegreeOfParallelism: 6
    ));


using var provider = services.BuildServiceProvider();
var manager = provider.GetRequiredService<DownloadManager>();
await manager.RunAsync(cts.Token);
await manager.RunAsync(cts.Token);

