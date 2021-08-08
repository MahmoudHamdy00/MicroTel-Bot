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

    [JsonProperty("natid")]
    public ValueExpression nationalID { get; set; }

    [JsonProperty("fname")]
    public ValueExpression firstName { get; set; }

    [JsonProperty("lname")]
    public ValueExpression lastName { get; set; }

    [JsonProperty("birthdate")]
    public ValueExpression birthdate { get; set; }


    [JsonProperty("streetNo")]
    public ValueExpression streetNo { get; set; }

    [JsonProperty("streetName")]
    public ValueExpression streetName { get; set; }

    [JsonProperty("city")]
    public ValueExpression city { get; set; }

    [JsonProperty("country")]
    public ValueExpression country { get; set; }




    [JsonProperty("resultProperty")]
    public StringExpression ResultProperty { get; set; }

    public override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
    {
        var _nationalID = nationalID.GetValue(dc.State);
        var _firstName = firstName.GetValue(dc.State);
        var _lastName = lastName.GetValue(dc.State);
        var _birthdate = birthdate.GetValue(dc.State);
        var _streetNo = streetNo.GetValue(dc.State);
        var _streetName = streetName.GetValue(dc.State);
        var _city = city.GetValue(dc.State);
        var _country = country.GetValue(dc.State);
        var _phoneNumber = 01012;//getPhoneNumber();

        //SqlConnection conn = new SqlConnection("Data Source=MININT-5B89IPO\\SQLEXPRESs;Initial Catalog=microDBB;Integrated Security=True");
        SqlConnection conn = new SqlConnection("Server=tcp:microtel.database.windows.net,1433;Initial Catalog=microtel-db;Persist Security Info=False;User ID=ahmed;Password=123456#Mahmoud;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
        conn.Open();
    //    SqlCommand cmd = new SqlCommand($"insert into users values('{_nationalID},{_firstName}','{_lastName}','{_birthdate}','{_streetNo}','{_streetName}','{_city}','{_country},'{_phoneNumber}'');", conn);
        var affectedRows = 1;// cmd.ExecuteNonQuery();
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