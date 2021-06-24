using System;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using FFImageLoading.Svg.Forms;
using GitTrends.Mobile.Common;
using Xamarin.CommunityToolkit.Markup;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace GitTrends
{
    public class SvgImage : SvgCachedImage
    {
        public static readonly BindableProperty GetColorProperty =
            BindableProperty.Create(nameof(GetColorProperty), typeof(Func<Color>), typeof(SvgImage), (Func<Color>)(() => Color.Default), propertyChanged: HandleGetTextColorPropertyChanged);

        public SvgImage(in string svgFileName, in Func<Color> getColor, double widthRequest = 24, double heightRequest = 24)
        {
            if (!svgFileName.EndsWith(".svg"))
                throw new ArgumentException($"{nameof(svgFileName)} must end with .svg", nameof(svgFileName));

            ThemeService.PreferenceChanged += HandlePreferenceChanged;

            this.FillExpand();

            GetColor = getColor;

            Source = SvgService.GetFullPath(svgFileName);

            WidthRequest = widthRequest;
            HeightRequest = heightRequest;
        }

        public Func<Color> GetColor
        {
            get => (Func<Color>)GetValue(GetColorProperty);
            set
            {
                SetSvgColorAsync().SafeFireAndForget();

                if (value() != GetColor())
                {
                    SetValue(GetColorProperty, value);
                    OnPropertyChanged(nameof(GetColor));
                }
            }
        }

        static void HandleGetTextColorPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var svgImage = (SvgImage)bindable;
            svgImage.GetColor = (Func<Color>)newValue;
        }

        void HandlePreferenceChanged(object sender, PreferredTheme e) => SetSvgColor();

        Task SetSvgColorAsync() => MainThread.InvokeOnMainThreadAsync(SetSvgColor);

        void SetSvgColor() => ReplaceStringMap = new(SvgService.GetColorStringMap(GetColor()));
    }
}
