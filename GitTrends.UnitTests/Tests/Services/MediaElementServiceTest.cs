using System;
using System.Threading;
using System.Threading.Tasks;
using GitTrends.Shared;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace GitTrends.UnitTests
{
    class MediaElementServiceTest : BaseTest
    {
        [Test]
        public async Task InitializeOnboardingChartTest()
        {
            //Arrange
            StreamingManifest? onboardingChartStreamingManifest_BeforeInitialization, onboardingChartStreamingManifest_EventHandlerResult, onboardingChartStreamingManifest_AfterInitialization,
                                enableOrganizationsStreamingManifest_BeforeInitialization, enableOrganizationsStreamingManifest_EventHandlerResult, enableOrganizationsStreamingManifest_AfterInitialization;

            var onboardingChartManifestChangedTCS = new TaskCompletionSource<StreamingManifest?>();
            var enableOrganizationsManifestChangedTCS = new TaskCompletionSource<StreamingManifest?>();

            var mediaElementService = ServiceCollection.ServiceProvider.GetRequiredService<MediaElementService>();

            MediaElementService.OnboardingChartManifestChanged += HandleOnboardingChartManifestChanged;
            MediaElementService.EnableOrganizationsManifestChanged += HandleEnableOrganizationsManifestChanged;

            //Act
            onboardingChartStreamingManifest_BeforeInitialization = mediaElementService.OnboardingChartManifest;
            enableOrganizationsStreamingManifest_BeforeInitialization = mediaElementService.EnableOrganizationsManifest;

            await mediaElementService.InitializeManifests(CancellationToken.None).ConfigureAwait(false);

            onboardingChartStreamingManifest_EventHandlerResult = await onboardingChartManifestChangedTCS.Task.ConfigureAwait(false);
            enableOrganizationsStreamingManifest_EventHandlerResult = await enableOrganizationsManifestChangedTCS.Task.ConfigureAwait(false);

            onboardingChartStreamingManifest_AfterInitialization = mediaElementService.OnboardingChartManifest;
            enableOrganizationsStreamingManifest_AfterInitialization = mediaElementService.EnableOrganizationsManifest;

            //Assert
            Assert.IsNull(onboardingChartStreamingManifest_BeforeInitialization);
            Assert.IsNotNull(onboardingChartStreamingManifest_EventHandlerResult);
            Assert.IsNotNull(onboardingChartStreamingManifest_AfterInitialization);

            Assert.IsTrue(Uri.IsWellFormedUriString(onboardingChartStreamingManifest_EventHandlerResult?.HlsUrl, UriKind.Absolute));
            Assert.IsTrue(Uri.IsWellFormedUriString(onboardingChartStreamingManifest_EventHandlerResult?.ManifestUrl, UriKind.Absolute));

            Assert.IsTrue(Uri.IsWellFormedUriString(onboardingChartStreamingManifest_AfterInitialization?.HlsUrl, UriKind.Absolute));
            Assert.IsTrue(Uri.IsWellFormedUriString(onboardingChartStreamingManifest_AfterInitialization?.ManifestUrl, UriKind.Absolute));

            Assert.IsNull(enableOrganizationsStreamingManifest_BeforeInitialization);
            Assert.IsNotNull(enableOrganizationsStreamingManifest_EventHandlerResult);
            Assert.IsNotNull(enableOrganizationsStreamingManifest_AfterInitialization);

            Assert.IsTrue(Uri.IsWellFormedUriString(enableOrganizationsStreamingManifest_EventHandlerResult?.HlsUrl, UriKind.Absolute));
            Assert.IsTrue(Uri.IsWellFormedUriString(enableOrganizationsStreamingManifest_EventHandlerResult?.ManifestUrl, UriKind.Absolute));
                                                    
            Assert.IsTrue(Uri.IsWellFormedUriString(enableOrganizationsStreamingManifest_AfterInitialization?.HlsUrl, UriKind.Absolute));
            Assert.IsTrue(Uri.IsWellFormedUriString(enableOrganizationsStreamingManifest_AfterInitialization?.ManifestUrl, UriKind.Absolute));

            Assert.AreEqual(enableOrganizationsStreamingManifest_EventHandlerResult?.HlsUrl, enableOrganizationsStreamingManifest_AfterInitialization?.HlsUrl);
            Assert.AreEqual(onboardingChartStreamingManifest_EventHandlerResult?.HlsUrl, onboardingChartStreamingManifest_AfterInitialization?.HlsUrl);

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
