using System;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using DatabaseCustomActions;
using DatabaseCustomActions.Models;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using static DatabaseCustomActions.HelperFunctions;

public class AuthenticateNationalID : Dialog
{
    [JsonConstructor]
    public AuthenticateNationalID([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        : base()
    {
        // enable instances of this command as debug break point
        RegisterSourceLocation(sourceFilePath, sourceLineNumber);
    }

    [JsonProperty("$kind")]
    public const string Kind = "AuthenticateNationalID";

    [JsonProperty("natid")]
    public ValueExpression nationalID { get; set; }


   // [JsonProperty("number")]
    //public StringExpression number { get; set; }


    [JsonProperty("resultProperty")]
    public ValueExpression ResultProperty { get; set; }

    public override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
    {
        // Extract connection string from env variables 

        user_details user_Details = new user_details();
        user_Details.nationalID = nationalID.GetValue(dc.State).ToString();

        bool result = false;//initialize with failed and then change it if it success
        try
        {
            microteldbContext microteldb = new microteldbContext();
            bool nationalId_exist = nationalId_checker(user_Details.nationalID, microteldb);
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
        }
        return dc.EndDialogAsync(result: result, cancellationToken: cancellationToken);
    }
}