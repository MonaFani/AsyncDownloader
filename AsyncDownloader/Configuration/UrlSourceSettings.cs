using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncDownloader.Configuration
{
    public sealed class UrlSourceSettings
    {
        public string[] Urls { get; init; } = Array.Empty<string>();
    }
}
