using System;
using System.Collections.Generic;

#nullable disable

namespace DatabaseCustomActions.Models
{
    public partial class Quotum
    {
        public Guid Id { get; set; }
        public int RemainingMinutes { get; set; }
        public int RemainingMessages { get; set; }
        public int RemainingMegabytes { get; set; }
        public DateTime Date { get; set; }

        public virtual Line Line { get; set; }
    }
}
