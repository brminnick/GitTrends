using System;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using FFImageLoading;
using FFImageLoading.Svg.Forms;
using GitTrends.Mobile.Common;
using Xamarin.CommunityToolkit.Markup;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace GitTrends;

public class SvgImage : SvgCachedImage
{
	public static readonly BindableProperty GetColorProperty =
		BindableProperty.Create(nameof(GetColor), typeof(Func<Color>), typeof(SvgImage), (Func<Color>)(() => Color.Default), propertyChanged: HandleGetTextColorPropertyChanged);

	public SvgImage(in string svgFileName, in Func<Color> getColor, double widthRequest = 24, double heightRequest = 24)
		: this(getColor, widthRequest, heightRequest)
	{
		Source = SvgService.GetValidatedFullPath(svgFileName);
	}

	public SvgImage(in Func<Color> getColor, double widthRequest = 24, double heightRequest = 24)
		: this(widthRequest, heightRequest)
	{
		GetColor = getColor;
	}

	public SvgImage(double widthRequest = 24, double heightRequest = 24)
	{
		ThemeService.PreferenceChanged += HandlePreferenceChanged;

		this.FillExpand();

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

	// Ensure only Validated Full Path is 
	protected override ImageSource CoerceImageSource(object newValue)
	{
		if (newValue is UriImageSource uri)
			SvgService.GetValidatedSvgFileName(uri.Uri.ToString());
		else if (newValue is FileImageSource file)
			SvgService.GetValidatedSvgFileName(file.File.ToString());

		return base.CoerceImageSource(newValue);
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