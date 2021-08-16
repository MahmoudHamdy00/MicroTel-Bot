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

    [JsonProperty("times")]
    public ValueExpression times { get; set; }

    [JsonProperty("resultProperty")]
    public StringExpression ResultProperty { get; set; }

    public override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
    {
        string connectionString = "Server=tcp:microtel.database.windows.net,1433;Initial Catalog=microtel-db;Persist Security Info=False;User ID=ahmed;Password=123456#Mahmoud;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        string _phoneNumber = phoneNumber.GetValue(dc.State).ToString();
        var data = packageName.GetValue(dc.State);
        int _times = Convert.ToInt32(times.GetValue(dc.State));

        SqlConnection conn = new SqlConnection(connectionString);
        string result = "Failed";//initialize with failed and then change it if it success
        try
        {
            conn.Open();
            int all_affected_rows = 0;
            Newtonsoft.Json.Linq.JArray packageNames;
            if (data.GetType().ToString() == "Newtonsoft.Json.Linq.JArray")
            {
                packageNames = (Newtonsoft.Json.Linq.JArray)data;
                foreach (var curPackage in packageNames)
                {
                    int affected_rows = insert_extendPackage(_phoneNumber, curPackage["packageName"].ToString(), _times, conn);
                    all_affected_rows += affected_rows;
                }
            }
            else
            {
                int affected_rows = insert_extendPackage(_phoneNumber, data.ToString(), _times, conn);
            }
            //  if (all_affected_rows != _times) throw new Exception("Someting went wrong");

            result = "Successfull";
            if (this.ResultProperty != null)
            {
                dc.State.SetValue(this.ResultProperty.GetValue(dc.State), result);
            }
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
        return dc.EndDialogAsync(result: result, cancellationToken: cancellationToken);
    }
}