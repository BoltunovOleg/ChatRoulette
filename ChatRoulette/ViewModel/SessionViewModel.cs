using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ChatRoulette.Core.Session;
using ChatRoulette.Repository;
using ChatRoulette.Repository.Exceptions;
using ChatRoulette.Repository.Model;
using ChatRoulette.Utils.Commands;
using NLog;

namespace ChatRoulette.ViewModel
{
    public class SessionViewModel : BaseViewModel
    {
        private readonly ChatRepository _repository;
        private bool _isLoading;
        private readonly BackgroundWorker _bwLoadingTimer = new BackgroundWorker();
        private SessionController _sessionController;
        private SessionPreference _preference;
        private bool _resultSending;
        public override string MenuCaption { get; } = "Сессия";
        public override bool ShowActionButton { get; set; } = false;
        public override bool RedirectInput { get; } = true;

        public SessionController SessionController
        {
            get => this._sessionController;
            set
            {
                this._sessionController = value;
                this.OnPropertyChanged();
            }
        }

        public ICommand ShowMyCameraCommand { get; }
        public ICommand HideMyCameraCommand { get; }

        public SessionViewModel(ChatRepository repository)
        {
            this._repository = repository;
            this._bwLoadingTimer.DoWork += this.BwLoadingTimerOnDoWork;
            this.ShowMyCameraCommand = new RelayCommand(this.ShowMyCamera, this.CanShowMyCamera);
            this.HideMyCameraCommand = new RelayCommand(this.HideMyCamera, this.CanShowMyCamera);
        }

        public void SetPreference(SessionPreference preference)
        {
            this._preference = preference;
        }

        private bool CanShowMyCamera()
        {
            return this.SessionController?.BrowserController?.Browser != null;
        }

        private async Task HideMyCamera()
        {
            await this.SessionController.BrowserController.HideMyCamera();
        }

        private async Task ShowMyCamera()
        {
            await this.SessionController.BrowserController.ShowMyCamera();
        }

        public override async Task<bool> KeyDown(Key key)
        {
            var result = await this.SessionController.KeyDown(key);
            if (result != null)
                this.SnackbarMessageQueue.Enqueue(result.ToString());
            return result != null;
        }

        public override Task Update()
        {
            this.Reload();
            return Task.CompletedTask;
        }

        private void Reload()
        {
            if (this.IsLoading)
                return;
            this.IsLoading = true;
            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += this.BackgroundWorkerOnDoWork;
            backgroundWorker.RunWorkerCompleted += this.BackgroundWorkerOnRunWorkerCompleted;
            backgroundWorker.RunWorkerAsync();
        }

        private void BackgroundWorkerOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result is ChatSession chatSession)
            {
                this.SessionController = new SessionController(this._repository,
                    this._preference ?? this.SettingsService.Settings.SessionPreferences.First(), chatSession,
                    LogManager.GetCurrentClassLogger(), App.SendBugReport);
                this.SessionController.PropertyChanged += this.SessionControllerOnPropertyChanged;
                this.SessionController.SessionEnd += this.SessionControllerOnSessionEnd;
            }

            if (e.Result is CannotStartNewSessionException exc)
            {
                this.SnackbarMessageQueue.Enqueue("CSNSexception");
                this.OnDialogResult(null);
                return;
            }

            if (e.Result is Exception ex)
            {
                App.SendBugReport(ex);
                this.SnackbarMessageQueue.Enqueue("UNException");
                LogManager.GetCurrentClassLogger().Error($"Unhandled UNException {Environment.NewLine}{ex}");
                this.OnDialogResult(null);
            }

            this.IsLoading = false;
        }

        private void SessionControllerOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(this._sessionController.EventProcessingStarted):
                    //this.ResultSending = this._sessionController.EventProcessingStarted;
                    break;
                case nameof(this._sessionController.Ip):
                case nameof(this._sessionController.BanState):
                case nameof(this._sessionController.BrowserBanState):
                    this.UpdateViewStatus();
                    break;
            }
        }

        private void UpdateViewStatus()
        {
            if (App.IsDebug)
            {
                this.ViewStatus =
                    $"IP: {this.SessionController.Ip} | BS: {this._sessionController.BanState} | BBS: {this._sessionController.BrowserBanState}";
            }
            else
            {
                this.ViewStatus = $"IP: {this.SessionController.Ip}";
            }
        }

        private void SessionControllerOnSessionEnd(object sender, ChatSession session)
        {
            this.OnDialogResult(session);
        }


        private void BackgroundWorkerOnDoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                e.Result = this._repository.GetOrCreateSession(this.SettingsService.Settings.UserId).GetAwaiter()
                    .GetResult();
            }
            catch (CannotStartNewSessionException ex)
            {
                e.Result = ex;
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private void BwLoadingTimerOnDoWork(object sender, DoWorkEventArgs e)
        {
            var sw = Stopwatch.StartNew();
            var bw = sender as BackgroundWorker;
            while (this.IsLoading || this.ResultSending)
            {
                Thread.Sleep(500);
                this.Info = sw.Elapsed.ToString();
            }

            sw.Stop();
        }

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

        public bool ResultSending
        {
            get => this._resultSending;
            set
            {
                this.OnPropertyChanging();
                this._resultSending = value;
                this.OnPropertyChanged();
                if (value && !this._bwLoadingTimer.IsBusy)
                    this._bwLoadingTimer.RunWorkerAsync();
            }
        }
    }
}