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
        string connectionString = "Server=tcp:microtel.database.windows.net,1433;Initial Catalog=microtel-db;Persist Security Info=False;User ID=ahmed;Password=123456#Mahmoud;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        string newTierName = newTier.GetValue(dc.State).ToString();
        string phoneNumber = lineNumber.GetValue(dc.State).ToString();


        SqlConnection conn = new SqlConnection(connectionString);
        try
        {
            conn.Open();
            // Get id of the given tier
            tier_details tierDetails = get_tier_details(newTierName, conn);
            Console.WriteLine("Got tier details " + tierDetails);
            // Update tier for the given phone number
            SqlCommand cmd = new SqlCommand($"UPDATE [line] SET [tierID]='{tierDetails.id}' WHERE [phoneNumber]='{phoneNumber}'", conn);
            int affected_rows = cmd.ExecuteNonQuery();
            Console.WriteLine("insert_user_result " + affected_rows);

            if (this.ResultProperty != null)
            {
                dc.State.SetValue(this.ResultProperty.GetValue(dc.State).ToString(), affected_rows);
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