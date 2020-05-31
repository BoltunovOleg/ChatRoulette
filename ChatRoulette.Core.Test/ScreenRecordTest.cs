using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using ChatRoulette.Core.Capture.DesktopVideo;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChatRoulette.Core.Test
{
    [TestClass]
    public class ScreenRecordTest
    {
        [TestMethod]
        public void TestXvid()
        {
            this.Test(RecorderCodecs.Xvid);
        }

        private void Test(RecorderCodecs codec)
        {
            var duration = TimeSpan.FromSeconds(10);
            var fileName = Path.Combine(Environment.CurrentDirectory, codec + ".avi");
            
            Debug.WriteLine("Начинаем запись экрана");
            var recorder = new Recorder(new RecorderParams(fileName, 6, codec, 50));

            Debug.WriteLine($"Пауза на {duration}");
            Thread.Sleep(60000);

            Debug.WriteLine("Завершаем запись экрана");
            recorder.Dispose();
            Thread.Sleep(500);
            Debug.WriteLine($"Путь к файлу: {fileName}");

            Debug.WriteLine("Проверяем файл");
            var fileInfo = new FileInfo(fileName);

            var bytes = fileInfo.Length;
            var kBytes = bytes / 1024;
            var mBytes = kBytes / 1024;
            Debug.WriteLine($"Размер файла: {mBytes}MB / {kBytes}KB");
        }
    }
}
