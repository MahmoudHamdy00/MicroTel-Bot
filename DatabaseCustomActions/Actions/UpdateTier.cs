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

public class UpdateTier : Dialog
{
    [JsonConstructor]
    public UpdateTier([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        : base()
    {
        // enable instances of this command as debug break point
        RegisterSourceLocation(sourceFilePath, sourceLineNumber);
    }

    [JsonProperty("$kind")]
    public const string Kind = "UpdateTier";

    [JsonProperty("newTier")]
    public ValueExpression newTier { get; set; }

    [JsonProperty("lineNumber")]
    public ValueExpression lineNumber { get; set; }

    [JsonProperty("resultProperty")]
    public ValueExpression ResultProperty { get; set; }

    public override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
    {


        string newTierName = toTitle(newTier.GetValue(dc.State).ToString());
        string phoneNumber = lineNumber.GetValue(dc.State).ToString();


        try
        {
            microteldbContext microteldb = new microteldbContext();

            // Get details of the given tier
            TierDetail tierDetails = get_tier_details(newTierName, microteldb);
            Console.WriteLine("Got tier details " + tierDetails);
            // Update tier for the given phone number
            bool result = update_tier(tierDetails.Id, phoneNumber, microteldb);

            if (this.ResultProperty != null)
            {
                dc.State.SetValue(this.ResultProperty.GetValue(dc.State).ToString(), result);
            }
            microteldb.SaveChanges();
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
        return dc.EndDialogAsync(cancellationToken: cancellationToken);
    }
}