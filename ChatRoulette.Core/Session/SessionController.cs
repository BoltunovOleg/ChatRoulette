using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Input;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ChatRoulette.Core.Capture.DesktopVideo;
using ChatRoulette.Core.Utils;
using ChatRoulette.Repository;
using ChatRoulette.Repository.Model;
using Newtonsoft.Json;
using NLog;
using NLog.Targets;

namespace ChatRoulette.Core.Session
{
    public class SessionController : INotifyPropertyChanged
    {
        public List<ChatConnectionInfo> ChatConnectionInfos { get; } =
            new List<ChatConnectionInfo>();

        public List<ChatConnection> ChatConnections { get; } =
            new List<ChatConnection>();

        private readonly ChatRepository _repository;
        private readonly SessionPreference _sessionPreference;
        private readonly ChatSession _session;
        private readonly Logger _logger;
        private readonly Action<object> _bugtrackerReport;
        private TimeSpan _sessionLeftTime;

        private ChatConnectionInfo _currentConnectionInfo;
        private Recorder _recorder;
        private BrowserController _browserController;
        private string _ip;
        private bool _banState;
        private bool _browserBanState;
        private bool _eventProcessingStarted;

        public SessionController(ChatRepository repository, SessionPreference sessionPreference, ChatSession session,
            Logger logger, Action<object> bugtrackerReport)
        {
            this.BrowserController = new BrowserController(sessionPreference.Mod, logger);
            this.BrowserController.PropertyChanged += this.BrowserControllerOnPropertyChanged;
            this._repository = repository;
            this._sessionPreference = sessionPreference;
            this._session = session;
            this._logger = logger;
            this._bugtrackerReport = bugtrackerReport;

            var sessionThread = new Thread(this.SessionTick)
            {
                Name = $"Session #{session.Id}",
                IsBackground = true
            };
            sessionThread.Start();
        }

        private void BrowserControllerOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.BrowserController.BrowserBanState))
                this.BrowserBanState = this.BrowserController.BrowserBanState;
            switch (e.PropertyName)
            {
                case nameof(this.BrowserController.BrowserBanState):
                    break;
                case nameof(this.BrowserController.Status):
                    this.UpdateConnectionInfo(this.BrowserController.Status);
                    this.UpdateBrowserView(this.BrowserController.Status);
                    break;
            }
        }

        private void SessionTick()
        {
            this.StartRecord();

            var currentSessionPath = this.GetCurrentSessionPath();
            if (LogManager.Configuration.FindTargetByName("logfile") is FileTarget target)
            {
                target.FileName = Path.Combine(currentSessionPath, "log.data");
                LogManager.ReconfigExistingLoggers();
            }

            var json = JsonConvert.SerializeObject(this._sessionPreference);
            this.Ip = InetUtils.GetMyIp();

            this._logger.Info("Session started");
            this._logger.Trace($"User public IP: {this.Ip}");
            this._logger.Trace($"Session preferences:{Environment.NewLine}{json}");

            while (true)
            {
                Thread.Sleep(500);
                try
                {
                    var sessionDuration = DateTime.Now - this._session.DateCreated;
                    this.SessionLeftTime = this._sessionPreference.WorkTime - sessionDuration;
                    if (sessionDuration > this._sessionPreference.WorkTime)
                    {
                        this.StopSession();
                        return;
                    }
                }
                catch (Exception ex)
                {
                    this._bugtrackerReport(ex);
                    LogManager.GetCurrentClassLogger()
                        .Error($"Unhandled SessionTick Exception {Environment.NewLine}{ex}");
                }
            }
        }

        private void UpdateConnectionInfo(Status status)
        {
            if (status == Status.PartnerConnected)
            {
                var id = this.ChatConnectionInfos.Count + 1;
                this.CurrentConnectionInfo = new ChatConnectionInfo(id);
                this.ChatConnectionInfos.Add(this.CurrentConnectionInfo);
                this.OnPropertyChanged(nameof(this.ChatConnectionInfos));
                this.BanState = false;
            }
            else
            {
                if (this.CurrentConnectionInfo != null && !this.CurrentConnectionInfo.Handled)
                {
                    this.CurrentConnectionInfo.DateEnd = DateTime.Now;
                }
            }
        }

        private void UpdateBrowserView(Status status)
        {
            switch (status)
            {
                case Status.EnableCamera:
                case Status.Start:
                case Status.Wait:
                    break;
                case Status.PartnerConnected:
                    if (!this.EventProcessingStarted)
                    {
                        this.BrowserController.HidePartnerInfo();
                        this.BrowserController.ShowPartner();
                    }

                    break;
                default:
                    this.BrowserController.HidePartner();
                    this.EventProcessingStarted = false;
                    break;
            }
        }

        public async Task<ChatConnectionResultEnum?> KeyDown(Key key)
        {
            if (!this.EventProcessingStarted)
                this.EventProcessingStarted = true;
            var swGlobal = Stopwatch.StartNew();
            var swLocal = Stopwatch.StartNew();
            this._logger.Trace($"Event KeyDown received: {key}");

            if (key == Key.F5)
            {
                this._logger.Trace($"Start page reload");
                await this.BrowserController.RefreshPage();
                this._logger.Trace($"Page reloaded in {swLocal.Elapsed}");
                this.EventProcessingStarted = false;
                return null;
            }

            if (this.BrowserController.Status != Status.PartnerConnected)
            {
                this.EventProcessingStarted = false;
                return null;
            }

            if (!this._sessionPreference.KeyToResultBinds.ContainsKey(key))
            {
                this.EventProcessingStarted = false;
                return null;
            }

            if (this.CurrentConnectionInfo.Handled)
            {
                this.EventProcessingStarted = false;
                return null;
            }

            this.CurrentConnectionInfo.DateEnd = DateTime.Now;
            this.CurrentConnectionInfo.Handled = true;
            var result = this._sessionPreference.KeyToResultBinds[key];

            this._logger.Trace($"{result} assigned to connection in {swLocal.Elapsed}");
            swLocal.Restart();

            this.MakeScreenShoot(result);
            this._logger.Trace($"Screenshoot taked in {swLocal.Elapsed}");
            swLocal.Restart();

            var action = "skipped";

            if (this._sessionPreference.WithBan && (result == ChatConnectionResultEnum.Inappropriate ||
                                                    result == ChatConnectionResultEnum.HiddenInappropriate))
            {
                this.BanState = true;
                action = "banned";
                this.BrowserController.BanPartner();
            }
            else
            {
                if (this._sessionPreference.WithReport && (result == ChatConnectionResultEnum.Spam1 ||
                                                           result == ChatConnectionResultEnum.Spam2 ||
                                                           result == ChatConnectionResultEnum.Spam3))
                {
                    action = "reported";
                        this.BrowserController.ReportPartner();
                        this.BrowserController.NextPartner();
                }
                else
                {
                    
                    this.BrowserController.NextPartner();
                }
            }

            this._logger.Trace($"Partner {action} in {swLocal.Elapsed}");
            swLocal.Restart();

            await Task.Run(() =>
            {
                var res = this._repository.AddResultAsync(this._session, result, "").GetAwaiter().GetResult();
                this.ChatConnections.Add(res);
                this.OnPropertyChanged(nameof(this.ChatConnections));
                this._logger.Trace($"Result saved in {swLocal.Elapsed}");
                swLocal.Restart();
            });

            swGlobal.Stop();
            this._logger.Info($"Event KeyDown processed in {swGlobal.Elapsed}");
            return result;
        }

        private void MakeScreenShoot(ChatConnectionResultEnum folderType)
        {
            var image = this.BrowserController.GetBrowserScreenShot(new JpegBitmapEncoder());
            var path = this.GetCurrentSessionPathWithResult(folderType.ToString());
            var fileName = Path.Combine(path, $"{this.ChatConnections.Count(x => x.Result == folderType) + 1}.jpg");

            using (Stream fileStream = File.Create(fileName))
            {
                image.Save(fileStream);
            }
        }

        private void StopSession()
        {
            try
            {
                if (LogManager.Configuration.FindTargetByName("logfile") is FileTarget target)
                {
                    target.FileName = Path.Combine(Environment.CurrentDirectory, "log.data");
                    LogManager.ReconfigExistingLoggers();
                }

                this.BrowserController.Stop();
                this.StopRecord();
                var session = this._repository.CloseSessionAsync(this._session).GetAwaiter().GetResult();
                GoogleReport.Report(this._sessionPreference, session, this._session.UserNumber);
                this.OnSessionEnd();
            }
            catch (Exception ex)
            {
                this._bugtrackerReport(ex);
                LogManager.GetCurrentClassLogger().Error($"Unhandled StopSession {Environment.NewLine}{ex}");
            }
        }

        private string GetCurrentSessionPathWithResult(string folderType)
        {
            var path = this.GetCurrentSessionPath();
            path = Path.Combine(path, folderType);
            new DirectoryInfo(path).Create();
            return path;
        }

        private string GetCurrentSessionPath()
        {
            var path = Path.Combine(Environment.CurrentDirectory, "sessions",
                this._session.DateStart.ToString("dd.MM.yyyy HH.mm"));
            new DirectoryInfo(path).Create();
            return path;
        }

        private void StartRecord()
        {
            var path = this.GetCurrentSessionPath();
            var fileName = "";
            for (var i = 0; i < 100; i++)
            {
                fileName = Path.Combine(path, $"screenRecord_{i}.avi");
                if (!File.Exists(fileName))
                    break;
            }

            this._recorder = new Recorder(new RecorderParams(fileName, 6, RecorderCodecs.Xvid, 50));
        }

        private void StopRecord()
        {
            this._recorder?.Dispose();
        }

        public ChatConnectionInfo CurrentConnectionInfo
        {
            get => this._currentConnectionInfo;
            set
            {
                this._currentConnectionInfo = value;
                this.OnPropertyChanged();
            }
        }

        public TimeSpan SessionLeftTime
        {
            get => this._sessionLeftTime;
            set
            {
                this._sessionLeftTime = value;
                this.OnPropertyChanged();
            }
        }

        public BrowserController BrowserController
        {
            get => this._browserController;
            set
            {
                this._browserController = value;
                this.OnPropertyChanged();
            }
        }

        public string Ip
        {
            get => this._ip;
            set
            {
                this._ip = value;
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

        public bool BanState
        {
            get => this._banState;
            set
            {
                this._banState = value;
                this.OnPropertyChanged();
            }
        }

        public bool EventProcessingStarted
        {
            get => this._eventProcessingStarted;
            set
            {
                this._eventProcessingStarted = value;
                this.OnPropertyChanged();
            }
        }

        public ChatSession Session => this._session;

        public event PropertyChangedEventHandler PropertyChanged;
        public event SessionEndEventHandler SessionEnd;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnSessionEnd()
        {
            this.SessionEnd?.Invoke(this, this._session);
        }
    }
}