﻿namespace Kontur.ImageTransformer.Filters
{
    class RotateCcw : AbstractFilter
    {
        public unsafe override void Transform()
        {
            byte* pointer1;
            byte* pointer2;
            int num1 = SourceImageData.Stride - rect.Top*4-4;
            for (int h = 0; h < rect.Height; h++)
            {
                pointer1 = (byte*)SourceImageData.Scan0 + rect.Left * SourceImageData.Stride + num1 - 4 * h;
                pointer2 = (byte*)DestinationImageData.Scan0 + h * DestinationImageData.Stride;
                for (int w = 0; w < rect.Width; w++)
                {
                    *pointer2 = *(pointer1); pointer2++;
                    *pointer2 = *(pointer1 + 1); pointer2++;
                    *pointer2 = *(pointer1 + 2); pointer2++;
                    *pointer2 = *(pointer1 + 3); pointer2++; pointer1 += SourceImageData.Stride;
                }
            }
        }
    }
}
