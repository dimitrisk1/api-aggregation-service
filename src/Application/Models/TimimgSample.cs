using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models
{
    public record TimingSample(DateTime Timestamp, TimeSpan Latency, bool IsSuccess);
}
