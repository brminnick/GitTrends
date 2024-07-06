using GitTrends.Shared;

namespace GitTrends.UnitTests;

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

		Assert.Multiple(() =>
		{
			//Assert
			Assert.That(onboardingChartStreamingManifest_BeforeInitialization, Is.Null);
			Assert.That(onboardingChartStreamingManifest_EventHandlerResult, Is.Not.Null);
			Assert.That(onboardingChartStreamingManifest_AfterInitialization, Is.Not.Null);

			Assert.That(Uri.IsWellFormedUriString(onboardingChartStreamingManifest_EventHandlerResult?.HlsUrl, UriKind.Absolute));
			Assert.That(Uri.IsWellFormedUriString(onboardingChartStreamingManifest_EventHandlerResult?.DashUrl, UriKind.Absolute));
			Assert.That(Uri.IsWellFormedUriString(onboardingChartStreamingManifest_EventHandlerResult?.ManifestUrl, UriKind.Absolute));

			Assert.That(Uri.IsWellFormedUriString(onboardingChartStreamingManifest_AfterInitialization?.HlsUrl, UriKind.Absolute));
			Assert.That(Uri.IsWellFormedUriString(onboardingChartStreamingManifest_AfterInitialization?.DashUrl, UriKind.Absolute));
			Assert.That(Uri.IsWellFormedUriString(onboardingChartStreamingManifest_AfterInitialization?.ManifestUrl, UriKind.Absolute));

			Assert.DoesNotThrow(() => getOnboardingChartUrlResponse_HLSUrl.EnsureSuccessStatusCode());
			Assert.DoesNotThrow(() => getOnboardingChartUrlResponse_DashUrl.EnsureSuccessStatusCode());
			Assert.DoesNotThrow(() => getOnboardingChartUrlResponse_ManifestUrl.EnsureSuccessStatusCode());

			if (deviceInfo.Platform == DevicePlatform.Android)
				Assert.That(onboardingChartUrl_Final, Is.EqualTo(onboardingChartStreamingManifest_AfterInitialization?.DashUrl));
			else if (deviceInfo.Platform == DevicePlatform.iOS)
				Assert.That(onboardingChartUrl_Final, Is.EqualTo(onboardingChartStreamingManifest_AfterInitialization?.HlsUrl));
			else
				throw new InvalidOperationException($"{deviceInfo.Platform} Not Under Test");

			Assert.That(enableOrganizationsStreamingManifest_BeforeInitialization, Is.Null);
			Assert.That(enableOrganizationsStreamingManifest_EventHandlerResult, Is.Not.Null);
			Assert.That(enableOrganizationsStreamingManifest_AfterInitialization, Is.Not.Null);

			Assert.That(Uri.IsWellFormedUriString(enableOrganizationsStreamingManifest_EventHandlerResult?.HlsUrl, UriKind.Absolute), Is.True);
			Assert.That(Uri.IsWellFormedUriString(enableOrganizationsStreamingManifest_EventHandlerResult?.DashUrl, UriKind.Absolute), Is.True);
			Assert.That(Uri.IsWellFormedUriString(enableOrganizationsStreamingManifest_EventHandlerResult?.ManifestUrl, UriKind.Absolute), Is.True);

			Assert.That(Uri.IsWellFormedUriString(enableOrganizationsStreamingManifest_AfterInitialization?.HlsUrl, UriKind.Absolute));
			Assert.That(Uri.IsWellFormedUriString(enableOrganizationsStreamingManifest_AfterInitialization?.DashUrl, UriKind.Absolute));
			Assert.That(Uri.IsWellFormedUriString(enableOrganizationsStreamingManifest_AfterInitialization?.ManifestUrl, UriKind.Absolute));

			Assert.DoesNotThrow(() => getEnableOrganizationsUrlResponse_HLSUrl.EnsureSuccessStatusCode());
			Assert.DoesNotThrow(() => getEnableOrganizationsUrlResponse_DashUrl.EnsureSuccessStatusCode());
			Assert.DoesNotThrow(() => getEnableOrganizationsUrlResponse_ManifestUrl.EnsureSuccessStatusCode());

			if (deviceInfo.Platform == DevicePlatform.Android)
				Assert.That(enableOrganizationsUrl_Final, Is.EqualTo(enableOrganizationsStreamingManifest_AfterInitialization?.DashUrl));
			else if (deviceInfo.Platform == DevicePlatform.iOS)
				Assert.That(enableOrganizationsUrl_Final, Is.EqualTo(enableOrganizationsStreamingManifest_AfterInitialization?.HlsUrl));
			else
				throw new InvalidOperationException($"{deviceInfo.Platform} Not Under Test");

			Assert.That(enableOrganizationsStreamingManifest_AfterInitialization?.ManifestUrl, Is.EqualTo(enableOrganizationsStreamingManifest_EventHandlerResult?.ManifestUrl));
			Assert.That(onboardingChartStreamingManifest_AfterInitialization?.ManifestUrl, Is.EqualTo(onboardingChartStreamingManifest_EventHandlerResult?.ManifestUrl));

			Assert.That(enableOrganizationsStreamingManifest_AfterInitialization?.HlsUrl, Is.EqualTo(enableOrganizationsStreamingManifest_EventHandlerResult?.HlsUrl));
			Assert.That(onboardingChartStreamingManifest_AfterInitialization?.HlsUrl, Is.EqualTo(onboardingChartStreamingManifest_EventHandlerResult?.HlsUrl));

			Assert.That(enableOrganizationsStreamingManifest_AfterInitialization?.DashUrl, Is.EqualTo(enableOrganizationsStreamingManifest_EventHandlerResult?.DashUrl));
			Assert.That(onboardingChartStreamingManifest_AfterInitialization?.DashUrl, Is.EqualTo(onboardingChartStreamingManifest_EventHandlerResult?.DashUrl));

			Assert.That(onboardingChartStreamingManifest_EventHandlerResult?.ManifestUrl, Is.Not.EqualTo(enableOrganizationsStreamingManifest_EventHandlerResult?.ManifestUrl));
			Assert.That(onboardingChartStreamingManifest_AfterInitialization?.ManifestUrl, Is.Not.EqualTo(enableOrganizationsStreamingManifest_AfterInitialization?.ManifestUrl));

			Assert.That(onboardingChartStreamingManifest_EventHandlerResult?.HlsUrl, Is.Not.EqualTo(enableOrganizationsStreamingManifest_EventHandlerResult?.HlsUrl));
			Assert.That(onboardingChartStreamingManifest_AfterInitialization?.HlsUrl, Is.Not.EqualTo(enableOrganizationsStreamingManifest_AfterInitialization?.HlsUrl));

			Assert.That(onboardingChartStreamingManifest_EventHandlerResult?.ManifestUrl, Is.Not.EqualTo(enableOrganizationsStreamingManifest_EventHandlerResult?.ManifestUrl));
			Assert.That(onboardingChartStreamingManifest_AfterInitialization?.ManifestUrl, Is.Not.EqualTo(enableOrganizationsStreamingManifest_AfterInitialization?.ManifestUrl));
		});

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