using System;
using System.Windows;
using SharpAvi;
using SharpAvi.Codecs;
using SharpAvi.Output;

namespace ChatRoulette.Core.Capture.DesktopVideo
{
    public enum RecorderCodecs
    {
        Uncompressed,
        MotionJpeg,
        MicrosoftMpeg4V3,
        MicrosoftMpeg4V2,
        Xvid,
        DivX,
        X264
    }

    public class RecorderParams
    {
        public RecorderParams(string filename, int frameRate, RecorderCodecs codec, int quality)
        {
            this._fileName = filename;
            this.FramesPerSecond = frameRate;
            switch (codec)
            {
                case RecorderCodecs.Uncompressed:
                    this._codec = KnownFourCCs.Codecs.Uncompressed;
                    break;
                case RecorderCodecs.MotionJpeg:
                    this._codec = KnownFourCCs.Codecs.MotionJpeg;
                    break;
                case RecorderCodecs.MicrosoftMpeg4V3:
                    this._codec = KnownFourCCs.Codecs.MicrosoftMpeg4V3;
                    break;
                case RecorderCodecs.MicrosoftMpeg4V2:
                    this._codec = KnownFourCCs.Codecs.MicrosoftMpeg4V2;
                    break;
                case RecorderCodecs.Xvid:
                    this._codec = KnownFourCCs.Codecs.Xvid;
                    break;
                case RecorderCodecs.DivX:
                    this._codec = KnownFourCCs.Codecs.DivX;
                    break;
                case RecorderCodecs.X264:
                    this._codec = KnownFourCCs.Codecs.X264;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(codec), codec, null);
            }
            this.Quality = quality;

            this.Height = Convert.ToInt32(SystemParameters.PrimaryScreenHeight);
            this.Width = Convert.ToInt32(SystemParameters.PrimaryScreenWidth);
        }

        private readonly string _fileName;
        public int FramesPerSecond, Quality;
        private readonly FourCC _codec;

        public int Height { get; }
        public int Width { get; }

        public AviWriter CreateAviWriter()
        {
            return new AviWriter(this._fileName)
            {
                FramesPerSecond = this.FramesPerSecond,
                EmitIndex1 = true,
            };
        }

        public IAviVideoStream CreateVideoStream(AviWriter writer)
        {
            if (this._codec == KnownFourCCs.Codecs.Uncompressed)
                return writer.AddUncompressedVideoStream(this.Width, this.Height);
            else if (this._codec == KnownFourCCs.Codecs.MotionJpeg)
                return writer.AddMotionJpegVideoStream(this.Width, this.Height, this.Quality);
            else
            {
                var codecs = Mpeg4VideoEncoderVcm.GetAvailableCodecs();
                foreach (var codecInfo in codecs)
                {
                    Console.WriteLine(codecInfo.Name);
                }
                return writer.AddMpeg4VideoStream(this.Width, this.Height, (double) writer.FramesPerSecond,
                    quality: this.Quality,
                    codec: this._codec,
                    forceSingleThreadedAccess: true);
            }
        }
    }
}