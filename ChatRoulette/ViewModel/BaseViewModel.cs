using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ChatRoulette.Core.Settings;
using ChatRoulette.Ioc;
using ChatRoulette.Properties;
using ChatRoulette.Utils;
using MaterialDesignThemes.Wpf;

namespace ChatRoulette.ViewModel
{
    public abstract class BaseViewModel : DependencyObject, INotifyPropertyChanged, INotifyPropertyChanging
    {
        private string _info;
        public abstract string MenuCaption { get; }
        public abstract bool IsLoading { get; set; }
        public abstract bool ShowActionButton { get; set; }
        public abstract bool RedirectInput { get; }

        public virtual string Caption => $"{this.MenuCaption}";
        public virtual string Description => $"v. {App.CurrentVersion}";

        public virtual string Info
        {
            get => this._info;
            set
            {
                this.OnPropertyChanging();
                this._info = value;
                this.OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;
        public event DialogResultEventHandler DialogResult;

        public SettingsService SettingsService { get; } = IocKernel.Get<SettingsService>();

        public virtual async Task<bool> KeyDown(Key key)
        {
            return await Task.Factory.StartNew(() => false);
        }

        public virtual async Task<bool> KeyUp(Key key)
        {
            return await Task.Factory.StartNew(() => false);
        }

        public virtual Task Update()
        {
            return Task.CompletedTask;
        }

        protected ISnackbarMessageQueue SnackbarMessageQueue => IocKernel.Get<ISnackbarMessageQueue>();

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanging([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnDialogResult(object result)
        {
            this.DialogResult?.Invoke(this, result);
        }
    }
}