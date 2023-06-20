using Microsoft.AspNetCore.Components.Forms;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
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
            // var browserFile = await pBrowerFile.RequestImageFileAsync(DEFAULT_CONTENT_TYPE, 1000, 1000);

            Image img;

            try
            {
                img = await Image.LoadAsync(pBrowerFile.OpenReadStream(Constants.MAX_FILE_SIZE));
            }
            catch
            {
                return null;
            }

            if (img == null)
                return null;

            if (img.Width > 1000 || img.Height > 1000)
            {
                img = ResizeBitmap(img, 1000, 1000);
            }

            using var ms = new MemoryStream();
            await img.SaveAsJpegAsync(ms, new JpegEncoder()
            {
                Quality = 80
            });

            ms.Seek(0, SeekOrigin.Begin);

            return ms.ToArray();
        }

        private static Image ResizeBitmap(Image pBitmap, int pMaxWidth, int pMaxHeight)
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

            pBitmap.Mutate(x => x.Resize((int)resizeWidth, (int)resizeHeight));

            return pBitmap;
        }
    }
}
