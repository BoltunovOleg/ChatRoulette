using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CefSharp;
using CefSharp.Wpf;
using NLog;

namespace ChatRoulette.Core.Session
{
    public class BrowserController : INotifyPropertyChanged
    {
        public ChromiumWebBrowser Browser
        {
            get => this._browser;
            set
            {
                this._browser = value;
                this.OnPropertyChanged();
            }
        }

        private bool _isFirstLoading = true;
        private ChromiumWebBrowser _browser;

        public BrowserController(string mod, Logger logger)
        {
            if (!Cef.IsInitialized)
            {
                var cefSettings = new CefSettings();
                cefSettings.BrowserSubprocessPath = Path.Combine(
                    AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
                    Environment.Is64BitProcess ? "x64" : "x86",
                    "CefSharp.BrowserSubprocess.exe");
                cefSettings.CefCommandLineArgs.Add("enable-media-stream", "1");
                if (File.Exists(Path.Combine(Environment.CurrentDirectory, "video.y4m")))
                {
                    cefSettings.CefCommandLineArgs.Add("use-fake-device-for-media-stream");
                    cefSettings.CefCommandLineArgs.Add("--use-file-for-fake-video-capture", "video.y4m");
                }

                Cef.Initialize(cefSettings, performDependencyCheck: false, browserProcessHandler: null);
            }

            var m = mod;
            if (mod != "0")
            {
                Cef.GetGlobalCookieManager().SetCookie("https://chatroulette.com",
                    new Cookie() { Path = "/", Domain = "chatroulette.com", Name = "counter", Value = mod });
                m = "-100";
            }

            Cef.GetGlobalCookieManager().SetCookie("https://chatroulette.com",
                new Cookie() {Path = "/", Domain = "chatroulette.com", Name = "mod", Value = m});

            this._browser = new ChromiumWebBrowser("https://chatroulette.com");

            this._browser.LoadingStateChanged += this.ChromeBrowserOnLoadingStateChanged;
        }

        public Task RefreshPage()
        {
            this._isFirstLoading = true;
            this._browser.Reload();
            return Task.CompletedTask;
        }

        public PngBitmapEncoder GetBrowserScreenShot()
        {
            var renderTargetBitmap = new RenderTargetBitmap(Convert.ToInt32(this._browser.ActualWidth),
                Convert.ToInt32(this._browser.ActualHeight), 96, 96, PixelFormats.Pbgra32);
            renderTargetBitmap.Render(this._browser);
            var pngImage = new PngBitmapEncoder();
            pngImage.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
            return pngImage;
        }

        public async Task<string> NextPartner()
        {
            if (!this._browser.CanExecuteJavascriptInMainFrame)
                return null;

            var script = "$('#next')[0].click()";
            var execResponse = await this._browser.EvaluateScriptAsync(script);
            if (execResponse.Success)
            {
                var result = Convert.ToString(execResponse.Result);
                return result;
            }

            return null;
        }

        public async Task<string> ReportPartner()
        {
            if (!this._browser.CanExecuteJavascriptInMainFrame)
                return null;

            var script = "$('#report')[0].click()";
            var execResponse = await this._browser.EvaluateScriptAsync(script);
            if (execResponse.Success)
            {
                var result = Convert.ToString(execResponse.Result);
                return result;
            }

            return null;
        }

        public async Task<string> BanPartner()
        {
            if (!this._browser.CanExecuteJavascriptInMainFrame)
                return null;

            var script = "$('#ban')[0].click()";
            var execResponse = await this._browser.EvaluateScriptAsync(script);
            if (execResponse.Success)
            {
                var result = Convert.ToString(execResponse.Result);
                return result;
            }

            return null;
        }

        public async Task HidePartnerInfo()
        {
            if (!this._browser.CanExecuteJavascriptInMainFrame)
                return;
            var script = "$('#partner-info-container').hide();";
            await this._browser.EvaluateScriptAsync(script);
        }

        public async Task PreparePage()
        {
            if (!this._browser.CanExecuteJavascriptInMainFrame)
                return;
            var script = "$(document.body).css({backgroundColor: '#303030'});" +
                         "$('.fa-row').hide();$('.footer-content').hide();" +
                         "$('.publisher-row').children().css({margin:0,padding:0,width:'100%',height:'100%'});" +
                         "$('#camera-subscriber-outer-container').css({height: '100%'});" +
                         "$('.row').css({padding: 0});" +
                         "$('.subscriber-row').css({padding: 0,margin: 0,position: 'absolute',zIndex: 9999,width: '270px',height: '260px',left: 0, bottom: 0});$('.subscriber-row').children().css({margin: 0});";
            await this._browser.EvaluateScriptAsync(script);
        }

        public async Task<string> GetStatus()
        {
            if (!this._browser.CanExecuteJavascriptInMainFrame)
                return null;
            var script = "$('#status')[0].innerHTML";
            var execResponse = await this._browser.EvaluateScriptAsync(script);
            if (execResponse.Success)
            {
                var result = Convert.ToString(execResponse.Result);
                return result;
            }
            else
            {
                return null;
            }
        }

        public async Task TogglePartner()
        {
            var isShowed = await this.IsPartnerShowed();
            if (isShowed)
                await this.HidePartner();
            else
                await this.ShowPartner();
        }

        public async Task ShowPartner()
        {
            if (!this._browser.CanExecuteJavascriptInMainFrame)
                return;
            var script = "$('.publisher-row').show();";
            await this._browser.EvaluateScriptAsync(script);
        }

        public async Task HidePartner()
        {
            if (!this._browser.CanExecuteJavascriptInMainFrame)
                return;
            var script = "$('.publisher-row').hide();";
            await this._browser.EvaluateScriptAsync(script);
        }

        public async Task ToggleMyCamera()
        {
            var isShowed = await this.IsMyCameraShowed();
            if (isShowed)
                await this.HideMyCamera();
            else
                await this.ShowMyCamera();
        }

        public async Task ShowMyCamera()
        {
            if (!this._browser.CanExecuteJavascriptInMainFrame)
                return;
            var script = "$('.subscriber-row').show();";
            await this._browser.EvaluateScriptAsync(script);
        }

        public async Task HideMyCamera()
        {
            if (!this._browser.CanExecuteJavascriptInMainFrame)
                return;
            var script = "$('.subscriber-row').hide();";
            await this._browser.EvaluateScriptAsync(script);
        }

        private async Task<bool> IsMyCameraShowed()
        {
            if (!this._browser.CanExecuteJavascriptInMainFrame)
                return false;
            var script = "$('.subscriber-row').css('display');";
            var execResponse = await this._browser.EvaluateScriptAsync(script);
            if (execResponse.Success)
            {
                var result = Convert.ToString(execResponse.Result);
                return result != "none";
            }
            else
            {
                return false;
            }
        }

        private async Task<bool> IsPartnerShowed()
        {
            if (!this._browser.CanExecuteJavascriptInMainFrame)
                return false;
            var script = "$('.publisher-row').css('display');";
            var execResponse = await this._browser.EvaluateScriptAsync(script);
            if (execResponse.Success)
            {
                var result = Convert.ToString(execResponse.Result);
                return result != "none";
            }
            else
            {
                return false;
            }
        }

        public void Stop()
        {
            if (this._browser.IsInitialized)
                this._browser.LoadHtml("<html><body>Closing session...</body></html>");
        }

        private async void ChromeBrowserOnLoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            if (!e.IsLoading && this._isFirstLoading)
            {
                this._isFirstLoading = false;
                await this.PreparePage();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}