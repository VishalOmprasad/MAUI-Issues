#if ANDROID
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Widget;
#endif
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Layouts;
using SkiaSharp;
using System.Reflection;

namespace MauiToolkitPopupSample;

public partial class PopupPage : Popup
{
    private const string _filename = "imageeditordesktop.png";


    public PopupPage()
	{
		InitializeComponent();

        image2.Source = ImageSource.FromFile(_filename);

        image3.Source = ImageSource.FromStream(_
            => FileSystem.OpenAppPackageFileAsync($"images/{_filename}"));

        //Assembly assembly = Assembly.GetExecutingAssembly();
        //image3.Source = ImageSource.FromStream(() => assembly.GetManifestResourceStream("MauiToolkitPopupSample.Resources.Images.imageeditordesktop.png"));
    }

	private void CloseButtonClicked(object sender, EventArgs e)
	{
		Close();
    }

    private static (int width, int height) GetImageDimensions(
        Stream source)
    {
        Stream s = source;
        if (!s.CanSeek)
        {
            var ms = new MemoryStream();
            s.CopyTo(ms);
            ms.Position = 0;
            s = ms;
        }

        using var skStream = new SKManagedStream(s, disposeManagedStream: false);
        using var code = SKCodec.Create(skStream)
            ?? throw new InvalidOperationException("Unsupported");

        var info = code.Info;

        return (info.Width, info.Height);
    }

    private async void DimensionButtonClicked(object sender, EventArgs e)
    {
        Size originalSize1 = image1.ComputeDesiredSize(double.PositiveInfinity, double.PositiveInfinity);
        Size originalSize2 = image2.ComputeDesiredSize(double.PositiveInfinity, double.PositiveInfinity);
        Size originalSize3 = image3.ComputeDesiredSize(double.PositiveInfinity, double.PositiveInfinity);

        Console.WriteLine($"Image 1 dimensions: {originalSize1.Width} x {originalSize1.Height}"); // Image 1 size: 2879 x 1612
        Console.WriteLine($"Image 2 dimensions: {originalSize2.Width} x {originalSize2.Height}"); // Image 2 size: 2879 x 1612
        Console.WriteLine($"Image 3 dimensions: {originalSize3.Width} x {originalSize3.Height}"); // Image 1 size: changes

#if ANDROID
        Android.Graphics.Bitmap? bitmapImage1 = GetImageBitmapView(image1);
        var imageStream1 = ConvertImageToStream(bitmapImage1);
        Android.Graphics.Bitmap? bitmapImage2 = GetImageBitmapView(image2);
        var imageStream2 = ConvertImageToStream(bitmapImage2);
        Android.Graphics.Bitmap? bitmapImage3 = GetImageBitmapView(image3);
        var imageStream3 = ConvertImageToStream(bitmapImage3);

        var fileDimensions1 = GetImageDimensions(imageStream1);
        var fileDimensions2 = GetImageDimensions(imageStream2);
        var fileDimensions3 = GetImageDimensions(imageStream3);

        Console.WriteLine($"Image 1 dimensions: {fileDimensions1.width} x {fileDimensions1.height}"); // Image 1 dimensions: 2879 x 1612
        Console.WriteLine($"Image 2 dimensions: {fileDimensions2.width} x {fileDimensions2.height}"); // Image 2 dimensions: 2879 x 1612
        Console.WriteLine($"Image 3 dimensions: {fileDimensions3.width} x {fileDimensions3.height}"); // Image 3 dimesnsions: dimension changes
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

    private Stream ConvertImageToStream(Android.Graphics.Bitmap? mergedBitmap)
    {
        if (mergedBitmap != null)
        {
            Bitmap.CompressFormat? compressFormat = Bitmap.CompressFormat.Png;
            if (compressFormat != null)
            {
                MemoryStream stream = new MemoryStream();
                mergedBitmap.Compress(compressFormat, 100, stream);

                stream.Position = 0;
                return stream;
            }
        }

        return Stream.Null;
    }

#endif
}