using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace tabmuenster;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // Starte mit deiner Hauptseite
       // MainPage = new MainPage();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // Lege hier die Startseite fest
        return new Window(new MainPage());
    }

    // In MAUI sind OnStart/OnSleep/OnResume entfallen.
    // Falls du weiterhin App-Lifecycle-Ereignisse brauchst,
    // kannst du die neuen Methoden verwenden:
    protected override void OnStart()
    {
        // Optional: Start-Logik
    }

    protected override void OnSleep()
    {
        // Optional: App geht in den Hintergrund
    }

    protected override void OnResume()
    {
        // Optional: App kehrt aus dem Hintergrund zurück
    }
}
