using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AsyncAwaitBestPractices.MVVM;
using GitTrends.Mobile.Common;
using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using static GitTrends.MarkupExtensions;
using static Xamarin.Forms.Markup.GridRowsColumns;

namespace GitTrends
{
    class InformationButton : Grid
    {
        const double _luminosityMultiplier = 1.1;

        readonly IMainThread _mainThread;
        readonly FloatingActionButtonView _informationFloatingActionButton, _statistic1FloatingActionButton, _statistic2FloatingActionButton, _statistic3FloatingActionButton;

        public InformationButton(MobileSortingService mobileSortingService, IMainThread mainThread)
        {
            _mainThread = mainThread;

            ColumnDefinitions = Columns.Define(AbsoluteGridLength(100));
            RowDefinitions = Rows.Define(AbsoluteGridLength(100));

            Children.Add(new FloatingActionButtonView { Size = FloatingActionButtonSize.Mini }.Center().Assign(out _statistic1FloatingActionButton)
                            .Bind<FloatingActionButtonView, IReadOnlyList<Repository>, Color>(FloatingActionButtonView.ColorNormalProperty, nameof(RepositoryViewModel.VisibleRepositoryList), convert: repositories => GetStatistic1BackgroundColor(mobileSortingService))
                            .Invoke(fab => fab.SetBinding(IsVisibleProperty, getIsVisibleBinding())));

            Children.Add(new FloatingActionButtonView { Size = FloatingActionButtonSize.Mini }.Center().Assign(out _statistic2FloatingActionButton)
                            .Bind<FloatingActionButtonView, IReadOnlyList<Repository>, Color>(FloatingActionButtonView.ColorNormalProperty, nameof(RepositoryViewModel.VisibleRepositoryList), convert: repositories => GetStatistic2BackgroundColor(mobileSortingService))
                            .Invoke(fab => fab.SetBinding(IsVisibleProperty, getIsVisibleBinding())));

            Children.Add(new FloatingActionButtonView { Size = FloatingActionButtonSize.Mini }.Center().Assign(out _statistic3FloatingActionButton)
                            .Bind<FloatingActionButtonView, IReadOnlyList<Repository>, Color>(FloatingActionButtonView.ColorNormalProperty, nameof(RepositoryViewModel.VisibleRepositoryList), convert: repositories => GetStatistic3BackgroundColor(mobileSortingService))
                            .Invoke(fab => fab.SetBinding(IsVisibleProperty, getIsVisibleBinding())));

            Children.Add(new FloatingActionButtonView { Command = new AsyncCommand(ExecuteFloatingActionButtonCommand) }.Center().Assign(out _informationFloatingActionButton)
                .DynamicResources((FloatingActionButtonView.ColorNormalProperty, nameof(BaseTheme.NavigationBarBackgroundColor)),
                                    (FloatingActionButtonView.RippleColorProperty, nameof(BaseTheme.PageBackgroundColor)))
                .Invoke(fab => fab.SetBinding(IsVisibleProperty, getIsVisibleBinding())));

            static MultiBinding getIsVisibleBinding() => new MultiBinding
            {
                Converter = new InformationMultiValueConverter(),
                Bindings =
                {
                    new Binding(nameof(RepositoryViewModel.IsRefreshing), BindingMode.OneWay),
                    new Binding(nameof(RepositoryViewModel.VisibleRepositoryList), BindingMode.OneWay)
                }
            };
        }

        //SortingOption.Clones - Total Clones, BaseTheme.CardClonesStatsIconColor
        //SortingOption.Views - Total Views, BaseTheme.CardViewsStatsIconColor
        //SortingOption.IssuesForks - Stars, BaseTheme.CardStarsStatsIconColor
        static Color GetStatistic1BackgroundColor(in MobileSortingService mobileSortingService)
        {
            var color = MobileSortingService.GetSortingCategory(mobileSortingService.CurrentOption) switch
            {
                SortingCategory.Clones => (Color)Application.Current.Resources[nameof(BaseTheme.CardClonesStatsIconColor)],
                SortingCategory.Views => (Color)Application.Current.Resources[nameof(BaseTheme.CardViewsStatsIconColor)],
                SortingCategory.IssuesForks => (Color)Application.Current.Resources[nameof(BaseTheme.CardStarsStatsIconColor)],
                _ => throw new NotImplementedException()
            };

            return color.MultiplyAlpha(_luminosityMultiplier);
        }

        //SortingOption.Clones - Unique Clones, BaseTheme.CardUniqueClonesStatsIconColor
        //SortingOption.Views - Unique Views, BaseTheme.CardUniqueViewsStatsIconColor
        //SortingOption.IssuesForks - Forks, BaseTheme.CardForksStatsIconColor
        static Color GetStatistic2BackgroundColor(in MobileSortingService mobileSortingService)
        {
            var color = MobileSortingService.GetSortingCategory(mobileSortingService.CurrentOption) switch
            {
                SortingCategory.Clones => (Color)Application.Current.Resources[nameof(BaseTheme.CardUniqueClonesStatsIconColor)],
                SortingCategory.Views => (Color)Application.Current.Resources[nameof(BaseTheme.CardUniqueViewsStatsIconColor)],
                SortingCategory.IssuesForks => (Color)Application.Current.Resources[nameof(BaseTheme.CardForksStatsIconColor)],
                _ => throw new NotImplementedException()
            };

            return color.MultiplyAlpha(_luminosityMultiplier);
        }

        //SortingOption.Clones - Stars, BaseTheme.CardStarsStatsIconColor
        //SortingOption.Views - Stars. BaseTheme.CardStarsStatsIconColor
        //SortingOption.IssuesForks - Issues, BaseTheme.CardIssuesStatsIconColor
        static Color GetStatistic3BackgroundColor(in MobileSortingService mobileSortingService)
        {
            var color = MobileSortingService.GetSortingCategory(mobileSortingService.CurrentOption) switch
            {
                SortingCategory.Clones => (Color)Application.Current.Resources[nameof(BaseTheme.CardStarsStatsIconColor)],
                SortingCategory.Views => (Color)Application.Current.Resources[nameof(BaseTheme.CardStarsStatsIconColor)],
                SortingCategory.IssuesForks => (Color)Application.Current.Resources[nameof(BaseTheme.CardIssuesStatsIconColor)],
                _ => throw new NotImplementedException()
            };

            return color.MultiplyAlpha(_luminosityMultiplier);
        }

        Task ExecuteFloatingActionButtonCommand()
        {
            const int animationDuration = 300;

            if (_informationFloatingActionButton is null || _statistic1FloatingActionButton is null || _statistic2FloatingActionButton is null || _statistic3FloatingActionButton is null)
                throw new NotSupportedException();

            return _mainThread.InvokeOnMainThreadAsync(() =>
            {
                if (isVisible(_statistic1FloatingActionButton)
                  && isVisible(_statistic2FloatingActionButton)
                  && isVisible(_statistic3FloatingActionButton))
                {
                    return Task.WhenAll(_statistic1FloatingActionButton.TranslateTo(0, 0, animationDuration, Easing.SpringIn),
                                           _statistic2FloatingActionButton.TranslateTo(0, 0, animationDuration, Easing.SpringIn),
                                           _statistic3FloatingActionButton.TranslateTo(0, 0, animationDuration, Easing.SpringIn));
                }

                return Task.WhenAll(_statistic1FloatingActionButton.TranslateTo(-75, 10, animationDuration, Easing.SpringOut),
                                        _statistic2FloatingActionButton.TranslateTo(-50, -50, animationDuration, Easing.SpringOut),
                                        _statistic3FloatingActionButton.TranslateTo(10, -75, animationDuration, Easing.SpringOut));
            });

            static bool isVisible(in FloatingActionButtonView statisticButton) => statisticButton.TranslationX != 0 && statisticButton.TranslationY != 0;
        }

        class InformationMultiValueConverter : IMultiValueConverter
        {
            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            {
                if (values[0] is bool isRefreshing && values[1] is IReadOnlyList<Repository> repositoryList)
                    return !isRefreshing && repositoryList.Any();

                return false;
            }

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
        }
    }
}
