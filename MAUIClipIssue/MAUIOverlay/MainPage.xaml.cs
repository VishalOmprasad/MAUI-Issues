using Microsoft.Maui.Controls.Shapes;

namespace MAUIOverlay
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }
    }

    public class CustomView : StackLayout
    {
        protected override void OnSizeAllocated(double width, double height)
        {
            this.Clip = new RoundRectangleGeometry { Rect = new Rect(0, 0, width, height)};
            base.OnSizeAllocated(width, height);
        }
    }
}
