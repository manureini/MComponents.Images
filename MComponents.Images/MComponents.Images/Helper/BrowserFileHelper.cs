using Microsoft.AspNetCore.Components.Forms;
using SkiaSharp;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MComponents.Images.Helper
{
    internal static class BrowserFileHelper
    {
        public const string DEFAULT_CONTENT_TYPE = "image/jpg";

        public static async Task<byte[]> ResizeAndGetImage(IBrowserFile pBrowerFile)
        {
            try
            {
                using var stream = pBrowerFile.OpenReadStream(Constants.MAX_FILE_SIZE);
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                ms.Seek(0, SeekOrigin.Begin);

                using var codec = SKCodec.Create(ms);
                if (codec == null)
                    return null;

                var origin = codec.EncodedOrigin;
                var bitmap = SKBitmap.Decode(codec);
                if (bitmap == null)
                    return null;

                bitmap = AutoOrient(bitmap, origin);

                if (bitmap.Width > 1000 || bitmap.Height > 1000)
                {
                    bitmap = ResizeBitmap(bitmap, 1000, 1000);
                }

                if (bitmap == null)
                    return null;

                using var image = SKImage.FromBitmap(bitmap);
                bitmap.Dispose();
                if (image == null)
                    return null;

                using var data = image.Encode(SKEncodedImageFormat.Jpeg, 80);
                if (data == null)
                    return null;

                return data.ToArray();
            }
            catch
            {
                return null;
            }
        }

        private static SKBitmap AutoOrient(SKBitmap pBitmap, SKEncodedOrigin pOrigin)
        {
            switch (pOrigin)
            {
                case SKEncodedOrigin.TopLeft:
                    return pBitmap;
                case SKEncodedOrigin.TopRight:
                    return Flip(pBitmap, true, false);
                case SKEncodedOrigin.BottomRight:
                    return Rotate(pBitmap, 180);
                case SKEncodedOrigin.BottomLeft:
                    return Flip(pBitmap, false, true);
                case SKEncodedOrigin.LeftTop:
                    using (var rotated = Rotate(pBitmap, 90))
                        return Flip(rotated, true, false);
                case SKEncodedOrigin.RightTop:
                    return Rotate(pBitmap, 90);
                case SKEncodedOrigin.RightBottom:
                    using (var rotated = Rotate(pBitmap, 270))
                        return Flip(rotated, true, false);
                case SKEncodedOrigin.LeftBottom:
                    return Rotate(pBitmap, 270);
                default:
                    return pBitmap;
            }
        }

        private static SKBitmap Rotate(SKBitmap pBitmap, int pDegrees)
        {
            bool swap = pDegrees == 90 || pDegrees == 270;
            int newWidth = swap ? pBitmap.Height : pBitmap.Width;
            int newHeight = swap ? pBitmap.Width : pBitmap.Height;

            var rotated = new SKBitmap(newWidth, newHeight);
            using (var canvas = new SKCanvas(rotated))
            {
                canvas.Translate(newWidth / 2f, newHeight / 2f);
                canvas.RotateDegrees(pDegrees);
                canvas.Translate(-pBitmap.Width / 2f, -pBitmap.Height / 2f);
                canvas.DrawBitmap(pBitmap, 0, 0, SKSamplingOptions.Default, null);
            }
            pBitmap.Dispose();
            return rotated;
        }

        private static SKBitmap Flip(SKBitmap pBitmap, bool pHorizontal, bool pVertical)
        {
            var flipped = new SKBitmap(pBitmap.Width, pBitmap.Height);
            using (var canvas = new SKCanvas(flipped))
            {
                canvas.Translate(pHorizontal ? pBitmap.Width : 0, pVertical ? pBitmap.Height : 0);
                canvas.Scale(pHorizontal ? -1 : 1, pVertical ? -1 : 1);
                canvas.DrawBitmap(pBitmap, 0, 0, SKSamplingOptions.Default, null);
            }
            pBitmap.Dispose();
            return flipped;
        }

        private static SKBitmap ResizeBitmap(SKBitmap pBitmap, int pMaxWidth, int pMaxHeight)
        {
            double resizeWidth = pBitmap.Width;
            double resizeHeight = pBitmap.Height;

            double aspect = resizeWidth / resizeHeight;

            if (resizeWidth > pMaxWidth)
            {
                resizeWidth = pMaxWidth;
                resizeHeight = resizeWidth / aspect;
            }
            if (resizeHeight > pMaxHeight)
            {
                aspect = resizeWidth / resizeHeight;
                resizeHeight = pMaxHeight;
                resizeWidth = resizeHeight * aspect;
            }

            int targetWidth = (int)Math.Round(resizeWidth);
            int targetHeight = (int)Math.Round(resizeHeight);

            var resized = pBitmap.Resize(new SKImageInfo(targetWidth, targetHeight), new SKSamplingOptions(SKCubicResampler.CatmullRom));
            pBitmap.Dispose();
            return resized;
        }
    }
}
