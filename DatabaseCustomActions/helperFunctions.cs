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
            public tier_details(bool valid, string id = "", string name = "", string minutes = "", string SMS = "", string megabytes = "")
            {
                this.valid = valid;
                this.id = id;
                this.name = name;
                this.minutes = minutes;
                this.SMS = SMS;
                this.megabytes = megabytes;
            }
            public bool valid { get; set; }
            public string id { get; set; }
            public string name { get; set; }
            public string minutes { get; set; }
            public string SMS { get; set; }
            public string megabytes { get; set; }
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
                _Details = new tier_details(true, reader["id"].ToString(), reader["name"].ToString(), reader["minutes"].ToString(), reader["messages"].ToString(), reader["megabytes"].ToString());
            }
            else
                _Details = new tier_details(false);

            reader.Dispose();
            return _Details;
        }
        public static bool get_package_details(ref package_details package_Details, SqlConnection conn)
        {

            SqlCommand cmd = new SqlCommand($"SELECT  * FROM [dbo].[ExtraPackageDetails] WHERE name ='{package_Details.packageName}';", conn);

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
        public static List<package_details> getAvailablePackages(SqlConnection conn)
        {
            List<package_details> availablePackages = new List<package_details>();
            SqlCommand cmd = new SqlCommand($"SELECT  * FROM [dbo].[ExtraPackageDetails];", conn);

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
        public static List<package_details> getPackages(int neededMinutes, int neededMessages, int neededMegabytes,ref bool exactPackages, SqlConnection conn)
        {
            List<package_details> selectedPackages = new List<package_details>();
            List<package_details> avalaiblePackages = getAvailablePackages(conn);
            package_details megabytesPackage, minutesPackage, messagesPackage;
            megabytesPackage = minutesPackage = messagesPackage = new package_details();
            foreach (var cur in avalaiblePackages)
            {
                if (cur.megabytes == neededMegabytes && cur.minutes == neededMinutes && cur.messages == neededMessages)
                {
                    selectedPackages.Add(cur);
                    return selectedPackages;
                }
                if (cur.packageName == "Text Messages") messagesPackage = cur;
                else if (cur.packageName == "Megabytes") megabytesPackage = cur;
                else if (cur.packageName == "Minutes") minutesPackage = cur;
            }
            exactPackages = neededMessages % messagesPackage.messages == 0 && neededMegabytes % megabytesPackage.megabytes == 0 && neededMinutes % minutesPackage.minutes == 0;
            minutesPackage.times = (neededMinutes + minutesPackage.minutes / 2) / minutesPackage.minutes;
            messagesPackage.times =  (neededMessages + messagesPackage.messages / 2) / messagesPackage.messages;// add  messagesPackage.messages / 2 to the numerator to round the answer
            megabytesPackage.times = (neededMegabytes + megabytesPackage.megabytes / 2) / megabytesPackage.megabytes;

            if (neededMinutes > 0) minutesPackage.times=Math.Max(1, minutesPackage.times);
            if (neededMessages > 0) messagesPackage.times= Math.Max(1, messagesPackage.times);
            if (neededMegabytes > 0) megabytesPackage.times=Math.Max(1, megabytesPackage.times);

            if (messagesPackage.times > 0) selectedPackages.Add(messagesPackage);
            if (megabytesPackage.times > 0) selectedPackages.Add(megabytesPackage);
            if (minutesPackage.times > 0) selectedPackages.Add(minutesPackage);
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
        public static int insert_user(user_details user_Details, SqlConnection conn)
        {
            SqlCommand cmd = new SqlCommand($"insert into [user] values('{user_Details.nationalID}','{user_Details.firstName}','{user_Details.lastName}','{user_Details.birthdate}','{user_Details.streetNo}','{user_Details.streetName}','{user_Details.city}','{user_Details.country}','{user_Details.phoneNumber}');", conn);
            int affected_rows = cmd.ExecuteNonQuery();
            return affected_rows;
        }
        public static int insert_extendPackage(string phoneNumber, string packageName, int times, SqlConnection conn)
        {
            SqlCommand cmd = new SqlCommand($"SELECT  id FROM [dbo].[ExtraPackageDetails] WHERE name ='{packageName}';", conn);
            string packageId = cmd.ExecuteScalar().ToString();
            DateTime _date = DateTime.Now;
            string singleRow = $"('{phoneNumber}','{packageId}','{_date}')";
            string values = singleRow;
            while (times-- > 1)
            {
                values += "," + singleRow;
            }
            string query = $"insert into [ExtraPackage] (phoneNumber,extraPackageID,date) values {values};";
            cmd = new SqlCommand(query, conn);
            int affected_rows = cmd.ExecuteNonQuery();
            return affected_rows;
        }
        public static object get_user_info(string nationalID, SqlConnection conn)
        {
            SqlCommand cmd = new SqlCommand($"SELECT * FROM [dbo].[user] as u JOIN [dbo].[line] as l ON u.phoneNumber = l.phoneNumber JOIN [dbo].[tier_details] as t ON t.id = l.tierID WHERE nationalID = '{nationalID}';", conn);
            var reader = cmd.ExecuteReader();
            user_details _UserInfo;
            if (reader.Read())
            {
                _UserInfo = new user_details(true, reader["nationalID"].ToString(), reader["fname"].ToString(), reader["lname"].ToString(), reader["birthdate"].ToString(), reader["streetNo"].ToString(), reader["streetName"].ToString(), reader["city"].ToString(), reader["country"].ToString(), reader["phoneNumber"].ToString(), reader["name"].ToString());
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
