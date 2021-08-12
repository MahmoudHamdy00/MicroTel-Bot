
using System;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using DatabaseCustomActions;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using static DatabaseCustomActions.helperFunctions;


public class VerifyPhoneNumber : Dialog
{
    [JsonConstructor]
    public VerifyPhoneNumber([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        : base()
    {
        // enable instances of this command as debug break point
        RegisterSourceLocation(sourceFilePath, sourceLineNumber);
    }

    [JsonProperty("$kind")]
    public const string Kind = "VerifyPhoneNumber";

    [JsonProperty("phoneNumber")]
    public ValueExpression phoneNumber { get; set; }

    [JsonProperty("resultProperty")]
    public ValueExpression ResultProperty { get; set; }

    public override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
    {
        string connectionString = "Server=tcp:microtel.database.windows.net,1433;Initial Catalog=microtel-db;Persist Security Info=False;User ID=ahmed;Password=123456#Mahmoud;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        user_details user_info = new user_details();
        user_info.phoneNumber = phoneNumber.GetValue(dc.State).ToString();

        SqlConnection conn = new SqlConnection(connectionString);
        bool isValidPhoneNumber = false; // Initialize with default false - phone number doesn't exist
        try
        {
            conn.Open();
            bool phoneNumber_exist = phoneNumber_checker(user_info.phoneNumber, conn);
            //if it's not vaild then it will contain the user's number
            if (phoneNumber_exist) {
                // Change result to indicate phone number exists 
                isValidPhoneNumber = true;
                // Get nationalID and tier assosiated to given phone number
            }

            if (this.ResultProperty != null)
            {
                dc.State.SetValue(this.ResultProperty.GetValue(dc.State).ToString(), isValidPhoneNumber);
            }
        }
        catch (Exception ex)
        {
            if (this.ResultProperty != null)
            {
                dc.State.SetValue(this.ResultProperty.GetValue(dc.State).ToString(), ex.Message);
            }
        }
        finally
        {
            conn.Close();
        }
        return dc.EndDialogAsync(result: isValidPhoneNumber, cancellationToken: cancellationToken);
    }
}