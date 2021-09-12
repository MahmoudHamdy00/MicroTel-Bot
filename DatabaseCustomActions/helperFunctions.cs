using System;
using System.Globalization;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using DatabaseCustomActions.Models;

namespace DatabaseCustomActions
{
    class HelperFunctions
    {
        /*   public struct tier_details
           {
               public tier_details(bool valid, string id = "", string name = "", int minutes = 0, int SMS = 0, int megabytes = 0, decimal price = 0)
               {
                   this.valid = valid;
                   this.id = id;
                   this.name = name;
                   this.minutes = minutes;
                   this.SMS = SMS;
                   this.megabytes = megabytes;
                   this.price = price;
               }
               public bool valid { get; set; }
               public string id { get; set; }
               public string name { get; set; }
               public int minutes { get; set; }
               public int SMS { get; set; }
               public int megabytes { get; set; }
               public decimal price { get; set; }

           }*/
        //public struct package_details
        //{
        //    public package_details(string packageName = "", int minutes = 0, int messages = 0, int megabytes = 0, decimal price = 0, int times = 1)
        //    {
        //        this.packageName = packageName;
        //        this.minutes = minutes;
        //        this.messages = messages;
        //        this.megabytes = megabytes;
        //        this.price = price;
        //        this.times = times;
        //    }
        //    public string packageName { get; set; }
        //    public int minutes { get; set; }
        //    public int messages { get; set; }
        //    public int megabytes { get; set; }
        //    public decimal price { get; set; }
        //    public int times { get; set; }// to store how many package to subscrib in when extend package
        //}
        /*   public struct user_details
           {
               public user_details(bool exists, string nationalID = "", string firstName = "", string lastName = "", string birthdate = "", string streetNo = "", string streetName = "", string city = "", string country = "", string phoneNumber = "", string tierName = "")
               {
                   this.exists = exists;
                   this.nationalID = nationalID;
                   this.firstName = firstName;
                   this.lastName = lastName;
                   this.birthdate = birthdate;
                   this.streetNo = streetNo;
                   this.streetName = streetName;
                   this.city = city;
                   this.country = country;
                   this.phoneNumber = phoneNumber;
                   this.tierName = tierName;
               }
               public bool exists { get; set; }
               public string nationalID { get; set; }
               public string firstName { get; set; }
               public string lastName { get; set; }
               public string birthdate { get; set; }
               public string streetNo { get; set; }
               public string streetName { get; set; }
               public string city { get; set; }
               public string country { get; set; }
               public string phoneNumber { get; set; }
               public string tierName { get; set; }
           }*/

        public struct bill_details
        {
            public bill_details(bool exists = false, string id = "", DateTime dueDate = new DateTime(), decimal amount = 0, string phoneNumber = "", int isPaid = 0, decimal remainingAmount = 0)
            {
                this.exists = exists;
                this.id = id;
                this.dueDate = dueDate;
                this.amount = amount;
                this.phoneNumber = phoneNumber;
                this.isPaid = isPaid;
                this.remainingAmount = remainingAmount;
            }
            public bool exists { get; set; }
            public string id { get; set; }
            public DateTime dueDate { get; set; }
            public decimal amount { get; set; }
            public string phoneNumber { get; set; }
            public int isPaid { get; set; }
            public decimal remainingAmount { get; set; }

        }
        public static int getPhoneNumber()
        {
            Random rnd = new Random();
            int ret = rnd.Next(100000, 999999);
            return ret;
        }
        public static bool phoneNumber_checker(string phoneNumber, ref string nationalID, microteldbContext microteldb)
        {
            User user = microteldb.Users.Where(x => x.PhoneNumber == phoneNumber).SingleOrDefault();
            bool phoneNumberExists = false;
            if (user != null)
            {
                phoneNumberExists = true;
                nationalID = user.NationalId.ToString();
            }
            return phoneNumberExists;
        }
        /// <summary>
        /// check if there is a user with the same national id or not
        /// </summary>
        /// <returns>
        /// if there is a user with the same national id return his number 
        /// else return 1;
        /// </returns>
        public static bool nationalId_checker(int natID, microteldbContext microteldb)
        {
            return microteldb.Users.Find(natID) != null;
        }
        public static TierDetail get_tier_details(string tierName, microteldbContext microteldb)
        {
            TierDetail tierDetail = microteldb.TierDetails.Where(x => x.Name == tierName).SingleOrDefault();
            TierDetail _Details;
            if (tierDetail == null) _Details = null;
            else
            {
                TierDetail tier = (TierDetail)(tierDetail);
                _Details = new TierDetail
                {
                    Id = tier.Id,
                    Name = tier.Name,
                    Minutes = tier.Minutes,
                    Messages = tier.Messages,
                    Megabytes = tier.Megabytes,
                    Price = tier.Price
                };
            }
            return _Details;
        }
        public static bool get_package_details(ref ExtraPackageDetail package_Details, microteldbContext microteldb)
        {
            string name = package_Details.Name;
            ExtraPackageDetail extraPackageDetail = microteldb.ExtraPackageDetails.Where(x => x.Name == name).SingleOrDefault();
            if (extraPackageDetail != null)
            {
                package_Details = new ExtraPackageDetail
                {
                    Name = extraPackageDetail.Name,
                    Minutes = extraPackageDetail.Minutes,
                    Messages = extraPackageDetail.Messages,
                    Megabytes = extraPackageDetail.Megabytes,
                    Price = extraPackageDetail.Price
                };
                return true;
            }
            return false;
        }
        public static decimal get_paid_amount(string bill_id, microteldbContext microteldb)
        {
            //SqlCommand cmd = new SqlCommand($"SELECT SUM(amount) AS total_amount FROM [dbo].[payment] WHERE billID='{bill_id}';", conn);
            /*return Convert.ToDouble(cmd.ExecuteScalar());*/
#warning review;

            return microteldb.Payments.Where(x => x.BillId.ToString() == bill_id).Sum(x => x.Amount);
        }
        public static decimal calc_remaining_amount(string bill_id, decimal total_amount, int is_paid, microteldbContext microteldb)
        {
            decimal paid_amount = 0;
            // If bill was partially paid, calc the amount paid from the bill
            if (is_paid == 1)
            {
                paid_amount = get_paid_amount(bill_id, microteldb);
            }
            else if (is_paid == 2)
            {
                paid_amount = total_amount;
            }
            return total_amount - paid_amount;
        }
        public static bill_details get_latest_bill_details(string phoneNumber, microteldbContext microteldb)
        {
            DateTime date = microteldb.Bills.Where(x => x.PhoneNumber == phoneNumber).Max(x => x.DueDate);
            //SqlCommand cmd = new SqlCommand($"SELECT TOP(1) * FROM [dbo].[bill] WHERE phoneNumber ='{phoneNumber}' ORDER BY dueDate DESC;", conn);

#warning review this;
            Bill bill = microteldb.Bills.Where(x => x.DueDate == date && x.PhoneNumber == phoneNumber).SingleOrDefault();


            bill_details _Details;
            if (bill != null)
            {
                string bill_id = bill.Id.ToString();
                decimal amount = bill.Amount;
                int is_paid = Convert.ToInt32(bill.IsPaid);
                string phone_number = bill.PhoneNumber.ToString();
                DateTime due_date = Convert.ToDateTime(bill.DueDate);

                decimal remaining_amount = calc_remaining_amount(bill_id, amount, is_paid, microteldb);

                _Details = new bill_details(true, bill_id, due_date, amount, phone_number, is_paid, remaining_amount);
            }
            else
            {
                _Details = new bill_details(false);
            }
            return _Details;
        }
        public static string Get_Bill_Details(string phoneNumber, microteldbContext microteldb)
        {
            DateTime date = microteldb.Bills.Where(x => x.PhoneNumber == phoneNumber).Max(x => x.DueDate);
            //SqlCommand cmd = new SqlCommand($"SELECT TOP(1) * FROM [dbo].[bill] WHERE phoneNumber ='{phoneNumber}' ORDER BY dueDate DESC;", conn);

#warning review this;
            Bill bill = microteldb.Bills.Where(x => x.DueDate == date && x.PhoneNumber == phoneNumber).SingleOrDefault();

            List<ExtraPackage> extraPackages;
            List<Payment> payments = microteldb.Payments.Where(x => x.BillId == bill.Id).ToList();
            bool is_there_payment = payments != null && payments.Count() != 0;
            string detailsMessage = "Bill Details:-" + Environment.NewLine;
            if (is_there_payment)
            {
                DateTime last_payment_date = payments.Max(x => x.Date);
                Console.WriteLine(date.ToString());

                extraPackages = microteldb.ExtraPackages.Where(x => x.PhoneNumber == phoneNumber && x.Date > last_payment_date).ToList();
            }
            else
            {
                extraPackages = microteldb.ExtraPackages.Where(x => x.PhoneNumber == phoneNumber).ToList();
                TierDetail tierDetail = microteldb.TierDetails.Where(x => x.Id == (microteldb.Lines.Where(x => x.PhoneNumber == phoneNumber).SingleOrDefault().TierId)).SingleOrDefault();
                detailsMessage += "- Main Tier Name: *" + tierDetail.Name + "*, ";
                detailsMessage += "Due date: *" + bill.DueDate.ToString("MMM dd, yyyy") + "*, ";
                detailsMessage += "Price: *" + tierDetail.Price.ToString() + "*" + Environment.NewLine;
                detailsMessage += Environment.NewLine;
            }
            Console.WriteLine(DateTime.Now.ToString());
            if (extraPackages != null && extraPackages.Count() != 0)
            {
                detailsMessage += "- Unpaid Extra Packages:-" + Environment.NewLine;
                foreach (ExtraPackage cur in extraPackages)
                {
                    detailsMessage += " 1. Package Name: *" + microteldb.ExtraPackageDetails.Where(x => x.Id == cur.ExtraPackageId).SingleOrDefault().Name + "*, ";
                    detailsMessage += "Subscription date: *" + Convert.ToDateTime(cur.Date).ToString("MMM dd, yyyy") + "*, ";
                    detailsMessage += "Price: *" + cur.TotalPrice.ToString() + "*" + Environment.NewLine;
                }
            }
            return detailsMessage;
        }

        public static List<ExtraPackageDetail> getAvailablePackages(microteldbContext microteldb)
        {
            List<ExtraPackageDetail> availablePackage = new List<ExtraPackageDetail>();
            List<ExtraPackageDetail> availablePackages = microteldb.ExtraPackageDetails.ToList();

            foreach (ExtraPackageDetail package in availablePackages)
            {
                ExtraPackageDetail package_Details = new ExtraPackageDetail
                {
                    Name = package.Name,
                    Minutes = package.Minutes,
                    Messages = package.Messages,
                    Megabytes = package.Megabytes,
                    Price = package.Price
                };
                availablePackage.Add(package_Details);
            }
            return availablePackage;
        }
        public static List<ExtraPackageDetail> mainGetBestPackages(int minutes, int messages, int megabytes, microteldbContext microteldb)
        {
            List<ExtraPackageDetail> selectedPackages = new List<ExtraPackageDetail>();
            List<ExtraPackageDetail> temp = new List<ExtraPackageDetail>();
            List<ExtraPackageDetail> availablePackages = getAvailablePackages(microteldb);
            decimal bestPrice = 10000;
            getBestPackages(minutes, messages, megabytes, 0, ref bestPrice, ref selectedPackages, ref temp, ref availablePackages);
            return selectedPackages;
        }
        public static void getBestPackages(int minutes, int messages, int megabytes, decimal price, ref decimal bestPrice, ref List<ExtraPackageDetail> selectedPackages, ref List<ExtraPackageDetail> temp, ref List<ExtraPackageDetail> availablePackages)
        {
            if (minutes <= 0 && messages <= 0 && megabytes <= 0)
            {
                if (price < bestPrice)
                {
                    bestPrice = price;
                    selectedPackages = new List<ExtraPackageDetail>(temp);
                }
                return;
            }
            //   if (price > 50) return;
            foreach (var cur in availablePackages)
            {

                if (cur.Name == "Minutes" && minutes <= 0) continue;
                if (cur.Name == "Text Messages" && messages <= 0) continue;
                if (cur.Name == "Megabytes" && megabytes <= 0) continue;
                temp.Add(cur);
                getBestPackages(Convert.ToInt32(minutes - cur.Minutes), Convert.ToInt32(messages - cur.Messages), Convert.ToInt32(megabytes - cur.Megabytes), Convert.ToDecimal(price + cur.Price), ref bestPrice, ref selectedPackages, ref temp, ref availablePackages);
                temp.Remove(cur);
            }

        }
        public static List<ExtraPackageDetail> getPackages(int neededMinutes, int neededMessages, int neededMegabytes, ref bool found, ref Dictionary<string, List<int>> map, microteldbContext microteldb)
        {
            List<ExtraPackageDetail> selectedPackages = new List<ExtraPackageDetail>();
            List<ExtraPackageDetail> avalaiblePackages = getAvailablePackages(microteldb);
            foreach (var cur in avalaiblePackages)
            {
                if (cur.Megabytes == neededMegabytes && cur.Minutes == neededMinutes && cur.Messages == neededMessages)
                {
                    selectedPackages.Clear();
                    selectedPackages.Add(cur);
                    found = true;
                    return selectedPackages;
                }
                if (cur.Name.Contains("Text Messages"))
                {
                    if (neededMessages == cur.Messages)
                    {
                        selectedPackages.Add(cur);
                        neededMessages = -1;
                    }
                    if (map.ContainsKey("Text Messages"))
                        map["Text Messages"].Add(Convert.ToInt32(cur.Messages));
                    else
                        map.Add("Text Messages", new List<int>() { Convert.ToInt32(cur.Messages) });

                }
                else if (cur.Name.Contains("Megabytes"))
                {
                    if (neededMegabytes == cur.Megabytes)
                    {
                        selectedPackages.Add(cur);
                        neededMegabytes = -1;
                    }

                    if (map.ContainsKey("Megabytes"))
                        map["Megabytes"].Add(Convert.ToInt32(cur.Megabytes));
                    else
                        map.Add("Megabytes", new List<int>() { Convert.ToInt32(cur.Megabytes) });

                }
                else if (cur.Name.Contains("Minutes"))
                {
                    if (neededMinutes == Convert.ToInt32(cur.Minutes))
                    {
                        selectedPackages.Add(cur);
                        neededMinutes = -1;
                    }

                    if (map.ContainsKey("Minutes"))
                        map["Minutes"].Add(Convert.ToInt32(cur.Minutes));
                    else
                        map.Add("Minutes", new List<int>() { Convert.ToInt32(cur.Minutes) });

                }

            }
            if (neededMinutes <= 0 & neededMessages <= 0 && neededMegabytes <= 0) found = true;
            map["Minutes"].Sort();
            map["Megabytes"].Sort();
            map["Text Messages"].Sort();
            return selectedPackages;
        }
        public static bool update_tier(Guid tier_id, string phoneNumber, microteldbContext microteldb)
        {
            Line line = microteldb.Lines.Where(x => x.PhoneNumber == phoneNumber).SingleOrDefault();
            line.TierId = tier_id;
            return true;
        }
        public static Guid insert_quota(TierDetail tierDetails, microteldbContext microteldb)
        {
            Quotum quota = new Quotum
            {
                Id = Guid.NewGuid(),
                RemainingMinutes = tierDetails.Minutes,
                RemainingMessages = tierDetails.Messages,
                RemainingMegabytes = tierDetails.Megabytes,
                Date = DateTime.Now
            };
            microteldb.Quota.Add(quota);
            return quota.Id;

        }
        public static int insert_line(string _phoneNumber, Guid tierId, Guid quotaID, microteldbContext microteldb)
        {
            Line line = new Line
            {
                PhoneNumber = _phoneNumber,
                TierId = tierId,
                QuotaId = quotaID
            };
            microteldb.Lines.Add(line);
            return 1;
        }
        public static int insert_bill(Guid tierID, decimal price, string phoneNumber, microteldbContext microteldb)
        {
            Bill bill = new Bill
            {
                DueDate = DateTime.Now.AddDays(30),
                Amount = Convert.ToDecimal(price),
                PhoneNumber = phoneNumber,
                TeirId = tierID
            };
            microteldb.Bills.Add(bill);
            return 1;
        }
        public static bool insert_user(User user_Details, microteldbContext microteldb)
        {
            microteldb.Users.Add(user_Details);
            return true;
        }
        public static int insert_extendPackage(string phoneNumber, string packageName, decimal price, microteldbContext microteldb)
        {
            ExtraPackageDetail extraPackageDetail = microteldb.ExtraPackageDetails.Where(x => x.Name == packageName).SingleOrDefault();
            string packageId = extraPackageDetail.Id.ToString();
            microteldb.ExtraPackages.Add(new ExtraPackage
            {
                Id = Guid.NewGuid(),
                PhoneNumber = phoneNumber,
                ExtraPackageId = Guid.Parse(packageId),
                Date = DateTime.Now,
                TotalPrice = Convert.ToDecimal(price),
            });
            return 1;
        }
        public static bool insert_payment(string bill_id, decimal amount, string credit_card, microteldbContext microteldb)
        {
            microteldb.Payments.Add(new Payment
            {
                Date = DateTime.Now,
                Amount = Convert.ToDecimal(amount),
                CreditCard = credit_card,
                BillId = Guid.Parse(bill_id)
            });
            return true;
        }
        public static bool update_bill_state(string bill_id, int state, microteldbContext microteldb)
        {
            Bill bill = microteldb.Bills.Where(x => x.Id.ToString() == bill_id).SingleOrDefault();
            bill.IsPaid = state;
            return true;
        }
        public static bool update_bill_amount(string bill_id, decimal amount, microteldbContext microteldb)
        {
            /*// Get user bill details for the current month 
            bill_details bill_info = get_latest_bill_details(phoneNumber, conn);
            if (!bill_info.exists) throw new Exception("There is no bill record for this user");

            // Update bill total amount with the new amount 
            double total_amount = bill_info.amount + amount;*/
            Bill bill = microteldb.Bills.Where(x => x.Id.ToString() == bill_id).SingleOrDefault();
            bill.Amount = Convert.ToDecimal(amount);
            /* SqlCommand cmd = new SqlCommand($"UPDATE [bill] SET [bill].amount={amount} WHERE id = '{bill_id}';", conn);
             int affected_rows = cmd.ExecuteNonQuery();*/
            return true;
        }

        public static bool update_quota(string phoneNumber, int minutes, int messages, int megabytes, microteldbContext microteldb)
        {
            Line line = microteldb.Lines.Where(x => x.PhoneNumber == phoneNumber).SingleOrDefault();
            if (line == null) throw new Exception("There is no quota recored for this user");
            string quotaId = line.QuotaId.ToString();

            Quotum quota = microteldb.Quota.Where(x => x.Id.ToString() == quotaId).SingleOrDefault();
            if (quota != null)
            {
                quota.RemainingMinutes += minutes;
                quota.RemainingMessages += messages;
                quota.RemainingMegabytes += megabytes;
            }
            else throw new Exception("There is no quota record for this user");

            return true;
        }
        public static User get_user_info(string nationalID, microteldbContext microteldb)
        {
            return microteldb.Users.Where(x => x.NationalId.ToString() == nationalID).SingleOrDefault();
            // string tierName = microteldb.TierDetails.Where(x => x.Id == microteldb.Lines.Where(x => x.PhoneNumber == user.PhoneNumber).SingleOrDefault().TierId).SingleOrDefault().Name;
            //SqlCommand cmd = new SqlCommand($"SELECT * FROM [dbo].[user] as u JOIN [dbo].[line] as l ON u.phoneNumber = l.phoneNumber JOIN [dbo].[tier_details] as t ON t.id = l.tierID WHERE nationalID = '{nationalID}';", conn);
            /*  User _UserInfo;
              if (user != null)
              {
                  _UserInfo = new user(true, user.NationalId.ToString(), user.FName, user.LName, user.BirthDate.ToString("yyyy-MM-dd"), user.StreetNo.ToString(), user.StreetName, user.City, user.Country, user.PhoneNumber, tierName);
              }
              else
              {
                  _UserInfo = new user_details(false);
              }
              return _UserInfo;*/
        }
        public static string toTitle(string word)
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            return textInfo.ToTitleCase(word.ToLower());
        }
    }

}
