using Kontur.ImageTransformer.Filters;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace Kontur.ImageTransformer
{
    class ImageTransformer : IDisposable
    {
        private HttpListenerContext context;
        private Rectangle rect;
        private Bitmap image;
        private const int MaxSize = 100 * 1024 * 8;
        private bool disposed;
        private FilterType Type;
        
        public enum FilterType
        {
            rotatCw,
            rotateCcw,
            flipV,
            flipH
        };

        public ImageTransformer(HttpListenerContext listenerContext)
        {
            context = listenerContext;
        }

        public void Dispose()
        {
            if (disposed)
                return;
            try
            {
                context.Response.Close();
            }
            catch(Exception e) { }
            disposed = true;
        }

        public bool Validate()
        {
            if (context.Request.ContentLength64 > MaxSize)
            {
                context.Response.StatusCode = 400;
                return false;
            }
            try
            {
                image = new Bitmap(context.Request.InputStream);
                if(image.Width * image.Height > 1000000)
                {
                    context.Response.StatusCode = 400;
                    return false;
                }
            }
            catch (Exception e)
            {
                context.Response.StatusCode = 400;
                return false;
            }
            if (!CheckUrl(context.Request.Url.ToString()))
            {
                context.Response.StatusCode = 400;
                return false;
            }
            if (rect.Width * rect.Height == 0)
            {
                context.Response.StatusCode = 204;
                return false;
            }
            return true;
        }

        private bool CheckUrl(string url)
        {
            string[] t = url.Split('/');
            bool flag = false;
            string[] transforms = new string[4] { "rotate-cw", "rotate-ccw", "flip-v", "flip-h" };

            if (!t[t.Length - 3].Trim().ToLower().Equals("process"))
                return false;

            for (int i = 0; i < transforms.Length; i++)
            {
                if (t[t.Length - 2].Trim().ToLower().Equals(transforms[i]))
                {
                    Type = (FilterType)i;
                    flag = true;
                }
            }
            if (!flag)
                return false;

            if (Regex.IsMatch(t[t.Length - 1], @"^[\d-]+,[\d-]+,[\d-]+,[\d-]+$"))
                InitCoords(t[t.Length - 1]);
            else
                return false;
            return true;
        }

        private void InitCoords(string coords)
        {
            string[] t = coords.Split(',');
            Point point = new Point(
                    Math.Max(0, int.Parse(t[0])),
                    Math.Max(0, int.Parse(t[1]))
                    );
            Size size;
            if (Type == FilterType.flipH || Type == FilterType.flipV)
            {
                size = new Size(
                     Math.Min(image.Width - point.X, Math.Min(0, int.Parse(t[0])) + int.Parse(t[2])),
                     Math.Min(image.Height - point.Y, Math.Min(0, int.Parse(t[1])) + int.Parse(t[3]))
                     );
            }
            else
            {
                size = new Size(
                     Math.Min(image.Height - point.X, Math.Min(0,int.Parse(t[0])) + int.Parse(t[2])),
                     Math.Min(image.Width - point.Y, Math.Min(0, int.Parse(t[1])) + int.Parse(t[3]))
                     ); 
            }
            rect = new Rectangle(point, size);
        }

        public void Transform()
        {
            Bitmap output = null;
            if (Type == FilterType.flipH)
            {
                FlipH result = new FlipH();
                output = result.Apply(image, rect);
                result.Dispose();
            }
            else if (Type == FilterType.flipV)
            {
                FlipV result = new FlipV();
                output = result.Apply(image, rect);
                result.Dispose();
            }
            else if (Type == FilterType.rotatCw)
            {
                RotateCw result = new RotateCw();
                output = result.Apply(image, rect);
                result.Dispose();
            }
            else if (Type == FilterType.rotateCcw)
            {
                RotateCcw result = new RotateCcw();
                output = result.Apply(image, rect);
                result.Dispose();
            }
            output.Save(context.Response.OutputStream, ImageFormat.Png);
        }
    }
}
