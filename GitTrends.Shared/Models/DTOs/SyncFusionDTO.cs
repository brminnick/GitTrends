namespace GitTrends.Shared
{
    public class SyncFusionDTO
    {
        public SyncFusionDTO(string licenseKey, string licenseVersion) =>
            (LicenseKey, LicenseVersion) = (licenseKey, licenseVersion);

        public string LicenseKey { get; }
        public string LicenseVersion { get; }
    }
}
