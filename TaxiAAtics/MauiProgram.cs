using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Maps;

namespace TaxiAAtics;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
            .UseSkiaSharp(true)
			.UseMauiCommunityToolkitMaps("AnrQnVt0Y_p65MGVdkLtXdAwMIy9P7Abk7OzBywVF69fkiGexRGSLxYsONmGXZl2")
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("GP.otf", "BB");
                fonts.AddFont("GR.otf", "R");
                fonts.AddFont("BBS.ttf", "S");
                fonts.AddFont("DMB.ttf", "D");
				fonts.AddFont("MaterialIconsSharp.otf", "M");
            });

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}

