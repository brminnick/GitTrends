namespace GitTrends.Mobile.Shared
{
    public static class SyncFusionService
    {
        public static void Initialize()
        {
#if __IOS__
            Syncfusion.SfChart.XForms.iOS.Renderers.SfChartRenderer.Init();
#endif
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("");

        }
    }
}
