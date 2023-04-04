using Microsoft.AspNetCore.Components.Forms;
using System;
using System.IO;
using System.Threading;

namespace MComponents.Images.MImageEditor
{
    internal class MUploadedImage : IBrowserFile
    {
        public string Name { get; set; }

        public DateTimeOffset LastModified { get; set; }

        public long Size { get; set; }

        public string ContentType { get; set; }

        protected byte[] mData;

        public MUploadedImage(string pName, string pContentType, byte[] pData)
        {
            Name = pName;
            ContentType = pContentType;
            mData = pData;
            Size = pData.Length;
            LastModified = DateTime.UtcNow;
        }

        public Stream OpenReadStream(long maxAllowedSize = 512000, CancellationToken cancellationToken = default)
        {
            return new MemoryStream(mData, false);
        }
    }
}
