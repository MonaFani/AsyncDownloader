using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsyncDownloader.Abstractions;
using AsyncDownloader.Domain;

namespace AsyncDownloader.Services
{
    public sealed class FileSystemStorage : IStorage
    {
        private readonly string _root;
        private readonly IFileNameStrategy _namer;


        public FileSystemStorage(string root, IFileNameStrategy namer)
        {
            _root = root ?? throw new ArgumentNullException(nameof(root));
            _namer = namer ?? throw new ArgumentNullException(nameof(namer));
        }


        public async Task SaveAsync(PageContent content, CancellationToken ct)
        {
            var relative = _namer.BuildFileName(content.Url, content.MediaType);
            var full = Path.Combine(_root, relative);
            var dir = Path.GetDirectoryName(full)!;
            Directory.CreateDirectory(dir);
            await File.WriteAllBytesAsync(full, content.Bytes, ct);
        }
    }
}
