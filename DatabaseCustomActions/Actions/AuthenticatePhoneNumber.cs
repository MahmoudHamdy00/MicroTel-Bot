
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


public class AuthenticatePhoneNumber : Dialog
{
    [JsonConstructor]
    public AuthenticatePhoneNumber([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        : base()
    {
        // enable instances of this command as debug break point
        RegisterSourceLocation(sourceFilePath, sourceLineNumber);
    }

    [JsonProperty("$kind")]
    public const string Kind = "AuthenticatePhoneNumber";

    [JsonProperty("phoneNumber")]
    public ValueExpression phoneNumber { get; set; }

    [JsonProperty("natid")]
    public ValueExpression nationalID { get; set; }

    [JsonProperty("resultProperty")]
    public ValueExpression ResultProperty { get; set; }

    public override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
    {

        User user_info = new User();
        user_info.PhoneNumber = phoneNumber.GetValue(dc.State).ToString();

        bool isValidPhoneNumber = false; // Initialize with default false - phone number doesn't exist
        try
        {
            microteldbContext microteldb = new microteldbContext();
            string nationalID = "";
            isValidPhoneNumber = phoneNumber_checker(user_info.PhoneNumber, ref nationalID, microteldb);

            // if it's not vaild then it will contain the user's number
            if (this.nationalID != null)
            {
                dc.State.SetValue(this.nationalID.GetValue(dc.State).ToString(), nationalID);
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
        }
        return dc.EndDialogAsync(result: isValidPhoneNumber, cancellationToken: cancellationToken);
    }
}