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

public class ExtendPackage : Dialog
{
    [JsonConstructor]
    public ExtendPackage([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        : base()
    {
        // enable instances of this command as debug break point
        RegisterSourceLocation(sourceFilePath, sourceLineNumber);
    }

    [JsonProperty("$kind")]
    public const string Kind = "ExtendPackage";

    [JsonProperty("phoneNumber")]
    public ValueExpression phoneNumber { get; set; }

    [JsonProperty("packageName")]
    public ValueExpression packageName { get; set; }

    [JsonProperty("resultProperty")]
    public ValueExpression ResultProperty { get; set; }

    [JsonProperty("error")]
    public ValueExpression error { get; set; }

    public override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
    {
        string connectionString = "Server=tcp:microtel.database.windows.net,1433;Initial Catalog=microtel-db;Persist Security Info=False;User ID=ahmed;Password=123456#Mahmoud;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        string _phoneNumber = phoneNumber.GetValue(dc.State).ToString();
        var data = packageName.GetValue(dc.State);

        SqlConnection conn = new SqlConnection(connectionString);
        bool result = false;//initialize with failed and then change it if it success
        int _totalPrice = 0;
        try
        {
            conn.Open();
            int all_affected_rows = 0;
            int megabytes_to_increae = 0;
            int messages_to_increae = 0;
            int minutes_to_increae = 0;
            Newtonsoft.Json.Linq.JArray packageNames;
            if (data.GetType().ToString() == "Newtonsoft.Json.Linq.JArray")
            {
                packageNames = (Newtonsoft.Json.Linq.JArray)data;
                foreach (var curPackage in packageNames)
                {
                    int affected_rows = insert_extendPackage(_phoneNumber, curPackage["packageName"].ToString(), Convert.ToInt32(curPackage["price"]), conn);
                    all_affected_rows += affected_rows;
                    _totalPrice += Convert.ToInt32(curPackage["price"]) * Convert.ToInt32(curPackage["times"]);
                    megabytes_to_increae += Convert.ToInt32(curPackage["megabytes"]);
                    messages_to_increae += Convert.ToInt32(curPackage["messages"]);
                    minutes_to_increae += Convert.ToInt32(curPackage["minutes"]);
                }
            }
            else
            {
                package_details package_Details = new package_details();
                package_Details.packageName = data.ToString();
                bool is_ok = get_package_details(ref package_Details, conn);
                if (!is_ok) throw new Exception("There isn't any package with this name");
                int affected_rows = insert_extendPackage(_phoneNumber, package_Details.packageName, package_Details.price, conn);
                _totalPrice = package_Details.price;
                megabytes_to_increae = package_Details.megabytes;
                messages_to_increae = package_Details.megabytes;
                minutes_to_increae = package_Details.megabytes;
            }
            // Get user bill details for the current month 
            bill_details bill_info = get_latest_bill_details(_phoneNumber, conn);
            if (!bill_info.exists) throw new Exception("There is no bill record for this user");
            // Calculate bill's total amount 
            double total_amount = bill_info.amount + _totalPrice;
            //  if (all_affected_rows != _times) throw new Exception("Someting went wrong");
            if (!update_bill_amount(bill_info.id, total_amount, conn)) throw new Exception("Error with update bill amount method");
            if (bill_info.isPaid == 2) {
                if (!update_bill_state(bill_info.id, 1, conn)) throw new Exception("Error with update bill state method");
            }
            if (!update_quota(_phoneNumber, minutes_to_increae, messages_to_increae, megabytes_to_increae, conn)) throw new Exception("Error with Update_Bill method");
            result = true;

        }
        catch (Exception ex)
        {
            if (this.error != null)
            {
                dc.State.SetValue(this.error.GetValue(dc.State).ToString(), ex.Message);
            }
            result = false;
        }
        finally
        {
            conn.Close();
        }
        if (this.ResultProperty != null)
        {
            dc.State.SetValue(this.ResultProperty.GetValue(dc.State).ToString(), result);
        }
        return dc.EndDialogAsync(result: result, cancellationToken: cancellationToken);
    }
}