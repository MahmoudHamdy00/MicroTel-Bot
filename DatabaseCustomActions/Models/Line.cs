using System;
using System.Collections.Generic;

#nullable disable

namespace DatabaseCustomActions.Models
{
    public partial class Line
    {
        public Line()
        {
            Bills = new HashSet<Bill>();
            ExtraPackages = new HashSet<ExtraPackage>();
            Users = new HashSet<User>();
        }

        public string PhoneNumber { get; set; }
        public Guid? TierId { get; set; }
        public Guid? QuotaId { get; set; }

        public virtual Quotum Quota { get; set; }
        public virtual TierDetail Tier { get; set; }
        public virtual ICollection<Bill> Bills { get; set; }
        public virtual ICollection<ExtraPackage> ExtraPackages { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}
