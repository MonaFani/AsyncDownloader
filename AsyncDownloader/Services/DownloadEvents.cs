using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsyncDownloader.Domain;
using Downloader.Abstractions;

namespace AsyncDownloader.Services
{
    public class DownloadEvents : IDownloadEvents
    {
        private readonly IEnumerable<IDownloadListener> _listeners;
        public DownloadEvents(IEnumerable<IDownloadListener> listeners)
        {
                _listeners = listeners;
        }

      

        public void PublishDownloadFailure(PageRequest req)
        {
            foreach (var l in _listeners)
            {
               l.OnFailure(req); 
            }
        }

        public void PublishDownloadStarted(PageRequest req)
        {
            foreach (var l in _listeners)
            {
                l.OnStarted(req); 
            }
        }

        public void PublishDownloadSuccess(PageRequest req)
        {
            foreach (var l in _listeners)
            {
                l.OnSuccess(req);
            }
        }
    }
}
