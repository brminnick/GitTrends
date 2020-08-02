using System;
using Xamarin.Forms.Platform.Android;
using Android.Widget;
using Xamarin.Forms;
using Android.Views;
using System.IO;
using GitTrends.Droid;
using Google.Android.Material.FloatingActionButton;
using Android.Content;
using GitTrends;
using Android.Graphics;
using Android.Content.Res;

//Inspired by https://github.com/jamesmontemagno/FloatingActionButton-for-Xamarin.Android
[assembly: ExportRenderer(typeof(FloatingActionButtonView), typeof(FloatingActionButtonViewRenderer))]
namespace GitTrends.Droid
{
    public class FloatingActionButtonViewRenderer : ViewRenderer<FloatingActionButtonView, FrameLayout>
    {
        const int _marginDips = 16;
        const int _normalHeight = 56;
        const int _miniHeight = 40;
        const int _frameHeight = (_marginDips * 2) + _normalHeight;
        const int _frameWidth = (_marginDips * 2) + _normalHeight;
        const int _miniFrameHeight = (_marginDips * 2) + _miniHeight;
        const int _miniFrameWidth = (_marginDips * 2) + _miniHeight;

        readonly FloatingActionButton _floatingActionButton;

        public FloatingActionButtonViewRenderer(Context context) : base(context)
        {
            var density = context.Resources?.DisplayMetrics?.Density ?? 0;
            var margin = (int)(_marginDips * density); // margin in pixels

            _floatingActionButton = new FloatingActionButton(context)
            {
                LayoutParameters = new FrameLayout.LayoutParams(LayoutParams.WrapContent, LayoutParams.WrapContent)
                {
                    Gravity = GravityFlags.CenterVertical | GravityFlags.CenterHorizontal,
                    LeftMargin = margin,
                    TopMargin = margin,
                    BottomMargin = margin,
                    RightMargin = margin
                }
            };
        }

        public static void Init()
        {
            var temp = DateTime.Now;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<FloatingActionButtonView> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null || Element is null || Context is null)
                return;

            if (e.OldElement != null)
                e.OldElement.PropertyChanged -= HandlePropertyChanged;

            Element.PropertyChanged += HandlePropertyChanged;

            SetFabImage(Element.ImageName);
            SetFabSize(Element.Size);

            _floatingActionButton.Elevation = 12;
            _floatingActionButton.BackgroundTintList = ColorStateList.ValueOf(Element.ColorNormal.ToAndroid());
            _floatingActionButton.RippleColor = Element.RippleColor.ToAndroid();
            _floatingActionButton.Click += Fab_Click;

            var frame = new FrameLayout(Context);
            frame.RemoveAllViews();
            frame.AddView(_floatingActionButton);

            SetNativeControl(frame);
        }

        void HandlePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName is "Content")
            {
                Tracker.UpdateLayout();
            }
            else if (e.PropertyName == FloatingActionButtonView.ColorNormalProperty.PropertyName)
            {
                _floatingActionButton.BackgroundTintList = ColorStateList.ValueOf(Element.ColorNormal.ToAndroid());
            }
            else if (e.PropertyName == FloatingActionButtonView.RippleColorProperty.PropertyName)
            {
                _floatingActionButton.RippleColor = Element.RippleColor.ToAndroid();
            }
            else if (e.PropertyName == FloatingActionButtonView.ImageNameProperty.PropertyName)
            {
                SetFabImage(Element.ImageName);
            }
            else if (e.PropertyName == FloatingActionButtonView.SizeProperty.PropertyName)
            {
                SetFabSize(Element.Size);
            }
        }

        void SetFabImage(string imageName)
        {
            if (!string.IsNullOrWhiteSpace(imageName) && Context?.Resources != null)
            {
                try
                {
                    var resources = Context.Resources;
                    var drawableNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(imageName).ToLower();

                    var imageResourceName = resources.GetIdentifier(drawableNameWithoutExtension, "drawable", Context.PackageName);

                    _floatingActionButton.SetImageBitmap(BitmapFactory.DecodeResource(Context.Resources, imageResourceName));
                }
                catch (Exception ex)
                {
                    throw new FileNotFoundException("There was no Android Drawable by that name.", ex);
                }
            }
        }

        void SetFabSize(FloatingActionButtonSize size)
        {
            if (size is FloatingActionButtonSize.Mini)
            {
                _floatingActionButton.Size = (int)(Resources?.GetDimension(Resource.Dimension.fab_size_mini) ?? _miniHeight);
                Element.WidthRequest = _miniFrameWidth;
                Element.HeightRequest = _miniFrameHeight;
            }
            else if (size is FloatingActionButtonSize.Normal)
            {
                _floatingActionButton.Size = (int)(Resources?.GetDimension(Resource.Dimension.fab_size_normal) ?? _normalHeight);
                Element.WidthRequest = _frameWidth;
                Element.HeightRequest = _frameHeight;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        void Fab_Click(object sender, EventArgs e)
        {
            if (Element?.Command?.CanExecute(Element?.CommandParameter) ?? false)
                Element.Command.Execute(Element?.CommandParameter);
        }
    }
}