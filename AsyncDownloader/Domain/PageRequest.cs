using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncDownloader.Domain
{
    public sealed record PageRequest(Uri Url);
}
