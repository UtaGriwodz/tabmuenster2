using Microsoft.Extensions.Logging;

namespace tabmuenster
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<tabmuenster.App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("Cookie-Regular.ttf", "Roboto Regular");
                    fonts.AddFont("Raleway-Regular.ttf", "Raleway");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
