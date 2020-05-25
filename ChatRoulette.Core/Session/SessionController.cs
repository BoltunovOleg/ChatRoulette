using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Input;
using System.Linq;
using System.Threading.Tasks;
using ChatRoulette.Core.Capture.DesktopVideo;
using ChatRoulette.Repository;
using ChatRoulette.Repository.Model;
using NLog;

namespace ChatRoulette.Core.Session
{
    public class SessionController : INotifyPropertyChanged
    {
        public ObservableCollection<ChatConnectionInfo> ChatConnectionInfos { get; } =
            new ObservableCollection<ChatConnectionInfo>();

        public ObservableCollection<ChatConnection> ChatConnections { get; }

        private readonly ChatRepository _repository;
        private readonly SessionPreference _sessionPreference;
        private readonly ChatSession _session;

        public BrowserController BrowserController
        {
            get => this._browserController;
            set
            {
                this._browserController = value;
                this.OnPropertyChanged();
            }
        }

        private TimeSpan _sessionLeftTime;
        private Status _status;
        private ChatConnectionInfo _currentConnectionInfo;
        private Recorder _recorder;
        private BrowserController _browserController;

        public SessionController(ChatRepository repository, SessionPreference sessionPreference, ChatSession session)
        {
            this.BrowserController = new BrowserController(sessionPreference.Mod);
            this._repository = repository;
            this._sessionPreference = sessionPreference;
            this._session = session;
            this.ChatConnections = new ObservableCollection<ChatConnection>(this._session.ChatConnections);
            for (var i = 0; i < this.ChatConnections.Count; i++)
                this.ChatConnectionInfos.Add(new ChatConnectionInfo(i + 1));
            var sessionThread = new Thread(this.SessionTick)
            {
                Name = $"Session #{session.Id}",
                IsBackground = true
            };
            sessionThread.Start();
        }

        private void SessionTick()
        {
            this.StartRecord();
            var statusText = "";
            while (true)
            {
                Thread.Sleep(100);
                try
                {
                    var sessionDuration = DateTime.Now - this._session.DateCreated;
                    this.SessionLeftTime = this._sessionPreference.WorkTime - sessionDuration;
                    if (sessionDuration > this._sessionPreference.WorkTime)
                    {
                        this.StopSession();
                        return;
                    }

                    if (this.UpdateStatus(ref statusText))
                    {
                        this.UpdateConnectionInfo(this.Status);
                        this.UpdateBrowserView(this.Status);
                    }
                }
                catch (Exception ex)
                {
                    LogManager.GetCurrentClassLogger().Error($"Unhandled SessionTick Exception {Environment.NewLine}{ex}");
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
                case Status.PartnerConnected:
                    this.BrowserController.HidePartnerInfo().GetAwaiter().GetResult();
                    this.BrowserController.ShowPartner().GetAwaiter().GetResult();
                    break;
                default:
                    this.BrowserController.HidePartner().GetAwaiter().GetResult();
                    break;
            }
        }

        private bool UpdateStatus(ref string statusText)
        {
            var newStatusText = this.BrowserController.GetStatus().GetAwaiter().GetResult();

            if (newStatusText != null && statusText != newStatusText)
            {
                statusText = newStatusText;
                this.Status = ChatRouletteStatusParser.Parse(statusText);
                return true;
            }

            return false;
        }

        public async Task<ChatConnectionResultEnum?> KeyDown(Key key)
        {
            if (key == Key.F5)
            {
                await this.BrowserController.RefreshPage();
                return null;
            }

            if (this.Status != Status.PartnerConnected)
                return null;

            if (!this._sessionPreference.KeyToResultBinds.ContainsKey(key))
                return null;
            if (this.CurrentConnectionInfo.Handled)
                return null;

            this.CurrentConnectionInfo.DateEnd = DateTime.Now;
            this.CurrentConnectionInfo.Handled = true;

            var result = this._sessionPreference.KeyToResultBinds[key];
            this.MakeScreenShoot(result);
            var res = await this._repository.AddResultAsync(this._session, result, "");
            this.ChatConnections.Add(res);
            this.OnPropertyChanged(nameof(this.ChatConnections));

            if (this._sessionPreference.WithBan && (result == ChatConnectionResultEnum.Inappropriate ||
                                                    result == ChatConnectionResultEnum.HiddenInappropriate))
            {
                await this.BrowserController.BanPartner();
                return result;
            }

            if (this._sessionPreference.WithReport && (result == ChatConnectionResultEnum.Spam1 ||
                                                       result == ChatConnectionResultEnum.Spam2 ||
                                                       result == ChatConnectionResultEnum.Spam3))
            {
                await this.BrowserController.ReportPartner();
                return result;
            }

            await this.BrowserController.NextPartner();
            return result;
        }

        private void MakeScreenShoot(ChatConnectionResultEnum folderType)
        {
            var pngImage = this.BrowserController.GetBrowserScreenShot();
            var path = this.GetCurrentSessionPathWithResult(folderType.ToString());
            var fileName = Path.Combine(path, $"{this.ChatConnections.Count(x => x.Result == folderType) + 1}.png");

            using (Stream fileStream = File.Create(fileName))
            {
                pngImage.Save(fileStream);
            }
        }

        private void StopSession()
        {
            try
            {
                this.BrowserController.Stop();
                this.StopRecord();
                var session = this._repository.CloseSessionAsync(this._session).GetAwaiter().GetResult();
                GoogleReport.Report(this._sessionPreference, session, this._session.UserNumber);
                this.OnSessionEnd();
            }
            catch (Exception ex)
            {
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
            var fileName = Path.Combine(path, "screenRecord.avi");
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

        public Status Status
        {
            get => this._status;
            set
            {
                this._status = value;
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