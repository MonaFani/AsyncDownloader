using AsyncDownloader.Abstractions;
using AsyncDownloader.Core;
using AsyncDownloader.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration.Json;
using AsyncDownloader.Configuration;
using Microsoft.Extensions.Options;
using AsyncDownloader.Services.Decorators;
using Downloader.Abstractions;

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (s, e) => { e.Cancel = true; cts.Cancel(); };



//var urls = args.Length > 0 ? args : new[]
//{
//"https://example.com/",
//"https://www.entaingroup.com/",
//"https://httpbin.org/json",
//"https://en.wikipedia.org/wiki/Special:Random",
//"https://www.bbc.com/"
//};

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();

var services = new ServiceCollection();

//var urls = config.GetSection("UrlSource:Urls").Get<string[]>()
//           ?? Array.Empty<string>();

services.AddSingleton<IUrlSource, StaticUrlSource>();

services.Configure<UrlSourceSettings>(config.GetSection("UrlSource"));
//services.AddSingleton<IUrlSource>(sp =>
//{
//    var opts = sp.GetRequiredService<IOptions<UrlSourceSettings>>().Value;
//    return new StaticUrlSource(opts.Urls);
//});
services.AddSingleton<IFileNameStrategy, SafeFileNameStrategy>();
services.AddSingleton<IRetryPolicy>(sp => new ExponentialBackoffRetryPolicy(maxRetries: 3, baseDelay: TimeSpan.FromMilliseconds(250)));

services.AddHttpClient<HttpDownloadService>((sp, http) =>
{
    http.Timeout = TimeSpan.FromSeconds(10);
    http.DefaultRequestHeaders.UserAgent.ParseAdd("AsyncDownloader/1.0");
});


services.AddTransient<IDownloadService>(sp =>
{
    var baseDownloader = sp.GetRequiredService<HttpDownloadService>();
    var retry = sp.GetRequiredService<IRetryPolicy>();
    return new RetryingDownloadService(baseDownloader);
});

services.AddSingleton<IStorage>(sp =>
{
    var namer = sp.GetRequiredService<IFileNameStrategy>();
    var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\"));
    var root = Path.Combine(projectRoot, "downloads");
    return new FileSystemStorage(root, namer);
});

//services.AddSingleton<IUrlSource>(_ => new StaticUrlSource(urls));

//services.AddHttpClient<IDownloadService, HttpDownloadService>(http =>
//{
//    http.Timeout = TimeSpan.FromSeconds(10);
//    http.DefaultRequestHeaders.UserAgent.ParseAdd("AsyncDownloader/1.0");
//});

services.AddSingleton<IProgressSink, ConsoleProgressSink>();

services.AddTransient<DownloadManager>(sp =>
    new DownloadManager(
        sp.GetRequiredService<IUrlSource>(),
        sp.GetRequiredService<IDownloadService>(),
        sp.GetRequiredService<IStorage>(),
        sp.GetRequiredService<IProgressSink>(),
         sp.GetRequiredService<IDownloadEvents>(),
        maxDegreeOfParallelism: 6
    ));

services.AddScoped<IDownloadEvents, DownloadEvents>();
services.AddScoped<IDownloadListener, DownloadListener>();


using var provider = services.BuildServiceProvider();
var manager = provider.GetRequiredService<DownloadManager>();
try
{
    await manager.RunAsync(cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("Canceled by user (Ctrl+C). Goodbye!");
}
catch (Exception ex)
{
    Console.WriteLine($"Unexpected error: {ex.Message}");
}