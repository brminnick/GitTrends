using System;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using FFImageLoading.Svg.Forms;
using GitTrends.Mobile.Common;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Markup;

namespace GitTrends
{
    public class SvgImage : SvgCachedImage
    {
        public static readonly BindableProperty GetTextColorProperty =
            BindableProperty.Create(nameof(GetTextColorProperty), typeof(Func<Color>), typeof(SvgImage), (Func<Color>)(() => Color.Default), propertyChanged: HandleGetTextColorPropertyChanged);

        public SvgImage(in string svgFileName, in Func<Color> getTextColor, double widthRequest = 24, double heightRequest = 24)
        {
            if (!svgFileName.EndsWith(".svg"))
                throw new ArgumentException($"{nameof(svgFileName)} must end with .svg", nameof(svgFileName));

            ThemeService.PreferenceChanged += HandlePreferenceChanged;

            this.FillExpand();

            GetTextColor = getTextColor;

            Source = SvgService.GetFullPath(svgFileName);

            WidthRequest = widthRequest;
            HeightRequest = heightRequest;
        }

        public Func<Color> GetTextColor
        {
            get => (Func<Color>)GetValue(GetTextColorProperty);
            set
            {
                SetSvgColorAsync().SafeFireAndForget();

                if (value() != GetTextColor())
                {
                    SetValue(GetTextColorProperty, value);
                    OnPropertyChanged(nameof(GetTextColor));
                }
            }
        }

        static void HandleGetTextColorPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var svgImage = (SvgImage)bindable;
            svgImage.GetTextColor = (Func<Color>)newValue;
        }

        void HandlePreferenceChanged(object sender, PreferredTheme e) => SetSvgColor();

        Task SetSvgColorAsync() => MainThread.InvokeOnMainThreadAsync(SetSvgColor);

        void SetSvgColor() => ReplaceStringMap = SvgService.GetColorStringMap(GetTextColor());
    }
}
