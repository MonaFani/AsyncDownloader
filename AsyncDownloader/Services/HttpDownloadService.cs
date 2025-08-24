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

        public HttpDownloadService(HttpClient http)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
        }

        public async Task<PageContent> DownloadAsync(PageRequest request, CancellationToken ct)
        {

            using var msg = new HttpRequestMessage(HttpMethod.Get, request.Url);
            using var resp = await _http.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead, ct);
            resp.EnsureSuccessStatusCode();
            var mediaType = resp.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
            var bytes = await resp.Content.ReadAsByteArrayAsync(ct);
            return new PageContent(request.Url, bytes, mediaType);

        }
    }
}
