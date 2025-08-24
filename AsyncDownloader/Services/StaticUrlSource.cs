using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsyncDownloader.Abstractions;
using AsyncDownloader.Configuration;
using AsyncDownloader.Domain;
using Microsoft.Extensions.Options;

namespace AsyncDownloader.Services
{
    public sealed class StaticUrlSource : IUrlSource
    {
        private readonly IReadOnlyList<Uri> _urls;
        public StaticUrlSource(IOptions<UrlSourceSettings> options)
        {
            var urls = options.Value.Urls ?? Array.Empty<string>();
            _urls = urls
                .Where(u => !string.IsNullOrWhiteSpace(u))
                .Select(u => new Uri(u, UriKind.Absolute))
                .ToList();
        }
        public Task<IReadOnlyList<PageRequest>> GetUrlsAsync(CancellationToken ct)
        {
            var list = _urls.Select(u => new PageRequest(u)).ToList();
            return Task.FromResult<IReadOnlyList<PageRequest>>(list);
        }
    }
}
