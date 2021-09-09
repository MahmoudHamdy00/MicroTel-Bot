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

        string _phoneNumber = phoneNumber.GetValue(dc.State).ToString();
        var data = packageName.GetValue(dc.State);

        bool result = false;//initialize with failed and then change it if it success
        decimal _totalPrice = 0;
        try
        {
            microteldbContext microteldb = new microteldbContext();

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
                    int affected_rows = insert_extendPackage(_phoneNumber, curPackage["Name"].ToString(), Convert.ToInt32(curPackage["Price"]), microteldb);
                    all_affected_rows += affected_rows;
                    _totalPrice += Convert.ToInt32(curPackage["Price"]) ;
                    megabytes_to_increae += Convert.ToInt32(curPackage["Megabytes"]);
                    messages_to_increae += Convert.ToInt32(curPackage["Messages"]);
                    minutes_to_increae += Convert.ToInt32(curPackage["Minutes"]);
                }
            }
            else
            {
                ExtraPackageDetail package_Details = new ExtraPackageDetail();
                package_Details.Name = data.ToString();
                bool is_ok = get_package_details(ref package_Details, microteldb);
                if (!is_ok) throw new Exception("There isn't any package with this name");
                int affected_rows = insert_extendPackage(_phoneNumber, package_Details.Name, Convert.ToDecimal(package_Details.Price), microteldb);
                _totalPrice = Convert.ToDecimal( package_Details.Price);
                megabytes_to_increae =Convert.ToInt32( package_Details.Megabytes);
                messages_to_increae = Convert.ToInt32(package_Details.Messages);
                minutes_to_increae = Convert.ToInt32(package_Details.Minutes);
            }
            // Get user bill details for the current month 
            bill_details bill_info = get_latest_bill_details(_phoneNumber, microteldb);
            if (!bill_info.exists) throw new Exception("There is no bill record for this user");
            // Calculate bill's total amount 
            decimal total_amount = bill_info.amount + _totalPrice;
            //  if (all_affected_rows != _times) throw new Exception("Someting went wrong");
            if (!update_bill_amount(bill_info.id, total_amount, microteldb)) throw new Exception("Error with update bill amount method");
            if (bill_info.isPaid == 2)
            {
                if (!update_bill_state(bill_info.id, 1, microteldb)) throw new Exception("Error with update bill state method");
            }
            if (!update_quota(_phoneNumber, minutes_to_increae, messages_to_increae, megabytes_to_increae, microteldb)) throw new Exception("Error with Update_Bill method");
            result = true;
            microteldb.SaveChanges();
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
        }
        if (this.ResultProperty != null)
        {
            dc.State.SetValue(this.ResultProperty.GetValue(dc.State).ToString(), result);
        }
        return dc.EndDialogAsync(result: result, cancellationToken: cancellationToken);
    }
}