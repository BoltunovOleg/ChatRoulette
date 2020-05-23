using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using SharpAvi.Output;

namespace ChatRoulette.Core.Capture.DesktopVideo
{
    public class Recorder : IDisposable
    {
        private readonly AviWriter _writer;
        private readonly RecorderParams _params;
        private readonly IAviVideoStream _videoStream;
        private readonly Thread _screenThread;
        private readonly ManualResetEvent _stopThread = new ManualResetEvent(false);

        public Recorder(RecorderParams Params)
        {
            this._params = Params;

            this._writer = Params.CreateAviWriter();

            this._videoStream = Params.CreateVideoStream(this._writer);
            this._videoStream.Name = "DesktopCapture";

            this._screenThread = new Thread(this.RecordScreen)
            {
                Name = typeof(Recorder).Name + ".RecordScreen",
                IsBackground = true
            };

            this._screenThread.Start();
        }

        public void Dispose()
        {
            this._stopThread.Set();
            this._screenThread.Join();

            this._writer.Close();

            this._stopThread.Dispose();
        }

        private void RecordScreen()
        {
            var frameInterval = TimeSpan.FromSeconds(1 / (double)this._writer.FramesPerSecond);
            var buffer = new byte[this._params.Width * this._params.Height * 4];
            Task videoWriteTask = null;
            var timeTillNextFrame = TimeSpan.Zero;

            while (!this._stopThread.WaitOne(timeTillNextFrame))
            {
                var timestamp = DateTime.Now;

                this.ScreenShot(buffer);

                videoWriteTask?.Wait();

                videoWriteTask = this._videoStream.WriteFrameAsync(true, buffer, 0, buffer.Length);

                timeTillNextFrame = timestamp + frameInterval - DateTime.Now;
                if (timeTillNextFrame < TimeSpan.Zero)
                    timeTillNextFrame = TimeSpan.Zero;
            }

            videoWriteTask?.Wait();
        }

        public void ScreenShot(byte[] buffer)
        {
            using (var bmp = new Bitmap(this._params.Width, this._params.Height))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(Point.Empty, Point.Empty, new Size(this._params.Width, this._params.Height), CopyPixelOperation.SourceCopy);

                    g.Flush();

                    var bits = bmp.LockBits(new Rectangle(0, 0, this._params.Width, this._params.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
                    Marshal.Copy(bits.Scan0, buffer, 0, buffer.Length);
                    bmp.UnlockBits(bits);
                }
            }
        }
    }
}