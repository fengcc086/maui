using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Hosting.Internal;
using Microsoft.Maui.LifecycleEvents;

namespace Microsoft.Maui
{
	public sealed class MauiAppBuilder
	{
		private MauiAppBuilder(bool useDefaults = true)
		{
			if (useDefaults)
			{
				// Register required services
				ConfigureMauiHandlers(configureDelegate: null);

				ConfigureFonts();
				ConfigureImageSources();
				//ConfigureAnimations();
				this.ConfigureCrossPlatformLifecycleEvents();
			}
		}

		public static MauiAppBuilder CreateBuilder(bool useDefaults = true) => new(useDefaults);

		/// <summary>
		/// A collection of services for the application to compose. This is useful for adding user provided or framework provided services.
		/// </summary>
		public IServiceCollection Services { get; } = new ServiceCollection();

		public MauiAppBuilder ConfigureMauiHandlers(Action<IMauiHandlersCollection>? configureDelegate)
		{
			Services.TryAddSingleton<IMauiHandlersServiceProvider, MauiHandlersServiceProvider>();
			if (configureDelegate != null)
			{
				Services.AddSingleton<HandlerRegistration>(new HandlerRegistration(configureDelegate));
			}
			return this;
		}

		public MauiAppBuilder ConfigureFonts()
		{
			ConfigureFonts(configureDelegate: null);
			return this;
		}

		public MauiAppBuilder ConfigureFonts(Action<IFontCollection>? configureDelegate)
		{
			Services.TryAddSingleton<IEmbeddedFontLoader>(svc => new EmbeddedFontLoader(svc.CreateLogger<EmbeddedFontLoader>()));
			Services.TryAddSingleton<IFontRegistrar>(svc => new FontRegistrar(svc.GetRequiredService<IEmbeddedFontLoader>(), svc.CreateLogger<FontRegistrar>()));
			Services.TryAddSingleton<IFontManager>(svc => new FontManager(svc.GetRequiredService<IFontRegistrar>(), svc.CreateLogger<FontManager>()));
			if (configureDelegate != null)
			{
				Services.AddSingleton<FontsRegistration>(new FontsRegistration(configureDelegate));
			}
			Services.AddSingleton<IMauiInitializeService, FontInitializer>();
			return this;
		}


		internal class FontsRegistration
		{
			private readonly Action<IFontCollection> _registerFonts;

			public FontsRegistration(Action<IFontCollection> registerFonts)
			{
				_registerFonts = registerFonts;
			}

			internal void AddFonts(IFontCollection fonts)
			{
				_registerFonts(fonts);
			}
		}

		internal class FontInitializer : IMauiInitializeService
		{
			private readonly IEnumerable<FontsRegistration> _fontsRegistrations;
			readonly IFontRegistrar _fontRegistrar;

			public FontInitializer(IEnumerable<FontsRegistration> fontsRegistrations, IFontRegistrar fontRegistrar)
			{
				_fontsRegistrations = fontsRegistrations;
				_fontRegistrar = fontRegistrar;
			}

			public void Initialize(HostBuilderContext _, IServiceProvider __)
			{
				if (_fontsRegistrations != null)
				{
					var fontsBuilder = new FontCollection();

					// Run all the user-defined registrations
					foreach (var font in _fontsRegistrations)
					{
						font.AddFonts(fontsBuilder);
					}

					// Register the fonts in the registrar
					foreach (var font in fontsBuilder)
					{
						if (font.Assembly == null)
							_fontRegistrar.Register(font.Filename, font.Alias);
						else
							_fontRegistrar.Register(font.Filename, font.Alias, font.Assembly);
					}
				}
			}
		}

		readonly List<Action<HostBuilderContext, IConfigurationBuilder>> _configureAppConfigActions = new List<Action<HostBuilderContext, IConfigurationBuilder>>();
		readonly List<Action<IConfigurationBuilder>> _configureHostConfigActions = new List<Action<IConfigurationBuilder>>();

		public MauiAppBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate)
		{
			_configureAppConfigActions.Add(configureDelegate ?? throw new ArgumentNullException(nameof(configureDelegate)));
			return this;
		}

		public MauiAppBuilder ConfigureAppConfiguration(Action<IConfigurationBuilder> configureDelegate)
		{
			ConfigureAppConfiguration((_, config) => configureDelegate(config));
			return this;
		}

		public MauiAppBuilder ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate)
		{
			_configureHostConfigActions.Add(configureDelegate ?? throw new ArgumentNullException(nameof(configureDelegate)));
			return this;
		}

		public MauiAppBuilder ConfigureImageSources()
		{
			ConfigureImageSources(services =>
			{
				services.AddService<IFileImageSource>(svcs => new FileImageSourceService(svcs.GetService<IImageSourceServiceConfiguration>(), svcs.CreateLogger<FileImageSourceService>()));
				services.AddService<IFontImageSource>(svcs => new FontImageSourceService(svcs.GetRequiredService<IFontManager>(), svcs.CreateLogger<FontImageSourceService>()));
				services.AddService<IStreamImageSource>(svcs => new StreamImageSourceService(svcs.CreateLogger<StreamImageSourceService>()));
				services.AddService<IUriImageSource>(svcs => new UriImageSourceService(svcs.CreateLogger<UriImageSourceService>()));
			});
			return this;
		}

		public MauiAppBuilder ConfigureImageSources(Action<IImageSourceServiceCollection>? configureDelegate)
		{
			if (configureDelegate != null)
			{
				Services.AddSingleton<ImageSourceRegistration>(new ImageSourceRegistration(configureDelegate));
			}

			Services.AddSingleton<IImageSourceServiceConfiguration, ImageSourceServiceConfiguration>();
			Services.AddSingleton<IImageSourceServiceProvider>(svcs => new ImageSourceServiceProvider(svcs.GetRequiredService<IImageSourceServiceCollection>(), svcs));
			Services.AddSingleton<IImageSourceServiceCollection, ImageSourceServiceBuilder>();

			return this;
		}

		class ImageSourceRegistration
		{
			private readonly Action<IImageSourceServiceCollection> _registerAction;

			public ImageSourceRegistration(Action<IImageSourceServiceCollection> registerAction)
			{
				_registerAction = registerAction;
			}

			internal void AddRegistration(IImageSourceServiceCollection builder)
			{
				_registerAction(builder);
			}
		}

		class ImageSourceServiceBuilder : MauiServiceCollection, IImageSourceServiceCollection
		{
			public ImageSourceServiceBuilder(IEnumerable<ImageSourceRegistration> registrationActions)
			{
				if (registrationActions != null)
				{
					foreach (var effectRegistration in registrationActions)
					{
						effectRegistration.AddRegistration(this);
					}
				}
			}
		}

		internal class HandlerRegistration
		{
			private readonly Action<IMauiHandlersCollection> _registerAction;

			public HandlerRegistration(Action<IMauiHandlersCollection> registerAction)
			{
				_registerAction = registerAction;
			}

			internal void AddRegistration(IMauiHandlersCollection builder)
			{
				_registerAction(builder);
			}
		}

		public IServiceProvider Build()
		{
			// AppConfig
			BuildHostConfiguration();
			BuildAppConfiguration();
			if (_appConfiguration != null)
				Services.AddSingleton(_appConfiguration);

			// ConfigureServices
			var properties = new Dictionary<object, object>();
			var builderContext = new HostBuilderContext(properties); // TODO: Should get this from somewhere...

			var serviceProvider = Services.BuildServiceProvider();

			var initServices = serviceProvider.GetService<IEnumerable<IMauiInitializeService>>();
			if (initServices != null)
			{
				foreach (var instance in initServices)
				{
					instance.Initialize(builderContext, serviceProvider);
				}
			}

			return serviceProvider;
		}

		IConfiguration? _hostConfiguration;
		IConfiguration? _appConfiguration;

		void BuildHostConfiguration()
		{
			var configBuilder = new ConfigurationBuilder();
			foreach (var buildAction in _configureHostConfigActions)
			{
				buildAction(configBuilder);
			}
			_hostConfiguration = configBuilder.Build();
		}

		void BuildAppConfiguration()
		{
			var properties = new Dictionary<object, object>();
			var builderContext = new HostBuilderContext(properties); // TODO: Should get this from somewhere...

			var configBuilder = new ConfigurationBuilder();
			configBuilder.AddConfiguration(_hostConfiguration);
			foreach (var buildAction in _configureAppConfigActions)
			{
				buildAction(builderContext, configBuilder);
			}
			_appConfiguration = configBuilder.Build();

			builderContext.Configuration = _appConfiguration;
		}
	}
}
