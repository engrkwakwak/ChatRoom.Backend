using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DataTransferObjects.Status
{
    public record StatusDto
    {
        public int StatusId { get; init; }
        public string StatusName { get; init; } = string.Empty;
    }
}
