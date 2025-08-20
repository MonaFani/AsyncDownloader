using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AsyncDownloader.Abstractions;

namespace AsyncDownloader.Services
{
    public sealed class SafeFileNameStrategy : IFileNameStrategy
    {
        private static readonly Regex Unsafe = new Regex("[^a-zA-Z0-9._-]+", RegexOptions.Compiled);
        public string BuildFileName(Uri url, string mediaType)
        {
            var path = url.Host + url.AbsolutePath;
            if (string.IsNullOrWhiteSpace(path) || path.EndsWith("/")) path += "index";
            var ext = mediaType switch
            {
                "text/html" => ".html",
                "application/json" => ".json",
                "text/plain" => ".txt",
                _ => ".bin"
            };
            var safe = Unsafe.Replace(path, "-").Trim('-');
            return safe + ext;
        }
    }
}
