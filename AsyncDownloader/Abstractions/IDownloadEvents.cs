using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsyncDownloader.Domain;


namespace Downloader.Abstractions
{
    public interface IDownloadEvents
    {
        void PublishDownloadStarted(PageRequest req);
        void PublishDownloadSuccess(PageRequest req);
        void PublishDownloadFailure(PageRequest req);
    }
}
