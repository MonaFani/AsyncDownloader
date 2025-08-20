using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsyncDownloader.Domain;

namespace AsyncDownloader.Abstractions
{
    public interface IDownloadService
    {
        Task<PageContent> DownloadAsync(PageRequest request, CancellationToken ct);
    }
}
