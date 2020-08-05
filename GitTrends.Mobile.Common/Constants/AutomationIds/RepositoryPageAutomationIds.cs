namespace GitTrends.Mobile.Common
{
    public static class RepositoryPageAutomationIds
    {
        public const string SearchBar = nameof(RepositoryPageAutomationIds) + nameof(SearchBar);
        public const string SettingsButton = nameof(RepositoryPageAutomationIds) + nameof(SettingsButton);
        public const string SortButton = nameof(RepositoryPageAutomationIds) + nameof(SortButton);
        public const string CollectionView = nameof(RepositoryPageAutomationIds) + nameof(CollectionView);
        public const string RefreshView = nameof(RepositoryPageAutomationIds) + nameof(RefreshView);
        public const string EmptyDataView = nameof(RepositoryPageAutomationIds) + nameof(EmptyDataView);
        public const string LargeScreenTrendingImage = nameof(RepositoryPageAutomationIds) + nameof(LargeScreenTrendingImage);
        public const string SmallScreenTrendingImage = nameof(RepositoryPageAutomationIds) + nameof(SmallScreenTrendingImage);
        public const string InformationButton = nameof(RepositoryPageAutomationIds) + nameof(InformationButton);
        public const string InformationLabel = nameof(RepositoryPageAutomationIds) + nameof(InformationLabel);

        public static string GetFloatingActionTextButtonLabelAutomationId(in FloatingActionButtonType floatingActionButtonType)
        {
            const string label = "Label";

            return floatingActionButtonType switch
            {
                FloatingActionButtonType.Information => nameof(RepositoryPageAutomationIds) + nameof(FloatingActionButtonType.Information) + label,
                FloatingActionButtonType.Statistic1 => nameof(RepositoryPageAutomationIds) + nameof(FloatingActionButtonType.Statistic1) + label,
                FloatingActionButtonType.Statistic2 => nameof(RepositoryPageAutomationIds) + nameof(FloatingActionButtonType.Statistic2) + label,
                FloatingActionButtonType.Statistic3 => nameof(RepositoryPageAutomationIds) + nameof(FloatingActionButtonType.Statistic3) + label,
                _ => throw new System.NotImplementedException(),
            };
        }
    }
}
