using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IExternalProvider
    {
        string Name { get; }
        Task<IEnumerable<object>> GetDataAsync(CancellationToken ct);
    }
}
