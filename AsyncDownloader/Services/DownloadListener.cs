using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsyncDownloader.Abstractions;
using AsyncDownloader.Domain;
using Downloader.Abstractions;

namespace AsyncDownloader.Services
{
    public class DownloadListener : IDownloadListener
    {
        public void OnStarted(PageRequest req)
        {
            Log("Download initiated", req.Url);
        }

        public void OnSuccess(PageRequest req)
        {
            Log("Download completed successfully", req.Url);
        }

        public void OnFailure(PageRequest req)
        {
            Log("Download failed", req.Url);
        }

        private void Log(string message, Uri url)
        {
            if (string.IsNullOrWhiteSpace(url.AbsolutePath))
                Console.WriteLine($"[Info] {message}");
            else
                Console.WriteLine($"[Info] {message} ---> {url}");
        }
    }
}
