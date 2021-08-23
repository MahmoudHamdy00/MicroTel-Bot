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

public class PayBill : Dialog
{
    [JsonConstructor]
    public PayBill([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        : base()
    {
        // enable instances of this command as debug break point
        RegisterSourceLocation(sourceFilePath, sourceLineNumber);
    }

    [JsonProperty("$kind")]
    public const string Kind = "PayBill";

    [JsonProperty("lineNumber")]
    public ValueExpression lineNumber { get; set; }

    [JsonProperty("creditCard")]
    public ValueExpression creditCard { get; set; }

    [JsonProperty("resultProperty")]
    public ValueExpression ResultProperty { get; set; }

    public override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
    {
        string connectionString = "Server=tcp:microtel.database.windows.net,1433;Initial Catalog=microtel-db;Persist Security Info=False;User ID=ahmed;Password=123456#Mahmoud;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        string phone_number = lineNumber.GetValue(dc.State).ToString();
        string credit_card = creditCard.GetValue(dc.State).ToString();

        SqlConnection conn = new SqlConnection(connectionString);
        bool isPaid = false;
        try
        {
            conn.Open();
            // Get bill detials assosiated to the given phone number 
            bill_details bill_info = get_latest_bill_details(phone_number, conn);

            // If bill was found successfully 
            if (bill_info.exists) {
                // Intiate a new payment for bill 
                string payment_id = insert_payment(bill_info.amount, credit_card, conn);
                isPaid = update_bill_to_paid(bill_info.id, payment_id, conn);
            }

            if (this.ResultProperty != null)
            {
                dc.State.SetValue(this.ResultProperty.GetValue(dc.State).ToString(), isPaid);
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
        return dc.EndDialogAsync(cancellationToken: cancellationToken);
    }
}