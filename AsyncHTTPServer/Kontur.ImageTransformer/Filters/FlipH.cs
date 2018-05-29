namespace Kontur.ImageTransformer.Filters
{
    class FlipH : AbstractFilter
    {
        public unsafe override void Transform()
        {
            byte* pointer1;
            byte* pointer2;
            int num1 = SourceImageData.Width*4 - rect.Left*4;
            for (int h = rect.Top; h < rect.Height + rect.Top; h++)
            {
                pointer1 = (byte*)SourceImageData.Scan0 + h * SourceImageData.Stride + num1-4;
                pointer2 = (byte*)DestinationImageData.Scan0 + (h - rect.Top) * DestinationImageData.Stride;
                for (int w = rect.Left; w < rect.Width + rect.Left; w++)
                {
                    *pointer2 = *(pointer1); pointer2++;
                    *pointer2 = *(pointer1 + 1); pointer2++;
                    *pointer2 = *(pointer1 + 2); pointer2++;
                    *pointer2 = *(pointer1 + 3); pointer2++; pointer1 -= 4;
                }
            }
        }
    }
}
