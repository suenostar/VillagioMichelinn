
using CommunityToolkit.Maui;            // << inicializa o MAUI Community Toolkit
using Microsoft.Extensions.Logging;

namespace Teste
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("bootstrap-icons.ttf", "BootstrapIcons");
                    fonts.AddFont("fonnts.com-Canvas_Inline_Reg.otf", "CanvasInline");
                    fonts.AddFont("SourceSans3.ttf", "SourceSans");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
