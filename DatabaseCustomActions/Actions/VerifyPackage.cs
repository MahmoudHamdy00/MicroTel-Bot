using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using DatabaseCustomActions;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using static DatabaseCustomActions.helperFunctions;

public class VerifyPackage : Dialog
{
    [JsonConstructor]
    public VerifyPackage([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        : base()
    {
        // enable instances of this command as debug break point
        RegisterSourceLocation(sourceFilePath, sourceLineNumber);
    }

    [JsonProperty("$kind")]
    public const string Kind = "VerifyPackage";

    [JsonProperty("packageName")]
    public ValueExpression packageName { get; set; }

    [JsonProperty("minutes")]
    public ValueExpression minutes { get; set; }

    [JsonProperty("messages")]
    public ValueExpression messages { get; set; }

    [JsonProperty("megabytes")]
    public ValueExpression megabytes { get; set; }


    [JsonProperty("price")]
    public ValueExpression price { get; set; }

    [JsonProperty("packages")]
    public StringExpression packages { get; set; }


    [JsonProperty("resultProperty")]
    public StringExpression ResultProperty { get; set; }

    public override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
    {
        string connectionString = "Server=tcp:microtel.database.windows.net,1433;Initial Catalog=microtel-db;Persist Security Info=False;User ID=ahmed;Password=123456#Mahmoud;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        var data = packageName.GetValue(dc.State);

        SqlConnection conn = new SqlConnection(connectionString);
        var result = "Failed";//initialize with failed and then change it if it success
        try
        {
            conn.Open();
            Newtonsoft.Json.Linq.JArray packageNames;
            package_details package_Details = new package_details();
            if (data.GetType().ToString() == "Newtonsoft.Json.Linq.JArray")
            {
                packageNames = (Newtonsoft.Json.Linq.JArray)data;
                foreach (var cur in packageNames)
                {
                    Console.WriteLine(cur["amount"][0].ToString() + " : " + cur["unit"][0].ToString());
                    if (cur["unit"][0].ToString() == "minutes") package_Details.minutes += Convert.ToInt32(cur["amount"][0]);
                    else if (cur["unit"][0].ToString() == "gigabyte") package_Details.megabytes += Convert.ToInt32(cur["amount"][0]) * 1000;
                    else if (cur["unit"][0].ToString() == "megabyte") package_Details.megabytes += Convert.ToInt32(cur["amount"][0]);
                    else if (cur["unit"][0].ToString() == "messages") package_Details.messages += Convert.ToInt32(cur["amount"][0]);
                }
                //     List<package_details> selectedPackages = mainGetBestPackages(package_Details.minutes, package_Details.messages, package_Details.megabytes, conn);
                List<package_details> selectedPackages = getPackages(package_Details.minutes, package_Details.messages, package_Details.megabytes, conn);

                package_Details.minutes = package_Details.megabytes = package_Details.messages = 0;
                foreach (var package in selectedPackages)
                {
                    package_Details.minutes += package.minutes;
                    package_Details.megabytes += package.megabytes;
                    package_Details.messages += package.messages;
                    package_Details.price += package.price;
                }
                if (this.packages != null)
                {
                    dc.State.SetValue(this.packages.GetValue(dc.State), selectedPackages);
                }
            }
            else
            {
                package_Details.packageName = packageName.GetValue(dc.State).ToString();
                bool validPackage = get_package_details(ref package_Details, conn);
                if (!validPackage) throw new Exception("Someting went wrong");
                if (this.packages != null)
                {
                    dc.State.SetValue(this.packages.GetValue(dc.State), package_Details.packageName);
                }
            }
            if (this.minutes != null)
            {
                dc.State.SetValue(this.minutes.GetValue(dc.State).ToString(), package_Details.minutes);
            }
            if (this.messages != null)
            {
                dc.State.SetValue(this.messages.GetValue(dc.State).ToString(), package_Details.messages);
            }
            if (this.megabytes != null)
            {
                dc.State.SetValue(this.megabytes.GetValue(dc.State).ToString(), package_Details.megabytes);
            }
            if (this.price != null)
            {
                dc.State.SetValue(this.price.GetValue(dc.State).ToString(), package_Details.price);
            }
            result = "successfull";
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
        if (this.ResultProperty != null)
        {
            dc.State.SetValue(this.ResultProperty.GetValue(dc.State), result);
        }
        return dc.EndDialogAsync(result: result, cancellationToken: cancellationToken);
    }
}