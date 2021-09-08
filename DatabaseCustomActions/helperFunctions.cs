using System;
using System.Globalization;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using DatabaseCustomActions.Models;

namespace DatabaseCustomActions
{
    public class EnvironmentVariables
    {
        public string connectionString = "Data Source=microtel.database.windows.net;Initial Catalog=microtel-db;Persist Security Info=True;User ID=ahmed;Password=123456#Mahmoud";
    }


    class HelperFunctions
    {
        public struct tier_details
        {
            public tier_details(bool valid, string id = "", string name = "", int minutes = 0, int SMS = 0, int megabytes = 0, double price = 0)
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
            public double price { get; set; }

        }
        public struct package_details
        {
            public package_details(string packageName = "", int minutes = 0, int messages = 0, int megabytes = 0, int price = 0, int times = 1)
            {
                this.packageName = packageName;
                this.minutes = minutes;
                this.messages = messages;
                this.megabytes = megabytes;
                this.price = price;
                this.times = times;
            }
            public string packageName { get; set; }
            public int minutes { get; set; }
            public int messages { get; set; }
            public int megabytes { get; set; }
            public int price { get; set; }
            public int times { get; set; }// to store how many package to subscrib in when extend package
        }
        public struct user_details
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
        }

        public struct bill_details
        {
            public bill_details(bool exists = false, string id = "", DateTime dueDate = new DateTime(), double amount = 0, string phoneNumber = "", int isPaid = 0, double remainingAmount = 0)
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
            public double amount { get; set; }
            public string phoneNumber { get; set; }
            public int isPaid { get; set; }
            public double remainingAmount { get; set; }

        }
        public static int getPhoneNumber()
        {
            Random rnd = new Random();
            int ret = rnd.Next(100000, 999999);
            return ret;
        }
        public static bool phoneNumber_checker(string phoneNumber, ref string nationalID, SqlConnection conn)
        {
            SqlCommand cmd = new SqlCommand($"SELECT [nationalID] FROM [user] WHERE [phoneNumber]='{phoneNumber}'", conn);
            var result = cmd.ExecuteScalar();
            bool phoneNumberExists = false;
            if (result != null)
            {
                phoneNumberExists = true;
                nationalID = result.ToString();
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
        public static bool nationalId_checker(string natID, microteldbContext microteldb)
        {
            return microteldb.Users.Find(Convert.ToInt32( natID)) != null;
        }
        public static tier_details get_tier_details(string tierName, microteldbContext microteldb)
        {
            TierDetail tierDetail = microteldb.TierDetails.Where(x => x.Name == tierName).SingleOrDefault();
            tier_details _Details;
            if (tierDetail == null) _Details = new tier_details(false);
            else
            {
                TierDetail tier = (TierDetail)(tierDetail);
                _Details = new tier_details(true, tier.Id.ToString(), tier.Name, tier.Minutes, tier.Messages, tier.Megabytes, Convert.ToDouble(tier.Price));
            }
            return _Details;
        }
        public static bool get_package_details(ref package_details package_Details, SqlConnection conn)
        {

            SqlCommand cmd = new SqlCommand($"SELECT  * FROM [dbo].[extra_package_details] WHERE name = '{package_Details.packageName}';", conn);

            var reader = cmd.ExecuteReader();

            if (reader.HasRows)
            {
                reader.Read();
                package_Details = new package_details(reader["name"].ToString(), Convert.ToInt32(reader["minutes"]), Convert.ToInt32(reader["messages"]), Convert.ToInt32(reader["megabytes"]), Convert.ToInt32(reader["price"]));
                reader.Dispose();
                return true;
            }
            return false;
        }
        public static double get_paid_amount(string bill_id, SqlConnection conn)
        {
            SqlCommand cmd = new SqlCommand($"SELECT SUM(amount) AS total_amount FROM [dbo].[payment] WHERE billID='{bill_id}';", conn);
            return Convert.ToDouble(cmd.ExecuteScalar());
        }
        public static double calc_remaining_amount(string bill_id, double total_amount, int is_paid, SqlConnection conn)
        {
            double paid_amount = 0;
            // If bill was partially paid, calc the amount paid from the bill
            if (is_paid == 1)
            {
                paid_amount = get_paid_amount(bill_id, conn);
            }
            else if (is_paid == 2)
            {
                paid_amount = total_amount;
            }
            return total_amount - paid_amount;
        }
        public static bill_details get_latest_bill_details(string phoneNUmber, SqlConnection conn)
        {
            SqlCommand cmd = new SqlCommand($"SELECT TOP(1) * FROM [dbo].[bill] WHERE phoneNumber ='{phoneNUmber}' ORDER BY dueDate DESC;", conn);

            var reader = cmd.ExecuteReader();

            bill_details _Details;
            if (reader.Read())
            {
                string bill_id = reader["id"].ToString();
                double amount = Convert.ToDouble(reader["amount"]);
                int is_paid = Convert.ToInt32(reader["isPaid"]);
                string phone_number = reader["phoneNumber"].ToString();
                DateTime due_date = Convert.ToDateTime(reader["dueDate"]);
                reader.Dispose();

                double remaining_amount = calc_remaining_amount(bill_id, amount, is_paid, conn);

                _Details = new bill_details(true, bill_id, due_date, amount, phone_number, is_paid, remaining_amount);
            }
            else
            {
                reader.Dispose();
                _Details = new bill_details(false);
            }
            return _Details;
        }

        public static List<package_details> getAvailablePackages(SqlConnection conn)
        {
            List<package_details> availablePackages = new List<package_details>();
            SqlCommand cmd = new SqlCommand($"SELECT  * FROM [dbo].[extra_package_details];", conn);

            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                package_details package_Details = new package_details(reader["name"].ToString(), Convert.ToInt32(reader["minutes"]), Convert.ToInt32(reader["messages"]), Convert.ToInt32(reader["megabytes"]), Convert.ToInt32(reader["price"]));
                availablePackages.Add(package_Details);
            }
            reader.Dispose();
            return availablePackages;
        }
        public static List<package_details> mainGetBestPackages(int minutes, int messages, int megabytes, SqlConnection conn)
        {
            List<package_details> selectedPackages = new List<package_details>();
            List<package_details> temp = new List<package_details>();
            List<package_details> availablePackages = getAvailablePackages(conn);
            int bestPrice = 10000;
            getBestPackages(minutes, messages, megabytes, 0, ref bestPrice, ref selectedPackages, ref temp, ref availablePackages);
            return selectedPackages;
        }
        public static void getBestPackages(int minutes, int messages, int megabytes, int price, ref int bestPrice, ref List<package_details> selectedPackages, ref List<package_details> temp, ref List<package_details> availablePackages)
        {
            if (minutes <= 0 && messages <= 0 && megabytes <= 0)
            {
                if (price < bestPrice)
                {
                    bestPrice = price;
                    selectedPackages = new List<package_details>(temp);
                }
                return;
            }
            //   if (price > 50) return;
            foreach (var cur in availablePackages)
            {

                if (cur.packageName == "Minutes" && minutes <= 0) continue;
                if (cur.packageName == "Text Messages" && messages <= 0) continue;
                if (cur.packageName == "Megabytes" && megabytes <= 0) continue;
                temp.Add(cur);
                getBestPackages(minutes - cur.minutes, messages - cur.messages, megabytes - cur.megabytes, price + cur.price, ref bestPrice, ref selectedPackages, ref temp, ref availablePackages);
                temp.Remove(cur);
            }

        }
        public static List<package_details> getPackages(int neededMinutes, int neededMessages, int neededMegabytes, ref bool found, ref Dictionary<string, List<int>> map, SqlConnection conn)
        {
            List<package_details> selectedPackages = new List<package_details>();
            List<package_details> avalaiblePackages = getAvailablePackages(conn);
            foreach (var cur in avalaiblePackages)
            {
                if (cur.megabytes == neededMegabytes && cur.minutes == neededMinutes && cur.messages == neededMessages)
                {
                    selectedPackages.Clear();
                    selectedPackages.Add(cur);
                    found = true;
                    return selectedPackages;
                }
                if (cur.packageName.Contains("Text Messages"))
                {
                    if (neededMessages == cur.messages)
                    {
                        selectedPackages.Add(cur);
                        neededMessages = -1;
                    }
                    if (map.ContainsKey("Text Messages"))
                        map["Text Messages"].Add(cur.messages);
                    else
                        map.Add("Text Messages", new List<int>() { cur.messages });

                }
                else if (cur.packageName.Contains("Megabytes"))
                {
                    if (neededMegabytes == cur.megabytes)
                    {
                        selectedPackages.Add(cur);
                        neededMegabytes = -1;
                    }

                    if (map.ContainsKey("Megabytes"))
                        map["Megabytes"].Add(cur.megabytes);
                    else
                        map.Add("Megabytes", new List<int>() { cur.megabytes });

                }
                else if (cur.packageName.Contains("Minutes"))
                {
                    if (neededMinutes == cur.minutes)
                    {
                        selectedPackages.Add(cur);
                        neededMinutes = -1;
                    }

                    if (map.ContainsKey("Minutes"))
                        map["Minutes"].Add(cur.minutes);
                    else
                        map.Add("Minutes", new List<int>() { cur.minutes });

                }

            }
            if (neededMinutes <= 0 & neededMessages <= 0 && neededMegabytes <= 0) found = true;
            map["Minutes"].Sort();
            map["Megabytes"].Sort();
            map["Text Messages"].Sort();
            return selectedPackages;
        }
        public static bool update_tier(string tier_id, string phoneNumber, SqlConnection conn)
        {

            SqlCommand cmd = new SqlCommand($"UPDATE [line] SET [tierID]='{tier_id}' WHERE [phoneNumber]='{phoneNumber}'", conn);
            int affected_rows = cmd.ExecuteNonQuery();
            if (affected_rows == 1)
                return true;
            else
                return false;

        }
        public static string insert_quota(tier_details tierDetails, microteldbContext microteldb)
        {
            Quotum quota = new Quotum
            {
                Id = Guid.NewGuid(),
                RemainingMinutes = tierDetails.minutes,
                RemainingMessages = tierDetails.SMS,
                RemainingMegabytes = tierDetails.megabytes,
                Date = DateTime.Now
            };
            microteldb.Quota.Add(quota);
            return quota.Id.ToString();

        }
        public static int insert_line(string _phoneNumber, string tierId, string quotaID, microteldbContext microteldb)
        {
            Line line = new Line
            {
                PhoneNumber = _phoneNumber,
                TierId = Guid.Parse(tierId),
                QuotaId = Guid.Parse(quotaID)
            };
            microteldb.Lines.Add(line);
            return 1;
        }
        public static int insert_bill(string tierID, double price, string phoneNumber, microteldbContext microteldb)
        {
            Bill bill = new Bill
            {
                DueDate = DateTime.Now.AddDays(30),
                Amount = Convert.ToDecimal(price),
                PhoneNumber = phoneNumber,
                TeirId = Guid.Parse(tierID)
            };
            microteldb.Bills.Add(bill);
            return 1;
        }
        public static bool insert_user(user_details user_Details, microteldbContext microteldb)
        {
            User user = new User
            {
                NationalId = Convert.ToInt32(user_Details.nationalID),
                FName = user_Details.firstName,
                LName = user_Details.lastName,
                BirthDate = Convert.ToDateTime(user_Details.birthdate),
                StreetNo = Convert.ToInt32(user_Details.streetNo),
                StreetName = user_Details.streetName,
                City = user_Details.city,
                Country = user_Details.country,
                PhoneNumber = user_Details.phoneNumber,
            };
            microteldb.Users.Add(user);
            return true;
        }
        public static int insert_extendPackage(string phoneNumber, string packageName, int price, SqlConnection conn)
        {
            SqlCommand cmd = new SqlCommand($"SELECT  id FROM [dbo].[extra_package_details] WHERE name ='{packageName}';", conn);
            string packageId = cmd.ExecuteScalar().ToString();
            DateTime _date = DateTime.Now;
            string query = $"insert into [extra_package] (phoneNumber,extraPackageID,date,totalPrice) values ('{phoneNumber}','{packageId}','{_date}','{ price}');";
            cmd = new SqlCommand(query, conn);
            int affected_rows = cmd.ExecuteNonQuery();
            return affected_rows;
        }
        public static bool insert_payment(string bill_id, double amount, string credit_card, SqlConnection conn)
        {
            DateTime _date = DateTime.Now;
            SqlCommand cmd = new SqlCommand($"INSERT INTO [payment] (date, amount, creditCard, billID) VALUES ('{_date}', '{amount}', '{credit_card}', '{bill_id}');", conn);
            int affected_rows = cmd.ExecuteNonQuery();
            return affected_rows == 1;
        }
        public static bool update_bill_state(string bill_id, int state, SqlConnection conn)
        {
            SqlCommand cmd = new SqlCommand($"UPDATE [bill] SET [bill].isPaid={state} WHERE [bill].id='{bill_id}';", conn);
            int affected_rows = cmd.ExecuteNonQuery();
            return affected_rows == 1;
        }
        public static bool update_bill_amount(string bill_id, double amount, SqlConnection conn)
        {
            /*// Get user bill details for the current month 
            bill_details bill_info = get_latest_bill_details(phoneNumber, conn);
            if (!bill_info.exists) throw new Exception("There is no bill record for this user");

            // Update bill total amount with the new amount 
            double total_amount = bill_info.amount + amount;*/

            SqlCommand cmd = new SqlCommand($"UPDATE [bill] SET [bill].amount={amount} WHERE id = '{bill_id}';", conn);
            int affected_rows = cmd.ExecuteNonQuery();
            return affected_rows == 1;
        }

        public static bool update_quota(string phoneNumber, int minutes, int messages, int megabytes, SqlConnection conn)
        {
            SqlCommand cmd = new SqlCommand($"SELECT [quotaID] FROM [line] WHERE [phoneNumber]='{phoneNumber}';", conn);
            var res = cmd.ExecuteScalar();
            if (res == null) throw new Exception("There is no quota recored for this user");
            string quotaId = res.ToString();

            cmd = new SqlCommand($"SELECT [remainingMinutes],[remainingMessages],[remainingMegabytes] FROM [quota] WHERE [id]='{quotaId}';", conn);
            SqlDataReader reader = cmd.ExecuteReader();
            int currentMinutes = 0, currentMessages = 0, currentMegabytes = 0;
            if (reader.Read())
            {
                currentMinutes += Convert.ToInt32(reader["remainingMinutes"]);
                currentMessages += Convert.ToInt32(reader["remainingMessages"]);
                currentMegabytes += Convert.ToInt32(reader["remainingMegabytes"]);
                reader.Dispose();
            }
            else throw new Exception("There is no quota record for this user");
            cmd = new SqlCommand($"UPDATE [quota] set [remainingMinutes] = {currentMinutes + minutes} ,[remainingMessages]={currentMessages + messages} ,[remainingMegabytes] ={currentMegabytes + megabytes} WHERE id='{quotaId}';", conn);
            int affected_rows = cmd.ExecuteNonQuery();
            return affected_rows == 1;
        }
        public static object get_user_info(string nationalID, SqlConnection conn)
        {
            SqlCommand cmd = new SqlCommand($"SELECT * FROM [dbo].[user] as u JOIN [dbo].[line] as l ON u.phoneNumber = l.phoneNumber JOIN [dbo].[tier_details] as t ON t.id = l.tierID WHERE nationalID = '{nationalID}';", conn);
            SqlDataReader reader = cmd.ExecuteReader();
            user_details _UserInfo;
            if (reader.Read())
            {
                _UserInfo = new user_details(true, reader["nationalID"].ToString(), reader["fname"].ToString(), reader["lname"].ToString(), Convert.ToDateTime(reader["birthdate"]).ToString("yyyy-MM-dd"), reader["streetNo"].ToString(), reader["streetName"].ToString(), reader["city"].ToString(), reader["country"].ToString(), reader["phoneNumber"].ToString(), reader["name"].ToString());
            }
            else
            {
                _UserInfo = new user_details(false);
            }
            reader.Dispose();
            return _UserInfo;
        }
        public static string toTitle(string word)
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            return textInfo.ToTitleCase(word.ToLower());
        }
    }

}
