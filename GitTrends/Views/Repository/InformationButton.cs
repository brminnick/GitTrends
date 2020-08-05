using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
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
        const int _diameter = 100;

        readonly IMainThread _mainThread;
        readonly IAnalyticsService _analyticsService;
        readonly FloatingActionTextButton _statistic1FloatingActionButton, _statistic2FloatingActionButton, _statistic3FloatingActionButton;

        public InformationButton(MobileSortingService mobileSortingService, in IMainThread mainThread, in IAnalyticsService analyticsService)
        {
            _mainThread = mainThread;
            _analyticsService = analyticsService;

            AutomationId = RepositoryPageAutomationIds.InformationButton;

            RowDefinitions = Rows.Define(AbsoluteGridLength(_diameter));
            ColumnDefinitions = Columns.Define(AbsoluteGridLength(_diameter));

            Children.Add(new FloatingActionTextButton(mobileSortingService, FloatingActionButtonSize.Mini, FloatingActionButtonType.Statistic1).Assign(out _statistic1FloatingActionButton)
                            .Bind<FloatingActionTextButton, IReadOnlyList<Repository>, string>(FloatingActionTextButton.TextProperty, nameof(RepositoryViewModel.VisibleRepositoryList), convert: repositories => StatisticsService.GetFloatingActionTextButtonText(mobileSortingService, repositories, FloatingActionButtonType.Statistic1))
                            .Invoke(fab => fab.SetBinding(IsVisibleProperty, getIsVisibleBinding())));

            Children.Add(new FloatingActionTextButton(mobileSortingService, FloatingActionButtonSize.Mini, FloatingActionButtonType.Statistic2).Assign(out _statistic2FloatingActionButton)
                            .Bind<FloatingActionTextButton, IReadOnlyList<Repository>, string>(FloatingActionTextButton.TextProperty, nameof(RepositoryViewModel.VisibleRepositoryList), convert: repositories => StatisticsService.GetFloatingActionTextButtonText(mobileSortingService, repositories, FloatingActionButtonType.Statistic2))
                            .Invoke(fab => fab.SetBinding(IsVisibleProperty, getIsVisibleBinding())));

            Children.Add(new FloatingActionTextButton(mobileSortingService, FloatingActionButtonSize.Mini, FloatingActionButtonType.Statistic3).Assign(out _statistic3FloatingActionButton)
                            .Bind<FloatingActionTextButton, IReadOnlyList<Repository>, string>(FloatingActionTextButton.TextProperty, nameof(RepositoryViewModel.VisibleRepositoryList), convert: repositories => StatisticsService.GetFloatingActionTextButtonText(mobileSortingService, repositories, FloatingActionButtonType.Statistic3))
                            .Invoke(fab => fab.SetBinding(IsVisibleProperty, getIsVisibleBinding())));

            Children.Add(new FloatingActionTextButton(mobileSortingService, FloatingActionButtonSize.Normal, FloatingActionButtonType.Information, new AsyncCommand(ExecuteFloatingActionButtonCommand)) { FontFamily = FontFamilyConstants.RobotoMedium, Text = "TOTAL" }.Center()
                            .DynamicResource(FloatingActionButtonView.RippleColorProperty, nameof(BaseTheme.PageBackgroundColor))
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
            const string informationButtonTapped = "Information Button Tapped";

            return _mainThread.InvokeOnMainThreadAsync(() =>
            {
                if (isVisible(_statistic1FloatingActionButton) && isVisible(_statistic2FloatingActionButton) && isVisible(_statistic3FloatingActionButton))
                {
                    _analyticsService.Track(informationButtonTapped, nameof(isVisible), "true");

                    return Task.WhenAll(_statistic1FloatingActionButton.TranslateTo(0, 0, animationDuration, Easing.SpringIn), _statistic1FloatingActionButton.RotateTo(0, animationDuration, Easing.SpringIn),
                                           _statistic2FloatingActionButton.TranslateTo(0, 0, animationDuration, Easing.SpringIn), _statistic2FloatingActionButton.RotateTo(0, animationDuration, Easing.SpringIn),
                                           _statistic3FloatingActionButton.TranslateTo(0, 0, animationDuration, Easing.SpringIn), _statistic3FloatingActionButton.RotateTo(0, animationDuration, Easing.SpringIn));
                }

                _analyticsService.Track(informationButtonTapped, nameof(isVisible), "false");

                return Task.WhenAll(_statistic1FloatingActionButton.TranslateTo(-75, 10, animationDuration, Easing.SpringOut), _statistic1FloatingActionButton.RotateTo(360, animationDuration, Easing.SpringOut),
                                        _statistic2FloatingActionButton.TranslateTo(-50, -50, animationDuration, Easing.SpringOut), _statistic2FloatingActionButton.RotateTo(360, animationDuration, Easing.SpringOut),
                                        _statistic3FloatingActionButton.TranslateTo(10, -75, animationDuration, Easing.SpringOut), _statistic3FloatingActionButton.RotateTo(360, animationDuration, Easing.SpringOut));
            });

            static bool isVisible(in FloatingActionTextButton statisticButton) => statisticButton.TranslationX != 0 && statisticButton.TranslationY != 0;
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

        class FloatingActionTextButton : Grid
        {
            public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(FloatingActionTextButton), string.Empty);
            public static readonly BindableProperty FontFamilyProperty = BindableProperty.Create(nameof(FontFamily), typeof(string), typeof(FloatingActionTextButton), null);

            readonly MobileSortingService _mobileSortingService;
            readonly FloatingActionButtonView _floatingActionButtonView;
            readonly FloatingActionButtonType _floatingActionButtonType;

            public FloatingActionTextButton(in MobileSortingService mobileSortingService,
                                            in FloatingActionButtonSize floatingActionButtonSize,
                                            in FloatingActionButtonType floatingActionButtonType,
                                            in ICommand? command = null)
            {
                _mobileSortingService = mobileSortingService;
                _floatingActionButtonType = floatingActionButtonType;
                ThemeService.PreferenceChanged += HandlePreferenceChanged;

                var fontSize = floatingActionButtonSize switch
                {
                    FloatingActionButtonSize.Mini => 10,
                    FloatingActionButtonSize.Normal => 13,
                    _ => throw new NotImplementedException(),
                };

                RowDefinitions = Rows.Define(AbsoluteGridLength(_diameter));
                ColumnDefinitions = Columns.Define(AbsoluteGridLength(_diameter));

                Children.Add(new FloatingActionButtonView { Size = floatingActionButtonSize, Command = command }.Center().Assign(out _floatingActionButtonView)
                                .Bind<FloatingActionButtonView, IReadOnlyList<Repository>, Color>(FloatingActionButtonView.ColorNormalProperty, nameof(RepositoryViewModel.VisibleRepositoryList), convert: repositories => GetBackgroundColor()));

                Children.Add(new TextLabel(floatingActionButtonType).Center().TextCenter().Font(fontSize)
                                .Bind(Label.TextProperty, nameof(Text), source: this)
                                .Bind(Label.FontFamilyProperty, nameof(FontFamily), source: this));
            }

            public string Text
            {
                get => (string)GetValue(TextProperty);
                set => SetValue(TextProperty, value);
            }

            public string? FontFamily
            {
                get => (string?)GetValue(FontFamilyProperty);
                set => SetValue(FontFamilyProperty, value);
            }

            Color GetBackgroundColor()
            {
                var color = (MobileSortingService.GetSortingCategory(_mobileSortingService.CurrentOption), _floatingActionButtonType) switch
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
                    (_, FloatingActionButtonType.Information) => Color.FromHex(BaseTheme.LightTealColorHex).AddLuminosity(.05),
                    (_, _) => throw new NotImplementedException()
                };

                return color.AddLuminosity(.1);
            }

            void HandlePreferenceChanged(object sender, PreferredTheme e) => _floatingActionButtonView.ColorNormal = GetBackgroundColor();

            class TextLabel : Label
            {
                public TextLabel(in FloatingActionButtonType floatingActionButtonType)
                {
                    TextColor = Color.Black;
                    InputTransparent = true;
                    AutomationId = RepositoryPageAutomationIds.GetFloatingActionTextButtonLabelAutomationId(floatingActionButtonType);
                }
            }
        }
    }
}
