using System;
using System.Collections.Generic;

#nullable disable

namespace DatabaseCustomActions.Models
{
    public partial class TierDetail
    {
        public TierDetail()
        {
            Bills = new HashSet<Bill>();
            Lines = new HashSet<Line>();
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Minutes { get; set; }
        public int Messages { get; set; }
        public int Megabytes { get; set; }
        public decimal Price { get; set; }

        public virtual ICollection<Bill> Bills { get; set; }
        public virtual ICollection<Line> Lines { get; set; }
    }
}
