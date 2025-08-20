using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsyncDownloader.Domain;

namespace AsyncDownloader.Abstractions
{
    public interface IStorage
    {
        Task SaveAsync(PageContent content, CancellationToken ct);
    }
}
