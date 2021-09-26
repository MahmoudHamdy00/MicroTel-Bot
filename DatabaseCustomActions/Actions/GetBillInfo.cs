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

public class GetBillInfo : Dialog
{
    [JsonConstructor]
    public GetBillInfo([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        : base()
    {
        // enable instances of this command as debug break point
        RegisterSourceLocation(sourceFilePath, sourceLineNumber);
    }

    [JsonProperty("$kind")]
    public const string Kind = "GetBillInfo";

    [JsonProperty("lineNumber")]
    public ValueExpression lineNumber { get; set; }

    [JsonProperty("resultProperty")]
    public ValueExpression ResultProperty { get; set; }

    [JsonProperty("billDetails")]
    public ValueExpression BillDetails { get; set; }
    public override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
    {

        string phone_number = lineNumber.GetValue(dc.State).ToString();

        try
        {
            microteldbContext microteldb = new microteldbContext();

            bill_details bill_info = get_latest_bill_details(phone_number, microteldb);
            string detailsMessage = $"Your total bill amount for the current month costs {bill_info.amount} USD." + Environment.NewLine;
            if (bill_info.amount != bill_info.remainingAmount)
                detailsMessage += $" Based on your previous payments, you are required to pay {bill_info.remainingAmount} USD."+ Environment.NewLine;
            detailsMessage += Get_Bill_Details(phone_number, microteldb);
            if (this.ResultProperty != null)
            {
                dc.State.SetValue(this.ResultProperty.GetValue(dc.State).ToString(), bill_info);
            }
            if (this.BillDetails != null)
            {
                dc.State.SetValue(this.BillDetails.GetValue(dc.State).ToString(), detailsMessage);
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
        return dc.EndDialogAsync(cancellationToken: cancellationToken);
    }
}