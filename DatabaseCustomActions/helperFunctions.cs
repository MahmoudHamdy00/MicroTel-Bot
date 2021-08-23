using System;
using System.Globalization;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace DatabaseCustomActions
{
    class helperFunctions
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
            public bill_details(bool exists = false, string id = "", DateTime dueDate = new DateTime(), double amount = 0, string phoneNumber = "", bool isPaid = false)
            {
                this.exists = exists;
                this.id = id;
                this.dueDate = dueDate;
                this.amount = amount;
                this.phoneNumber = phoneNumber;
                this.isPaid = isPaid;
            }
            public bool exists { get; set; }
            public string id { get; set; }
            public DateTime dueDate { get; set; }
            public double amount { get; set; }
            public string phoneNumber { get; set; }
            public bool isPaid { get; set; }

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
        public static bool nationalId_checker(string natID, SqlConnection conn)
        {
            SqlCommand cmd = new SqlCommand($"SELECT * FROM [user] WHERE [nationalID]='{natID}'", conn);
            var reader = cmd.ExecuteReader();
            bool result = false;
            if (reader.Read()) result = true;
            //  reader.Close();
            reader.Dispose();
            return result;
        }
        public static tier_details get_tier_details(string tier, SqlConnection conn)
        {

            SqlCommand cmd = new SqlCommand($"SELECT  * FROM [dbo].[tier_details] WHERE name ='{tier}';", conn);

            var reader = cmd.ExecuteReader();

            tier_details _Details;
            if (reader.Read())
            {
                _Details = new tier_details(true, reader["id"].ToString(), reader["name"].ToString(), Convert.ToInt32(reader["minutes"]), Convert.ToInt32(reader["messages"]), Convert.ToInt32(reader["megabytes"]), Convert.ToDouble(reader["price"]));
            }
            else
                _Details = new tier_details(false);

            reader.Dispose();
            return _Details;
        }
        public static bool get_package_details(ref package_details package_Details, SqlConnection conn)
        {

            SqlCommand cmd = new SqlCommand($"SELECT  * FROM [dbo].[extra_package_details] WHERE name ='{package_Details.packageName}';", conn);

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
        public static bill_details get_latest_bill_details(string phoneNUmber, SqlConnection conn)
        {

            SqlCommand cmd = new SqlCommand($"SELECT  TOP(1) * FROM [dbo].[bill] WHERE phoneNumber ='{phoneNUmber}' ORDER BY dueDate DESC;", conn);

            var reader = cmd.ExecuteReader();

            bill_details _Details;
            if (reader.Read())
            {
                _Details = new bill_details(true, reader["id"].ToString(), Convert.ToDateTime(reader["dueDate"]), Convert.ToDouble(reader["amount"]), reader["phoneNumber"].ToString(), Convert.ToBoolean(reader["isPaid"]));
            }
            else
                _Details = new bill_details(false);

            reader.Dispose();
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
        public static string insert_quota(tier_details tierDetails, SqlConnection conn)
        {
            DateTime _date = DateTime.Now;
            SqlCommand cmd = new SqlCommand($"insert into quota (remainingMinutes,remainingMessages,remainingMegabytes,date) OUTPUT INSERTED.id values('{tierDetails.minutes}','{tierDetails.SMS}','{tierDetails.megabytes}','{_date}');", conn);
            string quotaID = cmd.ExecuteScalar().ToString();
            return quotaID;
        }
        public static int insert_line(string _phoneNumber, string tierId, string quotaID, SqlConnection conn)
        {
            SqlCommand cmd = new SqlCommand($"insert into line values('{_phoneNumber}','{tierId}','{quotaID}');", conn);
            var affected_rows = cmd.ExecuteNonQuery();
            return affected_rows;
        }
        public static int insert_bill(string tierID, double price, string phoneNumber, SqlConnection conn)
        {
            DateTime _dueDate = DateTime.Now.AddDays(30);
            SqlCommand cmd = new SqlCommand($"insert into [dbo].[bill] (dueDate, amount, phoneNumber, teirID) VALUES ('{_dueDate}','{price}','{phoneNumber}','{tierID}');", conn);
            var affected_rows = cmd.ExecuteNonQuery();
            return affected_rows;
        }
        public static bool insert_user(user_details user_Details, SqlConnection conn)
        {
            SqlCommand cmd = new SqlCommand($"insert into [dbo].[user] (nationalID,fName,lName,birthDate,streetNo,streetName,city,country,phoneNumber) values('{user_Details.nationalID}','{user_Details.firstName}','{user_Details.lastName}','{user_Details.birthdate}','{user_Details.streetNo}','{user_Details.streetName}','{user_Details.city}','{user_Details.country}','{user_Details.phoneNumber}');", conn);
            int affected_rows = cmd.ExecuteNonQuery();
            return affected_rows == 1;
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
        public static string insert_payment(double amount, string credit_card, SqlConnection conn)
        {
            DateTime _date = DateTime.Now;
            SqlCommand cmd = new SqlCommand($"INSERT INTO [payment] (date, amount, creditCard) OUTPUT INSERTED.id VALUES ('{_date}', '{amount}', '{credit_card}');", conn);
            string payment_id = cmd.ExecuteScalar().ToString();
            Console.WriteLine(payment_id);
            return payment_id;
        }
        // to update the bill's price to anew one(when extend package is occured)
        public static bool Update_Bill(string phoneNumber, int price, SqlConnection conn)
        {
            SqlCommand cmd = new SqlCommand($"SELECT TOP 1 [amount] FROM [bill] WHERE [phoneNumber]='{phoneNumber}' ORDER BY [dueDate] DESC;", conn);
            var res = cmd.ExecuteScalar();
            if (res == null) throw new Exception("There is no bill record for this user");
            int currentPrice = Convert.ToInt32(res);

            cmd = new SqlCommand($"UPDATE [bill] set [bill].amount = {currentPrice + price} WHERE id=(SELECT TOP 1 [id] FROM [bill] WHERE [phoneNumber]={phoneNumber} ORDER BY [dueDate] DESC);", conn);
            int affected_rows = cmd.ExecuteNonQuery();
            return affected_rows == 1;
        }
        public static bool update_bill_to_paid(string bill_id, string payment_id, SqlConnection conn)
        {
            SqlCommand cmd = new SqlCommand($"UPDATE [bill] SET [bill].isPaid=1, [bill].paymentID='{payment_id}' WHERE [bill].id='{bill_id}';", conn);
            int affected_rows = cmd.ExecuteNonQuery();
            return affected_rows == 1;
        }
        public static bool Update_Quota(string phoneNumber, int minutes, int messages, int megabytes, SqlConnection conn)
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
            var reader = cmd.ExecuteReader();
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
