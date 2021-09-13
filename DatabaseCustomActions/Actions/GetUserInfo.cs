using System;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using DatabaseCustomActions;
using DatabaseCustomActions.Models;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using static DatabaseCustomActions.HelperFunctions;

public class GetUserInfo : Dialog
{
    [JsonConstructor]
    public GetUserInfo([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        : base()
    {
        // enable instances of this command as debug break point
        RegisterSourceLocation(sourceFilePath, sourceLineNumber);
    }

    [JsonProperty("$kind")]
    public const string Kind = "GetUserInfo";

    [JsonProperty("natid")]
    public ValueExpression nationalID { get; set; }

    [JsonProperty("tierName")]
    public ValueExpression TierName { get; set; }

    [JsonProperty("resultProperty")]
    public ValueExpression ResultProperty { get; set; }

    public override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
    {

        string national_id = nationalID.GetValue(dc.State).ToString();

        try
        {
            microteldbContext microteldb = new microteldbContext();
            User user_info_obj = microteldb.Users.Where(x => x.NationalId.ToString() == national_id).SingleOrDefault();// get_user_info(national_id, microteldb);
            Guid tierId = (Guid)(microteldb.Lines.Where(x => x.PhoneNumber == user_info_obj.PhoneNumber).SingleOrDefault().TierId);
            string tier = microteldb.TierDetails.Where(x => x.Id == tierId).SingleOrDefault().Name;
            Console.WriteLine(user_info_obj.PhoneNumberNavigation.Tier.Name);
            user_info_obj.PhoneNumberNavigation = null;
            if (this.ResultProperty != null)
            {
                dc.State.SetValue(this.ResultProperty.GetValue(dc.State).ToString(), user_info_obj);
            }
            if (this.TierName != null)
            {
                dc.State.SetValue(this.TierName.GetValue(dc.State).ToString(), tier);
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