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

public class VerifyNationalID : Dialog
{
    [JsonConstructor]
    public VerifyNationalID([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        : base()
    {
        // enable instances of this command as debug break point
        RegisterSourceLocation(sourceFilePath, sourceLineNumber);
    }

    [JsonProperty("$kind")]
    public const string Kind = "VerifyNationalID";

    [JsonProperty("natid")]
    public ValueExpression nationalID { get; set; }


   // [JsonProperty("number")]
    //public StringExpression number { get; set; }


    [JsonProperty("resultProperty")]
    public ValueExpression ResultProperty { get; set; }

    public override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
    {
        string connectionString = "Server=tcp:microtel.database.windows.net,1433;Initial Catalog=microtel-db;Persist Security Info=False;User ID=ahmed;Password=123456#Mahmoud;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        user_details user_Details = new user_details();
        user_Details.nationalID = nationalID.GetValue(dc.State).ToString();

        SqlConnection conn = new SqlConnection(connectionString);
        bool result = false;//initialize with failed and then change it if it success
        try
        {
            conn.Open();
            bool nationalId_exist = nationalId_checker(user_Details.nationalID, conn);
            //if it's not vaild then it will contain the user's number
            if (nationalId_exist)
                result = true;
            else
                result = false;

            if (this.ResultProperty != null)
            {
                dc.State.SetValue(this.ResultProperty.GetValue(dc.State).ToString(), result);
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
        return dc.EndDialogAsync(result: result, cancellationToken: cancellationToken);
    }
}