using System;
using System.Collections.Generic;

namespace IdentitiesService.Models.DBModels
{
    public partial class Applications
    {
        public int ApplicationId { get; set; }
        public string Name { get; set; }
        public DateTime? CreatedAt { get; set; }

        public virtual ICollection<Roles> Roles { get; set; }
    }
}
