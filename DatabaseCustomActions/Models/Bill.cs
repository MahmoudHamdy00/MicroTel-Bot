using System;
using System.Collections.Generic;

#nullable disable

namespace DatabaseCustomActions.Models
{
    public partial class Bill
    {
        public Bill()
        {
            Payments = new HashSet<Payment>();
        }

        public Guid Id { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }
        public string PhoneNumber { get; set; }
        public int IsPaid { get; set; }
        public Guid? TeirId { get; set; }

        public virtual Line PhoneNumberNavigation { get; set; }
        public virtual TierDetail Teir { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }
    }
}
