namespace GitTrends.Shared
{
    public class SyncFusionDTO
    {
        public SyncFusionDTO(string license) => License = license;

        public string License { get; }
    }
}
