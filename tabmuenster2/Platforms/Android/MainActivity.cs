using Android.App;
using Android.Content.PM;
using Android.OS;
using Microsoft.Maui;
// using Android.Util;
// using Android.Gms.Security; // Google Play Services Security

namespace tabmuenster
{
    [Activity(
        Label = "Taschengeldbörse Münster",
        Theme = "@style/Maui.SplashTheme",
        MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize
                             | ConfigChanges.Orientation
                             | ConfigChanges.UiMode
                             | ConfigChanges.ScreenLayout
                             | ConfigChanges.SmallestScreenSize
                             | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        // Keine OnCreate-Initialisierung mehr nötig.
        // MAUI ruft automatisch MauiProgram.CreateMauiApp() auf.
   /*     const string TAG = "MainActivity";
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            try
            {
                // Versucht den Security Provider zu aktualisieren (benötigt Google Play Services auf dem Gerät)
                ProviderInstaller.InstallIfNeeded(this);
                Log.Info(TAG, "Security provider installed/updated.");
            }
            catch (Java.Lang.Exception ex)
            {
                Log.Warn(TAG, "ProviderInstaller failed: " + ex);
                // Fallback: Provider konnte nicht aktualisiert werden; weiterhin Server prüfen
            }
        }*/ 
    }
}


