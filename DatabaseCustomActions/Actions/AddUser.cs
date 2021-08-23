﻿using System;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using DatabaseCustomActions;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using static DatabaseCustomActions.helperFunctions;

public class AddUser : Dialog
{
    [JsonConstructor]
    public AddUser([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        : base()
    {
        // enable instances of this command as debug break point
        RegisterSourceLocation(sourceFilePath, sourceLineNumber);
    }

    [JsonProperty("$kind")]
    public const string Kind = "AddUser";

    [JsonProperty("natid")]
    public ValueExpression nationalID { get; set; }

    [JsonProperty("fname")]
    public ValueExpression firstName { get; set; }

    [JsonProperty("lname")]
    public ValueExpression lastName { get; set; }

    [JsonProperty("birthdate")]
    public ValueExpression birthdate { get; set; }


    [JsonProperty("streetNo")]
    public ValueExpression streetNo { get; set; }

    [JsonProperty("streetName")]
    public ValueExpression streetName { get; set; }

    [JsonProperty("city")]
    public ValueExpression city { get; set; }

    [JsonProperty("country")]
    public ValueExpression country { get; set; }

    [JsonProperty("tier")]
    public ValueExpression tier { get; set; }


    [JsonProperty("number")]
    public StringExpression number { get; set; }


    [JsonProperty("resultProperty")]
    public StringExpression ResultProperty { get; set; }

    public override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
    {
        string connectionString = "Server=tcp:microtel.database.windows.net,1433;Initial Catalog=microtel-db;Persist Security Info=False;User ID=ahmed;Password=123456#Mahmoud;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        user_details user_Details = new user_details();
        user_Details.nationalID = nationalID.GetValue(dc.State).ToString();
        user_Details.firstName = toTitle(firstName.GetValue(dc.State).ToString());
        user_Details.lastName = toTitle(lastName.GetValue(dc.State).ToString());
        user_Details.birthdate = birthdate.GetValue(dc.State).ToString();
        user_Details.streetNo = streetNo.GetValue(dc.State).ToString();
        user_Details.streetName = toTitle(streetName.GetValue(dc.State).ToString());
        user_Details.city = toTitle(city.GetValue(dc.State).ToString());
        user_Details.country = toTitle(country.GetValue(dc.State).ToString());
        user_Details.phoneNumber = "010" + getPhoneNumber().ToString();

        string _tier = toTitle(tier.GetValue(dc.State).ToString());
        //SqlConnection conn = new SqlConnection("Data Source=MININT-5B89IPO\\SQLEXPRESs;Initial Catalog=microDBB;Integrated Security=True");

        SqlConnection conn = new SqlConnection(connectionString);
        bool userAdded = false; //initialize with failed and then change it if it success
        try
        {
            conn.Open();
            bool nationalId_exist = nationalId_checker(user_Details.nationalID, conn);
            // if national id already register, don't add register a new user
            if (nationalId_exist)
            {
                userAdded = false;
                Console.WriteLine(nationalId_exist + " is already registered.");
                throw new Exception("User with the same national id is already registered");
            }
            Console.WriteLine(nationalId_exist);
            
            // get tier detailes;
            tier_details tierDetails = get_tier_details(_tier, conn);
            if (!tierDetails.valid)
            {
                userAdded = false;
                Console.WriteLine("Invalid tier");
                throw new Exception("Invalid tier");
            }

            // Create a qouta for the new user
            string quotaID = insert_quota(tierDetails, conn);
            Console.WriteLine(quotaID);

            // Insert line details for the new user
            var insert_line_result = insert_line(user_Details.phoneNumber, tierDetails.id, quotaID, conn);
            Console.WriteLine("insert_line_result " + insert_line_result);

            // Create new bill for the user
            var insert_bill_result = insert_bill(tierDetails.id, tierDetails.price, user_Details.phoneNumber, conn);

            // Insert the new user's details 
            userAdded = insert_user(user_Details, conn);

            if (this.number != null)
            {
                dc.State.SetValue(this.number.GetValue(dc.State), user_Details.phoneNumber);
            }
            if (this.ResultProperty != null)
            {
                dc.State.SetValue(this.ResultProperty.GetValue(dc.State), userAdded);
            }
        }
        catch (Exception ex)
        {
            if (this.ResultProperty != null)
            {
                dc.State.SetValue(this.ResultProperty.GetValue(dc.State), ex.Message);
            }
        }
        finally
        {
            conn.Close();
        }
        return dc.EndDialogAsync(result: userAdded, cancellationToken: cancellationToken);
    }
}