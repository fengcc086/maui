﻿using Microsoft.Maui;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Hosting;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace Maui.Controls.Sample.Profiling
{
	public static class MauiProgram
	{
		public static MauiAppBuilder CreateAppBuilder()
		{
			var appBuilder = MauiAppBuilder.CreateBuilder();

			appBuilder
				.UseMauiApp<App>()
				.ConfigureFonts(fonts =>
				{
					fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				});

			return appBuilder;
		}
	}
}
