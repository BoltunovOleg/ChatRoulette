using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace ChatRoulette.Core.Session
{
    public class ChatConnectionInfo : INotifyPropertyChanged
    {
        private TimeSpan _duration;
        public int Id { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public bool Handled { get; set; }

        public TimeSpan Duration
        {
            get => this._duration;
            set
            {
                this._duration = value;
                this.OnPropertyChanged();
            }
        }

        public ChatConnectionInfo(int id)
        {
            this.Id = id;
            this.DateStart = DateTime.Now;
            this.DateEnd = new DateTime(1900, 1, 1);
            this.Handled = false;
            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += this.BackgroundWorkerOnDoWork;
            backgroundWorker.RunWorkerAsync();
        }

        private void BackgroundWorkerOnDoWork(object sender, DoWorkEventArgs e)
        {
            while (this.DateEnd.Year == 1900)
            {
                this.Duration = DateTime.Now - this.DateStart;
                Thread.Sleep(100);
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}