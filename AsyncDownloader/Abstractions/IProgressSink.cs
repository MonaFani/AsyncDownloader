using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsyncDownloader.Domain;

namespace AsyncDownloader.Abstractions
{
    public interface IProgressSink
    {
        void OnStarted(int total);
        void OnSuccess(PageRequest req, TimeSpan elapsed);
        void OnFailure(PageRequest req, Exception ex, TimeSpan elapsed);
        void OnCompleted(TimeSpan totalElapsed);
    }
}
