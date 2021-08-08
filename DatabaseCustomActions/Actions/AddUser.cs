using System;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;

public class AddUser : Dialog
{
    [JsonConstructor]
    public AddUser([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        : base()
    {
        // enable instances of this command as debug break point
        RegisterSourceLocation(sourceFilePath, sourceLineNumber);
    }

    [JsonProperty("$kind")]
    public const string Kind = "AddUser";

    [JsonProperty("fname")]
    public StringExpression firstName { get; set; }

    [JsonProperty("lname")]
    public StringExpression lastName { get; set; }

    [JsonProperty("birthdate")]
    public StringExpression birthdate { get; set; }

    [JsonProperty("natid")]
    public StringExpression nationalID { get; set; }

    [JsonProperty("address")]
    public ValueExpression address { get; set; }


    [JsonProperty("tier")]
    public StringExpression tier { get; set; }

    [JsonProperty("resultProperty")]
    public StringExpression ResultProperty { get; set; }

    public override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
    {
        var _firstName = firstName.GetValue(dc.State);
        var _lastName = lastName.GetValue(dc.State);
        var _birthdate = birthdate.GetValue(dc.State);
        var _nationalID = nationalID.GetValue(dc.State);
        var _address = address.GetValue(dc.State);
        var _tier = tier.GetValue(dc.State);
        SqlConnection conn = new SqlConnection("Data Source=MININT-5B89IPO\\SQLEXPRESs;Initial Catalog=microDBB;Integrated Security=True");
        conn.Open();
        SqlCommand cmd = new SqlCommand($"insert into users values('{_firstName}','{_lastName}','{_birthdate}','{_nationalID}','{_address}','{_tier}');", conn);
        var affectedRows = cmd.ExecuteNonQuery();
        conn.Close();
        var result = "successfully added user :)";
        if (affectedRows == 0)
            result = "faild to add user :( ";

        if (this.ResultProperty != null)
        {
            dc.State.SetValue(this.ResultProperty.GetValue(dc.State), result);
        }

        return dc.EndDialogAsync(result: result, cancellationToken: cancellationToken);
    }
}