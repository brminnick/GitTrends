namespace GitTrends.Shared
{
    public class SyncFusionDTO
    {
        public SyncFusionDTO(string licenseKey, long licenseVersion) =>
            (LicenseKey, LicenseVersion) = (licenseKey, licenseVersion);

        public string LicenseKey { get; }
        public long LicenseVersion { get; }
    }
}
