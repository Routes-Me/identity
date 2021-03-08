using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentitiesService.Models
{
    public class Pagination
    {
        public int offset { get; set; } = 1;
        public int limit { get; set; } = 10;
        public int total { get; set; }
    }
}
