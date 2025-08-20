using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncDownloader.Abstractions
{
    public interface IFileNameStrategy
    {
        string BuildFileName(Uri url, string mediaType);
    }
}
