using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsyncDownloader.Abstractions;
using AsyncDownloader.Domain;

namespace AsyncDownloader.Services
{
    public sealed class ConsoleProgressSink : IProgressSink
    {
        private int _done;
        private int _total;
        private readonly object _gate = new object();
        public void OnStarted(int total)
        {
            _total = total;
            _done = 0;
            Console.WriteLine($"Starting downloads: {_total} urls\n");
        }


        public void OnSuccess(PageRequest req, TimeSpan elapsed)
        {
            var d = Interlocked.Increment(ref _done);
            lock (_gate)
            {
                Console.WriteLine($"[OK] {req.Url} ({elapsed.TotalMilliseconds:n0} ms) {d}/{_total}");
            }
        }


        public void OnFailure(PageRequest req, Exception ex, TimeSpan elapsed)
        {
            var d = Interlocked.Increment(ref _done);
            lock (_gate)
            {
                Console.WriteLine($"[ERR] {req.Url} ({elapsed.TotalMilliseconds:n0} ms) {d}/{_total}\n → {ex.Message}");
            }
        }


        public void OnCompleted(TimeSpan totalElapsed)
        {
            Console.WriteLine($"\nAll done in {totalElapsed.TotalSeconds:n2}s");
        }
    }
}
