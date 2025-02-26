﻿using System.Linq;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.TestUtils.DeviceTests.Runners;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	public class Startup : IStartup
	{
		public void Configure(IAppHostBuilder appBuilder)
		{
			appBuilder
				.ConfigureLifecycleEvents(life =>
				{
#if __ANDROID__
					life.AddAndroid(android =>
					{
						android.OnCreate((activity, bundle) =>
							Platform.Init(activity, bundle));
						android.OnRequestPermissionsResult((activity, requestCode, permissions, grantResults) =>
							Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults));
					});
#endif
				})
				.ConfigureTests(new TestOptions
				{
					Assemblies =
					{
						typeof(Startup).Assembly
					},
					SkipCategories = Traits
						.GetSkipTraits()
#if __ANDROID__
						.Append($"{Traits.FileProvider}={Traits.FeatureSupport.ToExclude(Platform.HasApiLevel(24))}")
#endif
						.ToList(),
				})
				.UseHeadlessRunner(new HeadlessRunnerOptions
				{
					RequiresUIContext = true,
				})
				.UseVisualRunner();
		}
	}
}