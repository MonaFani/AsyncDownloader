using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsyncDownloader.Abstractions;
using AsyncDownloader.Domain;

namespace AsyncDownloader.Services
{
    public sealed class HttpDownloadService : IDownloadService
    {
        private readonly HttpClient _http;
        private readonly IRetryPolicy _retryPolicy;


        public HttpDownloadService(HttpClient http, IRetryPolicy retryPolicy)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _retryPolicy = retryPolicy ?? throw new ArgumentNullException(nameof(retryPolicy));
        }


        public async Task<PageContent> DownloadAsync(PageRequest request, CancellationToken ct)
        {
            return await _retryPolicy.ExecuteAsync<PageContent>(async innerCt =>
            {
                using var msg = new HttpRequestMessage(HttpMethod.Get, request.Url);
                using var resp = await _http.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead, innerCt);
                resp.EnsureSuccessStatusCode();
                var mediaType = resp.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
                var bytes = await resp.Content.ReadAsByteArrayAsync(innerCt);
                return new PageContent(request.Url, bytes, mediaType);
            }, ct);
        }
    }
}
