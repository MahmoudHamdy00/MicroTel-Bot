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

    [JsonProperty("resultProperty")]
    public StringExpression ResultProperty { get; set; }

    public override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
    {
        string connectionString = "Server=tcp:microtel.database.windows.net,1433;Initial Catalog=microtel-db;Persist Security Info=False;User ID=ahmed;Password=123456#Mahmoud;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        package_details package_Details = new package_details();
        package_Details.packageName = packageName.GetValue(dc.State).ToString();

        SqlConnection conn = new SqlConnection(connectionString);
        string result = "Failed";//initialize with failed and then change it if it success
        try
        {
            conn.Open();
            bool validPackage = get_package_details(ref package_Details, conn);
            if (!validPackage) throw new Exception("Someting went wrong");
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
            result = "Successfull";
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