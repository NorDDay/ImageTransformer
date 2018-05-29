using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Kontur.ImageTransformer
{
    class AsyncHttpServer:IDisposable
    {
        private Thread checkCpuThread;
        private readonly HttpListener listener;
        private bool disposed;
        private volatile bool isRunning;
        private const int countListeners = 10;
        private int countQuery = 0;
        private int activeQuery = 0;
        private long curTime = 0;
        private bool getQuery = true;

        public AsyncHttpServer()
        {
            listener = new HttpListener();
        }

        public void Start(string prefix)
        {
            lock (listener)
            {
                if (!isRunning)
                {
                    listener.Prefixes.Clear();
                    listener.Prefixes.Add(prefix);
                    listener.Start();

                    checkCpuThread = new Thread(СheckCpu)
                    {
                        IsBackground = true,
                    };
                    checkCpuThread.Start();

                    for (int i = 0; i < countListeners; i++)
                        BeginGetContext();
                    isRunning = true;
                }
            }
        }

        private void BeginGetContext()
        {
            listener.BeginGetContext(new AsyncCallback(EndGetContext), listener);
        }

        private void EndGetContext(IAsyncResult result)
        {
            activeQuery++;
            Stopwatch t = new Stopwatch();
            t.Start();
            HttpListenerContext context = ((HttpListener)result.AsyncState).EndGetContext(result);
            if (getQuery)
            {
                Task.Factory.StartNew(() =>
                {
                    using (ImageTransformer image = new ImageTransformer(context))
                    {
                        if (image.Validate())
                            image.Transform();
                    }
                    t.Stop();
                    curTime += t.ElapsedMilliseconds;
                    countQuery++;
                    activeQuery--;
                    Thread.Sleep(0);
                });
            }
            else
            {
                context.Response.StatusCode = 409;
                context.Response.Close();
                activeQuery--;
            }
            BeginGetContext();
        }

        private void СheckCpu()
        {
            int maxQuery = 100000;
            while (true)
            {
                long t = 0;
                if (countQuery != 0)
                    t = curTime / countQuery;
                if (80 < t)
                {
                    if (maxQuery == 100000)
                        maxQuery = activeQuery - 2;
                    else maxQuery -= 2;
                    getQuery = false;
                }
                else if (activeQuery > maxQuery)
                {
                    getQuery = false;
                    maxQuery += 1;
                }
                else
                    getQuery = true;
                
                curTime = 0;
                countQuery = 0;
                Thread.Sleep(10);
            }
        }

        public void Stop()
        {
            lock (listener)
            {
                if (!isRunning)
                    return;
                listener.Stop();
                isRunning = false;
            }
        }

        public void Dispose()
        {
            if (disposed)
                return;
            disposed = true;
            Stop();
            listener.Close();
        }
    }
}
