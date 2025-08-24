using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsyncDownloader.Domain;


namespace Downloader.Abstractions
{
    public interface IDownloadListener
    {
        void OnStarted(PageRequest req);
        void OnSuccess(PageRequest req);
        void OnFailure(PageRequest req);
    
    }
}
