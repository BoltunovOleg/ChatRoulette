using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChatRoulette.Repository;
using ChatRoulette.Repository.Model;

namespace ChatRoulette.ViewModel
{
    public class HomeViewModel : BaseViewModel
    {
        private readonly ChatRepository _repository;
        private List<ChatSession> _sessions = new List<ChatSession>();
        private bool _isLoading;
        private readonly BackgroundWorker _bwLoadingTimer = new BackgroundWorker();
        private readonly BackgroundWorker _bwUpdater = new BackgroundWorker();

        public override string MenuCaption { get; } = "Главная";

        public override bool IsLoading
        {
            get => this._isLoading;
            set
            {
                this.OnPropertyChanging();
                this._isLoading = value;
                this.OnPropertyChanged();
                if (value && !this._bwLoadingTimer.IsBusy)
                    this._bwLoadingTimer.RunWorkerAsync();
            }
        }

        public override bool ShowActionButton { get; set; } = true;
        public override bool RedirectInput { get; } = false;

        public List<ChatSession> Sessions
        {
            get => this._sessions;
            set
            {
                this.OnPropertyChanging();
                this._sessions = value;
                this.OnPropertyChanged();
            }
        }

        public HomeViewModel(ChatRepository repository)
        {
            this._repository = repository;
            this._bwLoadingTimer.DoWork += this.BwLoadingTimerOnDoWork;
            this._bwUpdater.DoWork += this.BackgroundWorkerOnDoWork;
            this._bwUpdater.RunWorkerCompleted += this.BackgroundWorkerOnRunWorkerCompleted;
            this.ReloadData();
        }

        private void BwLoadingTimerOnDoWork(object sender, DoWorkEventArgs e)
        {
            var sw = Stopwatch.StartNew();
            while (this.IsLoading)
            {
                Thread.Sleep(500);
                this.Info = sw.Elapsed.ToString();
            }

            sw.Stop();
        }

        public override Task Update()
        {
            this.ReloadData();
            return Task.CompletedTask;
        }

        private void ReloadData()
        {
            if (!this.IsLoading && !this._bwUpdater.IsBusy)
            {
                this.IsLoading = true;
                this._bwUpdater.RunWorkerAsync();
            }
        }

        private void BackgroundWorkerOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result is List<ChatSession> sessions)
            {
                this.Sessions = sessions;
            }
            this.IsLoading = false;
        }

        private async void BackgroundWorkerOnDoWork(object sender, DoWorkEventArgs e)
        {
            var userId = this.SettingsService.Settings.UserId;
            var sessions = await this._repository.GetUserSessions(userId);
            e.Result = sessions.OrderByDescending(x => x.DateStart).ToList();
        }
    }
}