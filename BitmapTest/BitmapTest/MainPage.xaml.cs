#if ANDROID
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Widget;
using Javax.Annotation.Meta;
using System.Reflection;
#endif

namespace BitmapTest
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            // When loading Embedded image as Stream or Resource, any of the below codes (either FromResource() or FromStream())

            //this.image.Source = ImageSource.FromResource("BitmapTest.Resources.Images.bmpimage.bmp");

            // OR

            //Assembly assembly = Assembly.GetExecutingAssembly();
            //this.image.Source = ImageSource.FromStream(() => assembly.GetManifestResourceStream("BitmapTest.Resources.Images.bmpimage.bmp"));

        }

        private void OnButtonClicked(object sender, EventArgs e)
        {
#if ANDROID
            Android.Graphics.Bitmap? bitmapImage = GetImageBitmapView(image1);
#endif
        }

#if ANDROID

        private Android.Graphics.Bitmap? GetImageBitmapView(Image image)
        {
            Android.Graphics.Bitmap? processedBitmap = null;
            if ((image.Handler.PlatformView is ImageView imageView))
            {
                if (imageView.Drawable is BitmapDrawable drawable)
                {
                    processedBitmap = drawable.Bitmap?.Copy(drawable.Bitmap.GetConfig(), true);
                }

                if (processedBitmap == null && Bitmap.Config.Argb8888 != null)
                {
                    processedBitmap = Bitmap.CreateBitmap(imageView.Width, imageView.Height, Bitmap.Config.Argb8888);
                    if (processedBitmap != null)
                    {
                        Canvas canvas = new Canvas(processedBitmap);
                        imageView.Draw(canvas);
                    }
                }
            }

            return processedBitmap;
        }
#endif

    }
}
