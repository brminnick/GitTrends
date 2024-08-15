using GitTrends.Mobile.Common;
using Microsoft.Maui.Platform;

namespace GitTrends;

partial class MauiProgram
{
	static void CustomizeHandlers()
	{
		AddPickerBorderCustomHandler();
	}
	
	static void AddPickerBorderCustomHandler()
	{
		Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping("PickerBorder", (handler, view) =>
		{
			ThemeService.PreferenceChanged += HandlePreferenceChanged;
			
			handler.PlatformView.TextAlignment = UIKit.UITextAlignment.Center;
			handler.PlatformView.Layer.BorderWidth = 1;
			handler.PlatformView.Layer.CornerRadius = 5;

			SetPickerBorder();

			void SetPickerBorder()
			{
				if (Application.Current?.Resources is BaseTheme theme)
				{
					var borderColor = Resources.AppResources.GetResource<Color>(nameof(BaseTheme.PickerBorderColor));
					handler.PlatformView.Layer.BorderColor = borderColor.ToCGColor();
				}
			}

			void HandlePreferenceChanged(object? sender, PreferredTheme e) => SetPickerBorder();
		});
	}
}