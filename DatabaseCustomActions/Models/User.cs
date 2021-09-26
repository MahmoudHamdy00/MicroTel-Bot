using System;
using System.Collections.Generic;

#nullable disable

namespace DatabaseCustomActions.Models
{
    public partial class User
    {
        public DateTime? Timestamp { get; set; }
        public int NationalId { get; set; }
        public string FName { get; set; }
        public string LName { get; set; }
        public DateTime BirthDate { get; set; }
        public int StreetNo { get; set; }
        public string StreetName { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string PhoneNumber { get; set; }

        public virtual Line PhoneNumberNavigation { get; set; }
    }
}
