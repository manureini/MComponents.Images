using Microsoft.AspNetCore.Components.Forms;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MComponents.Images.Helper
{
    internal static class BrowserFileHelper
    {
        public const string DEFAULT_CONTENT_TYPE = "image/jpg";

        public static async Task<byte[]> ResizeAndGetImage(IBrowserFile pBrowerFile)
        {
            // var browserFile = await pBrowerFile.RequestImageFileAsync(DEFAULT_CONTENT_TYPE, 1000, 1000);

            var img = await Image.LoadAsync(pBrowerFile.OpenReadStream(Constants.MAX_FILE_SIZE));

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
            int width = Math.Min(pMaxWidth, pBitmap.Width);
            int height = Math.Min(pMaxHeight, pBitmap.Height);

            pBitmap.Mutate(x => x.Resize(width, height));

            return pBitmap;
        }
    }
}
