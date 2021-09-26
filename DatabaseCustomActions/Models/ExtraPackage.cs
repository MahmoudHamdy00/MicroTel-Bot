using System;
using System.Collections.Generic;

#nullable disable

namespace DatabaseCustomActions.Models
{
    public partial class ExtraPackage
    {
        public Guid Id { get; set; }
        public string PhoneNumber { get; set; }
        public Guid? ExtraPackageId { get; set; }
        public DateTime? Date { get; set; }
        public decimal? TotalPrice { get; set; }

        public virtual ExtraPackageDetail ExtraPackageNavigation { get; set; }
        public virtual Line PhoneNumberNavigation { get; set; }
    }
}
