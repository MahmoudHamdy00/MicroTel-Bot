
using System;
using System.Data.SqlClient;

namespace DatabaseCustomActions
{
    class helperFunctions
    {
        public struct tier_details
        {
            public tier_details(bool valid, string id = "", string minutes = "", string SMS = "", string megabytes = "")
            {
                this.valid = valid;
                this.id = id;
                this.minutes = minutes;
                this.SMS = SMS;
                this.megabytes = megabytes;
            }
            public bool valid { get; set; }
            public string id { get; set; }
            public string minutes { get; set; }
            public string SMS { get; set; }
            public string megabytes { get; set; }
        }
        public struct package_details
        {
            public package_details(string packageName = "", string minutes = "", string messages = "", string megabytes = "", string price = "")
            {
                this.packageName = packageName;
                this.minutes = minutes;
                this.messages = messages;
                this.megabytes = megabytes;
                this.price = price;
            }
            public string packageName { get; set; }
            public string minutes { get; set; }
            public string messages { get; set; }
            public string megabytes { get; set; }
            public string price { get; set; }
        }
        public struct user_details
        {
            public user_details(string nationalID = "", string firstName = "", string lastName = "", string birthdate = "", string streetNo = "", string streetName = "", string city = "", string country = "", string phoneNumber = "")
            {
                this.nationalID = nationalID;
                this.firstName = firstName;
                this.lastName = lastName;
                this.birthdate = birthdate;
                this.streetNo = streetNo;
                this.streetName = streetName;
                this.city = city;
                this.country = country;
                this.phoneNumber = phoneNumber;
            }
            public string nationalID { get; set; }
            public string firstName { get; set; }
            public string lastName { get; set; }
            public string birthdate { get; set; }
            public string streetNo { get; set; }
            public string streetName { get; set; }
            public string city { get; set; }
            public string country { get; set; }
            public string phoneNumber { get; set; }
        }
        public static int getPhoneNumber()
        {
            Random rnd = new Random();
            int ret = rnd.Next(100000, 999999);
            return ret;
        }
        public static bool phoneNumber_checker(string phoneNumber, ref string nationalID, SqlConnection conn)
        {
            SqlCommand cmd = new SqlCommand($"SELECT  [nationalID] FROM [user] WHERE [phoneNumber]='{phoneNumber}'", conn);
            var reader = cmd.ExecuteScalar();
            bool result = false;
            if (reader != null)
            {
                result = true;
                nationalID = reader.ToString();
            }
            //  reader.Close();
            //reader.Dispose();
            return result;
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
            SqlCommand cmd = new SqlCommand($"SELECT  * FROM [user] WHERE [nationalID]='{natID}'", conn);
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
            Console.WriteLine("b res");

            var reader = cmd.ExecuteReader();

            Console.WriteLine("afte");
            tier_details _Details;
            if (reader.Read())
            {
                _Details = new tier_details(true, reader["id"].ToString(), reader["minutes"].ToString(), reader["messages"].ToString(), reader["megabytes"].ToString());
            }
            else
                _Details = new tier_details(false);
            //  reader.Close();
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
                package_Details = new package_details(reader["name"].ToString(), reader["minutes"].ToString(), reader["messages"].ToString(), reader["megabytes"].ToString(), reader["price"].ToString());
                reader.Dispose();
                return true;
            }
            return false;
        }
        public static string insert_quota(tier_details tierDetails, SqlConnection conn)
        {

            DateTime _date = DateTime.Now.AddMonths(1);
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
            DateTime _date = DateTime.Now.AddMonths(1);
            string singleRow = $"('{phoneNumber}','{packageId}','{_date}')";
            string values = singleRow;
            while (times-- > 1)
            {
                values += "," + singleRow;
            }
            string query = $"insert into [ExtraPackage] (phoneNumber,extraPackageID,date) values {values};";
            Console.WriteLine(values);
            Console.WriteLine(query);
            cmd = new SqlCommand(query, conn);
            int affected_rows = cmd.ExecuteNonQuery();
            return affected_rows;
        }
    }

}
