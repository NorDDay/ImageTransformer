using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace Kontur.ImageTransformer
{
    abstract class AbstractFilter: IDisposable
    {
        protected Rectangle rect;
        protected Bitmap SourceImage;
        protected Bitmap DestinationImage;
        protected BitmapData SourceImageData;
        protected BitmapData DestinationImageData;

        public unsafe Bitmap Apply(Bitmap img, Rectangle rect)
        {
            this.rect = rect;
            SourceImage = img;
            SourceImageData = img.LockBits(
                new Rectangle(0, 0, img.Width, img.Height),
                ImageLockMode.ReadOnly,
                img.PixelFormat
                );

            DestinationImage = new Bitmap(rect.Width, rect.Height, img.PixelFormat);
            DestinationImageData = DestinationImage.LockBits(
                new Rectangle(0, 0, rect.Width, rect.Height),
                ImageLockMode.ReadWrite,
                img.PixelFormat
                );
            Transform();
            EndWork();
            return DestinationImage;
        }
        private void EndWork()
        {
            SourceImage.UnlockBits(SourceImageData);
            DestinationImage.UnlockBits(DestinationImageData);
        }
        public abstract void Transform();

        public void Dispose()
        {
            return;
        }
    }
}
