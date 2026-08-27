#if WINDOWS
//using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
#endif

namespace CustomImage
{
    public partial class MainPage : ContentPage
    {
#if WINDOWS
        private byte[]? imageBytes;
        
        private IRandomAccessStream? processedImageStream;
#endif

        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnRotateButtonClicked(object sender, EventArgs e)
        {
#if WINDOWS
            this.image.Rotation += 90;
            
            // Get current stream
            IRandomAccessStream? stream = await this.GetImageStream();
            if (stream == null)
                return;

            // Apply rotation to bitmap data
            await this.RotateImage(stream, 90);

            stream.Dispose();
#endif
        }

        private async void OnSaveButtonClicked(object sender, EventArgs e)
        {
#if WINDOWS
            IRandomAccessStream? stream = await this.GetImageStream();
            if (stream == null)
                return;

            using (var memoryStream = new MemoryStream())
            {
                await stream.AsStream().CopyToAsync(memoryStream);
                imageBytes = memoryStream.ToArray();
            }
            stream.Dispose();

            // KEY: Recreate processedImageStream from cached bytes
            // This ensures the stream persists and can be re-used on next GetImageStream()
            this.processedImageStream?.Dispose();

            // Reset visual rotation since rotation is now baked into the bitmap pixels
            this.image.Rotation = 0;

            this.image.Source = ImageSource.FromStream(() => new MemoryStream(imageBytes));
#endif
        }


#if WINDOWS
        private async Task RotateImage(IRandomAccessStream imageStream, double angle)
        {
            IRandomAccessStream rotateStream = new InMemoryRandomAccessStream();
            using (var stream = imageStream.AsStream())
            {
                var randomAccessStream = stream.AsRandomAccessStream();
                var decoder = await BitmapDecoder.CreateAsync(randomAccessStream);
                var pixelData = await decoder.GetPixelDataAsync();
                var pixels = pixelData.DetachPixelData();

                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, rotateStream);
                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied,
                                     decoder.OrientedPixelWidth, decoder.OrientedPixelHeight,
                                     decoder.DpiX, decoder.DpiY, pixels);

                encoder.BitmapTransform.Rotation = angle > 0 ? BitmapRotation.Clockwise90Degrees : BitmapRotation.Clockwise270Degrees;

                await encoder.FlushAsync();
            }

            this.processedImageStream?.Dispose();
            this.processedImageStream = rotateStream;
        }
        
        internal async Task<IRandomAccessStream?> GetImageStream()
        {
            if (this.processedImageStream == null)
            {
                await this.SetImageStream();
            }

            if (this.processedImageStream != null && this.processedImageStream.CanRead)
            {
                return this.processedImageStream?.CloneStream();
            }

            return null;
        }

        private async Task SetImageStream()
        {
            if (this.processedImageStream != null)
            {
                this.processedImageStream.Dispose();
                this.processedImageStream = null;
            }

            // Mirrors ImageEditorHandler.SetImageStream() — handles all ImageSource types
            if (this.image.Source is FileImageSource fileSource)
            {
                // Construct ms-appx URI so GetBitmapStream() handles both packaged and unpackaged
                // (unpackaged: resolves scale-suffixed file from AppDomain.CurrentDomain.BaseDirectory)
                var uri = new Uri("ms-appx:///" + fileSource.File);
                this.processedImageStream = await this.GetBitmapStream(uri);
            }
        }

        internal async Task<IRandomAccessStream?> GetBitmapStream(Uri source)
        {
            if (source != null)
            {
                if (!source.IsFile)
                {
                    if (source.IsAbsoluteUri)
                    {
                        if (AppInfo.PackagingModel == AppPackagingModel.Unpackaged)
                        {
                            var displayScale = DeviceDisplay.MainDisplayInfo.Density;
                            int scaleValue = displayScale >= 3.5 ? 400 : displayScale >= 1.5 ? 200 : 100;
                            string actualImageName = source.Segments[source.Segments.Count() - 1];
                            int dotIndex = actualImageName.IndexOf('.');
                            string imageName = string.Empty;
                            string imageType = string.Empty;
                            if (dotIndex >= 0)
                            {
                                imageName = actualImageName.Substring(0, dotIndex);
                                imageType = actualImageName.Substring(dotIndex);
                            }

                            var imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                $"{imageName}.scale-{scaleValue}{imageType}");

                            //// - If the image is not available in the specified scale value, then the image will be loaded with the default scale value.
                            if (File.Exists(imagePath))
                            {
                                StorageFile imageFilePath = await StorageFile.GetFileFromPathAsync(imagePath);

                                IRandomAccessStreamReference streamRef = RandomAccessStreamReference.CreateFromFile(imageFilePath);
                                IRandomAccessStreamWithContentType stream = await streamRef.OpenReadAsync();
                                return stream;
                            }
                        }
                    }
                }
            }

            return null;
        }
#endif
    }
}