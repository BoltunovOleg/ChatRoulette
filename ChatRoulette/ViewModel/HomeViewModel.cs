using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
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
        private BackgroundWorker _bwLoadingTimer = new BackgroundWorker();

        public override string MenuCaption { get; } = "Главная";

        public override bool IsLoading
        {
            get => this._isLoading;
            set
            {
                this.OnPropertyChanging();
                this._isLoading = value;
                this.OnPropertyChanged();
                if (value)
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
            this.ReloadData();
        }

        private void BwLoadingTimerOnDoWork(object sender, DoWorkEventArgs e)
        {
            var sw = Stopwatch.StartNew();
            while (this.IsLoading)
            {
                Thread.Sleep(50);
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
            this.IsLoading = true;
            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += this.BackgroundWorkerOnDoWork;
            backgroundWorker.RunWorkerAsync();
        }

        private async void BackgroundWorkerOnDoWork(object sender, DoWorkEventArgs e)
        {
            var userId = this.SettingsService.Settings.UserId;
            this.Sessions = await this._repository.ChatSessions
                .Include(x => x.ChatConnections)
                .Where(x => x.UserNumber == userId)
                .OrderByDescending(x => x.DateStart)
                .ToListAsync();
            this.IsLoading = false;
        }
    }
}