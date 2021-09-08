using System;
using System.Collections.Generic;

#nullable disable

namespace DatabaseCustomActions.Models
{
    public partial class ExtraPackageDetail
    {
        public ExtraPackageDetail()
        {
            ExtraPackages = new HashSet<ExtraPackage>();
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public int? Minutes { get; set; }
        public int? Messages { get; set; }
        public int? Megabytes { get; set; }
        public decimal? Price { get; set; }

        public virtual ICollection<ExtraPackage> ExtraPackages { get; set; }
    }
}
