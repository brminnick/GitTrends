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
        const int _diameter = 100;

        readonly IMainThread _mainThread;
        readonly FloatingActionButtonView _informationFloatingActionButton;
        readonly FloatingActionTextButton _statistic1FloatingActionButton, _statistic2FloatingActionButton, _statistic3FloatingActionButton;

        public InformationButton(in MobileSortingService mobileSortingService, in IMainThread mainThread)
        {
            _mainThread = mainThread;

            RowDefinitions = Rows.Define(AbsoluteGridLength(_diameter));
            ColumnDefinitions = Columns.Define(AbsoluteGridLength(_diameter));

            Children.Add(new FloatingActionTextButton(mobileSortingService, FloatingActionButtonType.Statistic1).Assign(out _statistic1FloatingActionButton)
                            .Invoke(fab => fab.SetBinding(IsVisibleProperty, getIsVisibleBinding())));

            Children.Add(new FloatingActionTextButton(mobileSortingService, FloatingActionButtonType.Statistic2).Assign(out _statistic2FloatingActionButton)
                            .Invoke(fab => fab.SetBinding(IsVisibleProperty, getIsVisibleBinding())));

            Children.Add(new FloatingActionTextButton(mobileSortingService, FloatingActionButtonType.Statistic3).Assign(out _statistic3FloatingActionButton)
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

            static bool isVisible(in FloatingActionTextButton statisticButton) => statisticButton.TranslationX != 0 && statisticButton.TranslationY != 0;
        }

        enum FloatingActionButtonType { Information, Statistic1, Statistic2, Statistic3 }

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

        class FloatingActionTextButton : Grid
        {
            public FloatingActionTextButton(MobileSortingService mobileSortingService, FloatingActionButtonType floatingActionButtonType)
            {
                RowDefinitions = Rows.Define(AbsoluteGridLength(_diameter));
                ColumnDefinitions =  Columns.Define(AbsoluteGridLength(_diameter));

                Children.Add(new FloatingActionButtonView { Size = FloatingActionButtonSize.Mini }.Center()
                                .Bind<FloatingActionButtonView, IReadOnlyList<Repository>, Color>(FloatingActionButtonView.ColorNormalProperty, nameof(RepositoryViewModel.VisibleRepositoryList), convert: repositories => GetBackgroundColorConverter(mobileSortingService, floatingActionButtonType)));


                Children.Add(new Label { TextColor = Color.White }.Center().TextCenter().Font(10)
                                .Bind<Label, IReadOnlyList<Repository>, string>(Label.TextProperty, nameof(RepositoryViewModel.VisibleRepositoryList), convert: repositories => GetLabelTextConverter(mobileSortingService, repositories, floatingActionButtonType)));
            }

            static Color GetBackgroundColorConverter(in MobileSortingService mobileSortingService, in FloatingActionButtonType floatingActionButtonType)
            {
                var color = (MobileSortingService.GetSortingCategory(mobileSortingService.CurrentOption), floatingActionButtonType) switch
                {
                    (SortingCategory.Clones, FloatingActionButtonType.Statistic1) => (Color)Application.Current.Resources[nameof(BaseTheme.CardClonesStatsIconColor)],
                    (SortingCategory.Clones, FloatingActionButtonType.Statistic2) => (Color)Application.Current.Resources[nameof(BaseTheme.CardUniqueClonesStatsIconColor)],
                    (SortingCategory.Clones, FloatingActionButtonType.Statistic3) => (Color)Application.Current.Resources[nameof(BaseTheme.CardStarsStatsIconColor)],
                    (SortingCategory.Views, FloatingActionButtonType.Statistic1) => (Color)Application.Current.Resources[nameof(BaseTheme.CardViewsStatsIconColor)],
                    (SortingCategory.Views, FloatingActionButtonType.Statistic2) => (Color)Application.Current.Resources[nameof(BaseTheme.CardUniqueViewsStatsIconColor)],
                    (SortingCategory.Views, FloatingActionButtonType.Statistic3) => (Color)Application.Current.Resources[nameof(BaseTheme.CardStarsStatsIconColor)],
                    (SortingCategory.IssuesForks, FloatingActionButtonType.Statistic1) => (Color)Application.Current.Resources[nameof(BaseTheme.CardStarsStatsIconColor)],
                    (SortingCategory.IssuesForks, FloatingActionButtonType.Statistic2) => (Color)Application.Current.Resources[nameof(BaseTheme.CardForksStatsIconColor)],
                    (SortingCategory.IssuesForks, FloatingActionButtonType.Statistic3) => (Color)Application.Current.Resources[nameof(BaseTheme.CardIssuesStatsIconColor)],
                    (_, FloatingActionButtonType.Information) => throw new NotSupportedException(),
                    (_, _) => throw new NotImplementedException()
                };
                return color.MultiplyAlpha(_luminosityMultiplier);
            }

            static string GetLabelTextConverter(in MobileSortingService mobileSortingService, in IReadOnlyList<Repository> repositories, in FloatingActionButtonType floatingActionButtonType)
            {
                return (MobileSortingService.GetSortingCategory(mobileSortingService.CurrentOption), floatingActionButtonType) switch
                {
                    (SortingCategory.Clones, FloatingActionButtonType.Statistic1) => repositories.Sum(x => x.TotalClones).ToAbbreviatedText(),
                    (SortingCategory.Clones, FloatingActionButtonType.Statistic2) => repositories.Sum(x => x.TotalUniqueClones).ToAbbreviatedText(),
                    (SortingCategory.Clones, FloatingActionButtonType.Statistic3) => repositories.Sum(x => x.StarCount).ToAbbreviatedText(),
                    (SortingCategory.Views, FloatingActionButtonType.Statistic1) => repositories.Sum(x => x.TotalViews).ToAbbreviatedText(),
                    (SortingCategory.Views, FloatingActionButtonType.Statistic2) => repositories.Sum(x => x.TotalUniqueViews).ToAbbreviatedText(),
                    (SortingCategory.Views, FloatingActionButtonType.Statistic3) => repositories.Sum(x => x.StarCount).ToAbbreviatedText(),
                    (SortingCategory.IssuesForks, FloatingActionButtonType.Statistic1) => repositories.Sum(x => x.StarCount).ToAbbreviatedText(),
                    (SortingCategory.IssuesForks, FloatingActionButtonType.Statistic2) => repositories.Sum(x => x.ForkCount).ToAbbreviatedText(),
                    (SortingCategory.IssuesForks, FloatingActionButtonType.Statistic3) => repositories.Sum(x => x.IssuesCount).ToAbbreviatedText(),
                    (_, FloatingActionButtonType.Information) => throw new NotSupportedException(),
                    (_, _) => throw new NotImplementedException()
                };
            }

        }
    }
}
