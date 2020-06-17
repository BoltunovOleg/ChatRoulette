﻿using System;
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
        private readonly SessionPreference _sessionPreference;
        private readonly Logger _logger;
        private bool _isFirstLoading = true;
        private ChromiumWebBrowser _browser;
        private bool _browserBanState;
        private Status _status;

        public BrowserController(SessionPreference sessionPreference, Logger logger)
        {
            this._sessionPreference = sessionPreference;
            this._logger = logger;

            if (!Cef.IsInitialized)
            {
                var cefSettings = new CefSettings
                {
                    BrowserSubprocessPath = Path.Combine(
                        AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
                        Environment.Is64BitProcess ? "x64" : "x86",
                        "CefSharp.BrowserSubprocess.exe")
                };
                cefSettings.CefCommandLineArgs.Add("enable-media-stream", "1");
                if (File.Exists(Path.Combine(Environment.CurrentDirectory, "video.y4m")))
                {
                    cefSettings.CefCommandLineArgs.Add("use-fake-device-for-media-stream");
                    cefSettings.CefCommandLineArgs.Add("--use-file-for-fake-video-capture", "video.y4m");
                }

                Cef.Initialize(cefSettings, performDependencyCheck: false, browserProcessHandler: null);
            }

            if (this._sessionPreference.Mod != "-1")
            {
                var m = this._sessionPreference.Mod;
                if (this._sessionPreference.Mod != "0")
                {
                    if (this._sessionPreference.WithBan)
                    {
                        m = "0";
                    }
                    else
                    {
                        Cef.GetGlobalCookieManager().SetCookie("https://chatroulette.com",
                            new Cookie() {Path = "/", Domain = "chatroulette.com", Name = "counter", Value = this._sessionPreference.Mod });
                        m = "-100";
                    }
                }

                Cef.GetGlobalCookieManager().SetCookie("https://chatroulette.com",
                    new Cookie() {Path = "/", Domain = "chatroulette.com", Name = "mod", Value = m});
            }

            this._browser = new ChromiumWebBrowser("https://chatroulette.com");
            this._browser.ConsoleMessage +=
                (sender, args) =>
                {
                    if (args.Message == "Couldn't fetch stats due to GetStats is not possible: OT.Subscriber is not connected cannot getStats")
                        return;
                    this._logger.Trace($"Browser console message: {args.Message}");
                    if (args.Message.Contains("partner banned by moderator"))
                        this.BrowserBanState = true;
                    if (args.Message.Contains("Stream started"))
                        this.BrowserBanState = false;

                    if (args.Message.Contains("Search started."))
                        this.Status = Status.Wait;
                    if (args.Message.Contains("Client is now ready to begin."))
                        this.Status = Status.EnableCamera;
                    if (args.Message.Contains("Setup publisher and camera turned on"))
                        this.Status = Status.Start;
                    if (args.Message.Contains("Setup subscriber - success"))
                        this.Status = Status.PartnerConnected;
                    if (args.Message.Contains("partner skipped") || args.Message.Contains("partner banned by moderator"))
                        this.Status = Status.PutResult;
                };

            this._browser.LoadingStateChanged += this.ChromeBrowserOnLoadingStateChanged;
        }

        public Task RefreshPage()
        {
            this._isFirstLoading = true;
            this._browser.Reload();
            this._logger.Info("Page reloaded successfully");
            return Task.CompletedTask;
        }

        public BitmapEncoder GetBrowserScreenShot(BitmapEncoder encoder)
        {
            this.Browser.Dispatcher.Invoke(() =>
            {
                var renderTargetBitmap = new RenderTargetBitmap(Convert.ToInt32(this._browser.ActualWidth),
                    Convert.ToInt32(this._browser.ActualHeight), 96, 96, PixelFormats.Pbgra32);
                renderTargetBitmap.Render(this._browser);
                encoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
            });
            return encoder;
        }

        public void NextPartner()
        {
            if (!this._browser.CanExecuteJavascriptInMainFrame)
                return;
            this._logger.Trace($"Start next button click");
            var script = "$('#next')[0].click()";

            this._browser.ExecuteScriptAsync(script);
            this._logger.Trace($"Next button clicked");
        }

        public void ReportPartner()
        {
            if (!this._browser.CanExecuteJavascriptInMainFrame)
                return;
            this._logger.Trace($"Start report button click");
            var script = "$('#report')[0].click()";

            this._browser.ExecuteScriptAsync(script);
            this._logger.Trace($"Report button clicked");
        }

        public void BanPartner()
        {
            if (!this._browser.CanExecuteJavascriptInMainFrame)
                return;
            this._logger.Trace($"Start ban button click");
            var script = "$('#ban')[0].click()";

            this._browser.ExecuteScriptAsync(script);
            this._logger.Trace($"Ban button clicked");
        }

        public void HidePartnerInfo()
        {
            if (!this._browser.CanExecuteJavascriptInMainFrame)
                return;
            var script = "$('#partner-info-container').hide();";
            this._browser.ExecuteScriptAsync(script);
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
                this.HidePartner();
            else
                this.ShowPartner();
        }

        public void ShowPartner()
        {
            if (!this._browser.CanExecuteJavascriptInMainFrame)
                return;
            var script = "$('.publisher-row').show();";
            this.Browser.ExecuteScriptAsync(script);
        }

        public void HidePartner()
        {
            if (!this._browser.CanExecuteJavascriptInMainFrame)
                return;
            var script = "$('.publisher-row').hide();";
            this._browser.ExecuteScriptAsync(script);
        }

        public async Task ToggleMyCamera()
        {
            var isShowed = await this.IsMyCameraShowed();
            if (isShowed)
                this.HideMyCamera();
            else
                this.ShowMyCamera();
        }

        public void ShowMyCamera()
        {
            if (!this._browser.CanExecuteJavascriptInMainFrame)
                return;
            var script = "$('.subscriber-row').show();";
            this._browser.ExecuteScriptAsync(script);
        }

        public void HideMyCamera()
        {
            if (!this._browser.CanExecuteJavascriptInMainFrame)
                return;
            var script = "$('.subscriber-row').hide();";
            this._browser.ExecuteScriptAsync(script);
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

        public ChromiumWebBrowser Browser
        {
            get => this._browser;
            set
            {
                this._browser = value;
                this.OnPropertyChanged();
            }
        }

        public bool BrowserBanState
        {
            get => this._browserBanState;
            set
            {
                this._browserBanState = value;
                this.OnPropertyChanged();
            }
        }
        public Status Status
        {
            get => this._status;
            set
            {
                if (this._status == value)
                    return;
                this._status = value;
                this.OnPropertyChanged();
            }
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