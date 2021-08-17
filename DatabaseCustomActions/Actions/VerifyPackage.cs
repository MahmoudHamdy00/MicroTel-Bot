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
    public ValueExpression PackageName { get; set; }

    [JsonProperty("minutes")]
    public ValueExpression Minutes { get; set; }

    [JsonProperty("messages")]
    public ValueExpression Messages { get; set; }

    [JsonProperty("megabytes")]
    public ValueExpression Megabytes { get; set; }


    [JsonProperty("price")]
    public ValueExpression Price { get; set; }

    [JsonProperty("packages")]
    public ValueExpression Packages { get; set; }


    [JsonProperty("exactPackage")]
    public ValueExpression ExactPackage { get; set; }

    [JsonProperty("packagesDetails")]
    public ValueExpression PackagesDeatails { get; set; }

    [JsonProperty("resultProperty")]
    public ValueExpression ResultProperty { get; set; }


    [JsonProperty("error")]
    public ValueExpression Error { get; set; }

    public override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
    {
        string connectionString = "Server=tcp:microtel.database.windows.net,1433;Initial Catalog=microtel-db;Persist Security Info=False;User ID=ahmed;Password=123456#Mahmoud;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        var data = PackageName.GetValue(dc.State);

        SqlConnection conn = new SqlConnection(connectionString);
        bool result = false;//initialize with failed and then change it if it success
        try
        {
            conn.Open();
            Newtonsoft.Json.Linq.JArray packageNames;
            package_details package_Details = new package_details();
            bool _exactPackage = false;
            string confirmationMessage = "";
            if (data.GetType().ToString() == "Newtonsoft.Json.Linq.JArray")
            {
                packageNames = (Newtonsoft.Json.Linq.JArray)data;
                foreach (var cur in packageNames)
                {
                    Console.WriteLine(cur["amount"][0].ToString() + " : " + cur["unit"][0].ToString());
                    var haha = cur["unit"][0];
                    if (cur["unit"][0].GetType().ToString() == "Newtonsoft.Json.Linq.JArray")
                    {
                        if (cur["unit"][0][0].ToString() == "minutes") package_Details.minutes += Convert.ToInt32(cur["amount"][0]);
                        else if (cur["unit"][0][0].ToString() == "gigabyte") package_Details.megabytes += Convert.ToInt32(cur["amount"][0]) * 1000;
                        else if (cur["unit"][0][0].ToString() == "megabyte") package_Details.megabytes += Convert.ToInt32(cur["amount"][0]);
                        else if (cur["unit"][0][0].ToString() == "messages") package_Details.messages += Convert.ToInt32(cur["amount"][0]);
                        continue;
                    }
                    if (cur["unit"][0].ToString() == "minutes") package_Details.minutes += Convert.ToInt32(cur["amount"][0]);
                    else if (cur["unit"][0].ToString() == "gigabyte") package_Details.megabytes += Convert.ToInt32(cur["amount"][0]) * 1000;
                    else if (cur["unit"][0].ToString() == "megabyte") package_Details.megabytes += Convert.ToInt32(cur["amount"][0]);
                    else if (cur["unit"][0].ToString() == "messages") package_Details.messages += Convert.ToInt32(cur["amount"][0]);

                }
                //     List<package_details> selectedPackages = mainGetBestPackages(package_Details.minutes, package_Details.messages, package_Details.megabytes, conn);

                List<package_details> selectedPackages = getPackages(package_Details.minutes, package_Details.messages, package_Details.megabytes, ref _exactPackage, conn);

                package_Details.minutes = package_Details.megabytes = package_Details.messages = 0;
                int i = 0;
                foreach (var package in selectedPackages)
                {
                    package_Details.minutes += package.minutes * package.times;
                    package_Details.megabytes += package.megabytes * package.times;
                    package_Details.messages += package.messages * package.times;
                    package_Details.price += package.price * package.times;

                    //build confirmation message
                    if (selectedPackages.Count > 1) confirmationMessage += $"Package {++i} name : ";
                    else confirmationMessage += $"Package name : ";
                    confirmationMessage += $"{package.packageName}{Environment.NewLine}This package will give you:-{Environment.NewLine}";
                    if (package.minutes > 0) confirmationMessage += $"     {package.minutes} Minutes{Environment.NewLine}";
                    if (package.megabytes > 0) confirmationMessage += $"     {package.megabytes} Megabytes{Environment.NewLine}";
                    if (package.messages > 0) confirmationMessage += $"     {package.messages} Messages{Environment.NewLine}";
                    if (selectedPackages.Count > 1) confirmationMessage += "Single ";
                    confirmationMessage += $"Package Price is : {package.price}{Environment.NewLine}";
                    if (package.times > 1)
                        confirmationMessage += $"This package will be subscribed {package.times} times with total price {package.price * package.times}" + Environment.NewLine;
                    confirmationMessage += Environment.NewLine;

                }
                if (i > 1)
                {
                    confirmationMessage += Environment.NewLine;
                    confirmationMessage += $"With total: {package_Details.minutes} Minutes, {package_Details.megabytes} Megabytes,  {package_Details.messages} Messages {Environment.NewLine}";
                    confirmationMessage += $"Total price: {package_Details.price}$.{Environment.NewLine}";
                }
                if (this.Packages != null)
                {
                    dc.State.SetValue(this.Packages.GetValue(dc.State).ToString(), selectedPackages);
                }
            }
            else
            {
                package_Details.packageName = PackageName.GetValue(dc.State).ToString();
                bool validPackage = get_package_details(ref package_Details, conn);
                if (!validPackage) throw new Exception("Someting went wrong");
                _exactPackage = true;
                if (this.Packages != null)
                {
                    dc.State.SetValue(this.Packages.GetValue(dc.State).ToString(), package_Details.packageName);
                }
                //build confirmation message
                confirmationMessage += $"Package name : {package_Details.packageName}{Environment.NewLine}This package will give you:-{Environment.NewLine}";
                if (package_Details.minutes > 0) confirmationMessage += $"     {package_Details.minutes} Minutes{Environment.NewLine}";
                if (package_Details.megabytes > 0) confirmationMessage += $"     {package_Details.megabytes} Megabytes{Environment.NewLine}";
                if (package_Details.messages > 0) confirmationMessage += $"     {package_Details.messages} Messages{Environment.NewLine}";
                confirmationMessage += $"Package Price is : {package_Details.price}{Environment.NewLine}";

            }
            if (this.Minutes != null)
            {
                dc.State.SetValue(this.Minutes.GetValue(dc.State).ToString(), package_Details.minutes);
            }
            if (this.Messages != null)
            {
                dc.State.SetValue(this.Messages.GetValue(dc.State).ToString(), package_Details.messages);
            }
            if (this.Megabytes != null)
            {
                dc.State.SetValue(this.Megabytes.GetValue(dc.State).ToString(), package_Details.megabytes);
            }
            if (this.Price != null)
            {
                dc.State.SetValue(this.Price.GetValue(dc.State).ToString(), package_Details.price);
            }
            if (this.PackagesDeatails != null)
            {
                dc.State.SetValue(this.PackagesDeatails.GetValue(dc.State).ToString(), confirmationMessage);
            }
            if (this.ExactPackage != null)
            {
                dc.State.SetValue(this.ExactPackage.GetValue(dc.State).ToString(), _exactPackage);
            }
            if (confirmationMessage.Length == 0) throw new Exception("There is no chosen packages");
            result = true;
        }
        catch (Exception ex)
        {
            if (this.Error != null)
            {
                dc.State.SetValue(this.Error.GetValue(dc.State).ToString(), ex.Message);
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