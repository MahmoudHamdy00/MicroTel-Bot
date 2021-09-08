using System;
using System.Collections.Generic;

#nullable disable

namespace DatabaseCustomActions.Models
{
    public partial class Payment
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string CreditCard { get; set; }
        public Guid? BillId { get; set; }

        public virtual Bill Bill { get; set; }
    }
}
