using System;
using Xamarin.Forms;

namespace GitTrends
{
    abstract class BounceButton : Button
    {
        const int _animationTime = 100;
        const double _maxSize = 1.1;
        const double _normalSize = 1;

        public BounceButton() => Clicked += HandleButtonClick;

        async void HandleButtonClick(object sender, EventArgs e)
        {
            await this.ScaleTo(_maxSize, _animationTime);
            await this.ScaleTo(_normalSize, _animationTime);
        }
    }
}
