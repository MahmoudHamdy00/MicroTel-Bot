using System;
using System.Collections.Generic;
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


    [JsonProperty("packages")]
    public ValueExpression Packages { get; set; }


    [JsonProperty("packagesDetails")]
    public ValueExpression PackagesDeatails { get; set; }

    [JsonProperty("message")]
    public ValueExpression Message { get; set; }

    [JsonProperty("resultProperty")]
    public ValueExpression ResultProperty { get; set; }


    [JsonProperty("error")]
    public ValueExpression Error { get; set; }

    public override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
    {
     

        var data = PackageName.GetValue(dc.State);

        bool result = false;//initialize with failed and then change it if it success
        try
        {
            microteldbContext microteldb = new microteldbContext();

            Newtonsoft.Json.Linq.JArray packageNames;
            ExtraPackageDetail package_Details = new ExtraPackageDetail();
            string confirmationMessage = "";
            if (data.GetType().ToString() == "Newtonsoft.Json.Linq.JArray")
            {
                packageNames = (Newtonsoft.Json.Linq.JArray)data;
                foreach (var cur in packageNames)
                {
                    Console.WriteLine(cur["amount"][0].ToString() + " : " + cur["unit"][0][0].ToString());

                    if (cur["unit"][0][0].ToString() == "minutes") package_Details.Minutes += Convert.ToInt32(cur["amount"][0]);
                    else if (cur["unit"][0][0].ToString() == "gigabyte") package_Details.Megabytes += Convert.ToInt32(cur["amount"][0]) * 1000;
                    else if (cur["unit"][0][0].ToString() == "megabyte") package_Details.Megabytes += Convert.ToInt32(cur["amount"][0]);
                    else if (cur["unit"][0][0].ToString() == "messages") package_Details.Messages += Convert.ToInt32(cur["amount"][0]);
                }
                //     List<package_details> selectedPackages = mainGetBestPackages(package_Details.minutes, package_Details.messages, package_Details.megabytes, conn);
                bool found = false;
                Dictionary<string, List<int>> map = new Dictionary<string, List<int>>();
                List<ExtraPackageDetail> selectedPackages = getPackages(Convert.ToInt32(package_Details.Minutes), Convert.ToInt32(package_Details.Messages), Convert.ToInt32(package_Details.Megabytes), ref found, ref map, microteldb);
                if (!found)
                {
                    string packages = "There are only these packages" + Environment.NewLine;
                    if (package_Details.Minutes > 0)
                    {
                        packages += "packages that gives you ";
                        foreach (int cur in map["Minutes"])
                        {
                            packages += cur.ToString() + ",";
                        }
                        packages += "Minutes" + Environment.NewLine;

                    }
                    if (package_Details.Megabytes > 0)
                    {
                        packages += "packages that gives you ";
                        foreach (int cur in map["Megabytes"])
                        {
                            packages += cur.ToString() + ",";
                        }
                        packages += "Megabytes" + Environment.NewLine;

                    }
                    if (package_Details.Messages > 0)
                    {
                        packages += "packages that gives you ";
                        foreach (int cur in map["Text Messages"])
                        {
                            packages += cur.ToString() + ",";
                        }
                        packages += "Messages" + Environment.NewLine;

                    }
                    if (this.Packages != null)
                    {
                        dc.State.SetValue(this.Packages.GetValue(dc.State).ToString(), packages);
                    }
                    throw new Exception("Sorry there is no packeges as you required");
                }
                package_Details.Minutes = package_Details.Megabytes = package_Details.Messages = 0;
                int i = 0;
                foreach (var package in selectedPackages)
                {
                    package_Details.Minutes += package.Minutes;
                    package_Details.Megabytes += package.Megabytes;
                    package_Details.Messages += package.Messages;
                    package_Details.Price += package.Price;

                    //build confirmation message
                    if (selectedPackages.Count > 1) confirmationMessage += $"Package {++i} name : ";
                    else confirmationMessage += $"Package name : ";
                    confirmationMessage += $"{package.Name}{Environment.NewLine}This package will give you:-{Environment.NewLine}";
                    if (package.Minutes > 0) confirmationMessage += $"     {package.Minutes} Minutes{Environment.NewLine}";
                    if (package.Megabytes > 0) confirmationMessage += $"     {package.Megabytes} Megabytes{Environment.NewLine}";
                    if (package.Messages > 0) confirmationMessage += $"     {package.Messages} Messages{Environment.NewLine}";
                    if (selectedPackages.Count > 1) confirmationMessage += "Single ";
                    confirmationMessage += $"Package Price is : {package.Price}{Environment.NewLine}";
                    confirmationMessage += Environment.NewLine;

                }
                if (i > 1)
                {
                    confirmationMessage += Environment.NewLine;
                    confirmationMessage += $"With total: {package_Details.Minutes} Minutes, {package_Details.Megabytes} Megabytes,  {package_Details.Messages} Messages {Environment.NewLine}";
                    confirmationMessage += $"Total price: {package_Details.Price}$.{Environment.NewLine}";
                }
                if (this.Packages != null)
                {
                    dc.State.SetValue(this.Packages.GetValue(dc.State).ToString(), selectedPackages);
                }
            }
            else
            {
                package_Details.Name = PackageName.GetValue(dc.State).ToString();
                bool validPackage = get_package_details(ref package_Details, microteldb);
                if (!validPackage) throw new Exception("Someting went wrong");
                if (this.Packages != null)
                {
                    dc.State.SetValue(this.Packages.GetValue(dc.State).ToString(), package_Details.Name);
                }
                //build confirmation message
                confirmationMessage += $"Package name : {package_Details.Name}{Environment.NewLine}This package will give you:-{Environment.NewLine}";
                if (package_Details.Minutes > 0) confirmationMessage += $"     {package_Details.Minutes} Minutes{Environment.NewLine}";
                if (package_Details.Megabytes > 0) confirmationMessage += $"     {package_Details.Megabytes} Megabytes{Environment.NewLine}";
                if (package_Details.Messages > 0) confirmationMessage += $"     {package_Details.Messages} Messages{Environment.NewLine}";
                confirmationMessage += $"Package Price is : {package_Details.Price}{Environment.NewLine}";

            }

            if (this.PackagesDeatails != null)
            {
                dc.State.SetValue(this.PackagesDeatails.GetValue(dc.State).ToString(), package_Details);
            }
            if (this.Message != null)
            {
                dc.State.SetValue(this.Message.GetValue(dc.State).ToString(), confirmationMessage);
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
        }

        if (this.ResultProperty != null)
        {
            dc.State.SetValue(this.ResultProperty.GetValue(dc.State).ToString(), result);
        }
        return dc.EndDialogAsync(result: result, cancellationToken: cancellationToken);
    }
}