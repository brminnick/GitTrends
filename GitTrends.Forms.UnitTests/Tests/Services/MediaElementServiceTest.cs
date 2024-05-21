using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GitTrends.Shared;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Xamarin.Essentials.Interfaces;

namespace GitTrends.UnitTests
{
	class MediaElementServiceTest : BaseTest
	{
		[Test]
		public async Task InitializeOnboardingChartTest()
		{
			//Arrange
			HttpResponseMessage getOnboardingChartUrlResponse_HLSUrl, getOnboardingChartUrlResponse_DashUrl, getOnboardingChartUrlResponse_ManifestUrl,
								getEnableOrganizationsUrlResponse_HLSUrl, getEnableOrganizationsUrlResponse_DashUrl, getEnableOrganizationsUrlResponse_ManifestUrl;

			StreamingManifest? onboardingChartStreamingManifest_BeforeInitialization, onboardingChartStreamingManifest_EventHandlerResult, onboardingChartStreamingManifest_AfterInitialization,
								enableOrganizationsStreamingManifest_BeforeInitialization, enableOrganizationsStreamingManifest_EventHandlerResult, enableOrganizationsStreamingManifest_AfterInitialization;

			string enableOrganizationsUrl_Final, onboardingChartUrl_Final;

			var onboardingChartManifestChangedTCS = new TaskCompletionSource<StreamingManifest?>();
			var enableOrganizationsManifestChangedTCS = new TaskCompletionSource<StreamingManifest?>();

			var deviceInfo = ServiceCollection.ServiceProvider.GetRequiredService<IDeviceInfo>();
			var mediaElementService = ServiceCollection.ServiceProvider.GetRequiredService<MediaElementService>();

			var httpClient = new HttpClient();

			MediaElementService.OnboardingChartManifestChanged += HandleOnboardingChartManifestChanged;
			MediaElementService.EnableOrganizationsManifestChanged += HandleEnableOrganizationsManifestChanged;

			//Assert
			Assert.Throws<ArgumentNullException>(() => _ = mediaElementService.OnboardingChartUrl);
			Assert.Throws<ArgumentNullException>(() => _ = mediaElementService.EnableOrganizationsUrl);

			//Act
			onboardingChartStreamingManifest_BeforeInitialization = mediaElementService.OnboardingChartManifest;
			enableOrganizationsStreamingManifest_BeforeInitialization = mediaElementService.EnableOrganizationsManifest;

			await mediaElementService.InitializeManifests(CancellationToken.None).ConfigureAwait(false);

			onboardingChartStreamingManifest_EventHandlerResult = await onboardingChartManifestChangedTCS.Task.ConfigureAwait(false);
			enableOrganizationsStreamingManifest_EventHandlerResult = await enableOrganizationsManifestChangedTCS.Task.ConfigureAwait(false);

			onboardingChartUrl_Final = mediaElementService.OnboardingChartUrl;
			enableOrganizationsUrl_Final = mediaElementService.EnableOrganizationsUrl;
			onboardingChartStreamingManifest_AfterInitialization = mediaElementService.OnboardingChartManifest;
			enableOrganizationsStreamingManifest_AfterInitialization = mediaElementService.EnableOrganizationsManifest;

			getOnboardingChartUrlResponse_HLSUrl = await httpClient.GetAsync(onboardingChartStreamingManifest_AfterInitialization?.HlsUrl);
			getOnboardingChartUrlResponse_DashUrl = await httpClient.GetAsync(onboardingChartStreamingManifest_AfterInitialization?.DashUrl);
			getOnboardingChartUrlResponse_ManifestUrl = await httpClient.GetAsync(onboardingChartStreamingManifest_AfterInitialization?.ManifestUrl);

			getEnableOrganizationsUrlResponse_HLSUrl = await httpClient.GetAsync(enableOrganizationsStreamingManifest_AfterInitialization?.HlsUrl);
			getEnableOrganizationsUrlResponse_DashUrl = await httpClient.GetAsync(enableOrganizationsStreamingManifest_AfterInitialization?.DashUrl);
			getEnableOrganizationsUrlResponse_ManifestUrl = await httpClient.GetAsync(enableOrganizationsStreamingManifest_AfterInitialization?.ManifestUrl);

			//Assert
			Assert.IsNull(onboardingChartStreamingManifest_BeforeInitialization);
			Assert.IsNotNull(onboardingChartStreamingManifest_EventHandlerResult);
			Assert.IsNotNull(onboardingChartStreamingManifest_AfterInitialization);

			Assert.IsTrue(Uri.IsWellFormedUriString(onboardingChartStreamingManifest_EventHandlerResult?.HlsUrl, UriKind.Absolute));
			Assert.IsTrue(Uri.IsWellFormedUriString(onboardingChartStreamingManifest_EventHandlerResult?.DashUrl, UriKind.Absolute));
			Assert.IsTrue(Uri.IsWellFormedUriString(onboardingChartStreamingManifest_EventHandlerResult?.ManifestUrl, UriKind.Absolute));

			Assert.IsTrue(Uri.IsWellFormedUriString(onboardingChartStreamingManifest_AfterInitialization?.HlsUrl, UriKind.Absolute));
			Assert.IsTrue(Uri.IsWellFormedUriString(onboardingChartStreamingManifest_AfterInitialization?.DashUrl, UriKind.Absolute));
			Assert.IsTrue(Uri.IsWellFormedUriString(onboardingChartStreamingManifest_AfterInitialization?.ManifestUrl, UriKind.Absolute));

			Assert.DoesNotThrow(() => getOnboardingChartUrlResponse_HLSUrl.EnsureSuccessStatusCode());
			Assert.DoesNotThrow(() => getOnboardingChartUrlResponse_DashUrl.EnsureSuccessStatusCode());
			Assert.DoesNotThrow(() => getOnboardingChartUrlResponse_ManifestUrl.EnsureSuccessStatusCode());

			if (deviceInfo.Platform == Xamarin.Essentials.DevicePlatform.Android)
				Assert.AreEqual(onboardingChartStreamingManifest_AfterInitialization?.DashUrl, onboardingChartUrl_Final);
			else if (deviceInfo.Platform == Xamarin.Essentials.DevicePlatform.iOS)
				Assert.AreEqual(onboardingChartStreamingManifest_AfterInitialization?.HlsUrl, onboardingChartUrl_Final);
			else
				throw new InvalidOperationException($"{deviceInfo.Platform} Not Under Test");

			Assert.IsNull(enableOrganizationsStreamingManifest_BeforeInitialization);
			Assert.IsNotNull(enableOrganizationsStreamingManifest_EventHandlerResult);
			Assert.IsNotNull(enableOrganizationsStreamingManifest_AfterInitialization);

			Assert.IsTrue(Uri.IsWellFormedUriString(enableOrganizationsStreamingManifest_EventHandlerResult?.HlsUrl, UriKind.Absolute));
			Assert.IsTrue(Uri.IsWellFormedUriString(enableOrganizationsStreamingManifest_EventHandlerResult?.DashUrl, UriKind.Absolute));
			Assert.IsTrue(Uri.IsWellFormedUriString(enableOrganizationsStreamingManifest_EventHandlerResult?.ManifestUrl, UriKind.Absolute));

			Assert.IsTrue(Uri.IsWellFormedUriString(enableOrganizationsStreamingManifest_AfterInitialization?.HlsUrl, UriKind.Absolute));
			Assert.IsTrue(Uri.IsWellFormedUriString(enableOrganizationsStreamingManifest_AfterInitialization?.DashUrl, UriKind.Absolute));
			Assert.IsTrue(Uri.IsWellFormedUriString(enableOrganizationsStreamingManifest_AfterInitialization?.ManifestUrl, UriKind.Absolute));

			Assert.DoesNotThrow(() => getEnableOrganizationsUrlResponse_HLSUrl.EnsureSuccessStatusCode());
			Assert.DoesNotThrow(() => getEnableOrganizationsUrlResponse_DashUrl.EnsureSuccessStatusCode());
			Assert.DoesNotThrow(() => getEnableOrganizationsUrlResponse_ManifestUrl.EnsureSuccessStatusCode());

			if (deviceInfo.Platform == Xamarin.Essentials.DevicePlatform.Android)
				Assert.AreEqual(enableOrganizationsStreamingManifest_AfterInitialization?.DashUrl, enableOrganizationsUrl_Final);
			else if (deviceInfo.Platform == Xamarin.Essentials.DevicePlatform.iOS)
				Assert.AreEqual(enableOrganizationsStreamingManifest_AfterInitialization?.HlsUrl, enableOrganizationsUrl_Final);
			else
				throw new InvalidOperationException($"{deviceInfo.Platform} Not Under Test");

			Assert.AreEqual(enableOrganizationsStreamingManifest_EventHandlerResult?.ManifestUrl, enableOrganizationsStreamingManifest_AfterInitialization?.ManifestUrl);
			Assert.AreEqual(onboardingChartStreamingManifest_EventHandlerResult?.ManifestUrl, onboardingChartStreamingManifest_AfterInitialization?.ManifestUrl);

			Assert.AreEqual(enableOrganizationsStreamingManifest_EventHandlerResult?.HlsUrl, enableOrganizationsStreamingManifest_AfterInitialization?.HlsUrl);
			Assert.AreEqual(onboardingChartStreamingManifest_EventHandlerResult?.HlsUrl, onboardingChartStreamingManifest_AfterInitialization?.HlsUrl);

			Assert.AreEqual(enableOrganizationsStreamingManifest_EventHandlerResult?.DashUrl, enableOrganizationsStreamingManifest_AfterInitialization?.DashUrl);
			Assert.AreEqual(onboardingChartStreamingManifest_EventHandlerResult?.DashUrl, onboardingChartStreamingManifest_AfterInitialization?.DashUrl);

			Assert.AreNotEqual(enableOrganizationsStreamingManifest_EventHandlerResult?.ManifestUrl, onboardingChartStreamingManifest_EventHandlerResult?.ManifestUrl);
			Assert.AreNotEqual(enableOrganizationsStreamingManifest_AfterInitialization?.ManifestUrl, onboardingChartStreamingManifest_AfterInitialization?.ManifestUrl);

			Assert.AreNotEqual(enableOrganizationsStreamingManifest_EventHandlerResult?.HlsUrl, onboardingChartStreamingManifest_EventHandlerResult?.HlsUrl);
			Assert.AreNotEqual(enableOrganizationsStreamingManifest_AfterInitialization?.HlsUrl, onboardingChartStreamingManifest_AfterInitialization?.HlsUrl);

			Assert.AreNotEqual(enableOrganizationsStreamingManifest_EventHandlerResult?.ManifestUrl, onboardingChartStreamingManifest_EventHandlerResult?.ManifestUrl);
			Assert.AreNotEqual(enableOrganizationsStreamingManifest_AfterInitialization?.ManifestUrl, onboardingChartStreamingManifest_AfterInitialization?.ManifestUrl);

			void HandleOnboardingChartManifestChanged(object? sender, StreamingManifest? e)
			{
				MediaElementService.OnboardingChartManifestChanged -= HandleOnboardingChartManifestChanged;
				onboardingChartManifestChangedTCS.SetResult(e);
			}

			void HandleEnableOrganizationsManifestChanged(object? sender, StreamingManifest? e)
			{
				MediaElementService.EnableOrganizationsManifestChanged -= HandleEnableOrganizationsManifestChanged;
				enableOrganizationsManifestChangedTCS.SetResult(e);
			}
		}

	}
}