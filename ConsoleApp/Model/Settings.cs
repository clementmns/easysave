using System.Globalization;

namespace EasySave.ConsoleApp.Model;

public class Settings
{
    public string Language { get; set; } = "en-US";
    
    public void ChangeLanguage(string cultureName)
    {
        var culture = CultureInfo.GetCultureInfo(cultureName);

        Language = cultureName;

        // Change culture for all threads
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    } 
}