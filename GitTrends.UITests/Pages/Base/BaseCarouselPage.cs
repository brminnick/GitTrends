using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Xamarin.UITest;

namespace GitTrends.UITests
{
	abstract class BaseCarouselPage : BasePage
	{
		readonly string _getCurrentPageNumberBackdoorMethodConstant;

		protected BaseCarouselPage(in IApp app, in string getCurrentPageNumberBackdoorMethodConstant) : base(app)
		{
			_getCurrentPageNumberBackdoorMethodConstant = getCurrentPageNumberBackdoorMethodConstant;
		}

		public int CurrentPageNumber => App.InvokeBackdoorMethod<int>(_getCurrentPageNumberBackdoorMethodConstant);

		public async Task MoveToNextPage()
		{
			var initialPageNumber = CurrentPageNumber;

			var screenSize = App.Query().First().Rect;
			App.DragCoordinates(screenSize.Width * 9 / 10, screenSize.CenterY, screenSize.Width * 1 / 10, screenSize.CenterY);

			while (CurrentPageNumber == initialPageNumber)
				await Task.Delay(TimeSpan.FromMilliseconds(10)).ConfigureAwait(false);

			var finalPageNumber = CurrentPageNumber;

			App.Screenshot($"Moved from Page {initialPageNumber} to Page {finalPageNumber}");

			Assert.GreaterOrEqual(finalPageNumber, initialPageNumber);
		}

		public async Task MoveToPreviousPage()
		{
			var initialPageNumber = CurrentPageNumber;

			var screenSize = App.Query().First().Rect;
			App.DragCoordinates(screenSize.Width * 1 / 10, screenSize.CenterY, screenSize.Width * 9 / 10, screenSize.CenterY);

			while (CurrentPageNumber == initialPageNumber)
				await Task.Delay(TimeSpan.FromMilliseconds(10)).ConfigureAwait(false);

			var finalPageNumber = CurrentPageNumber;

			App.Screenshot($"Moved from Page {initialPageNumber} to Page {finalPageNumber}");

			Assert.LessOrEqual(finalPageNumber, initialPageNumber);
		}
	}
}