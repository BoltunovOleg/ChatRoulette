using System.Threading.Tasks;
using System.Windows.Input;
using ChatRoulette.Ioc;
using ChatRoulette.Utils.Commands;

namespace ChatRoulette.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        public override string MenuCaption { get; } = "";
        public override bool IsLoading { get; set; } = false;
        public override bool ShowActionButton { get; set; } = false;
        public override bool RedirectInput { get; } = false;

        public ICommand StartSessionCommand { get; }

        private BaseViewModel _content = IocKernel.Get<HomeViewModel>();

        public BaseViewModel Content
        {
            get => this._content;
            set
            {
                this.OnPropertyChanging(nameof(this.Content));
                this.OnPropertyChanging(nameof(this.Caption));
                this._content = value;
                this.OnPropertyChanged(nameof(this.Content));
                this.OnPropertyChanged(nameof(this.Caption));
            }
        }

        public MainViewModel()
        {
            this.StartSessionCommand = new RelayCommand(this.Execute);
        }

        private Task Execute()
        {
            var sessionPreferencesViewModel = IocKernel.Get<SessionPreferencesViewModel>();
            this.Content = sessionPreferencesViewModel;
            return Task.CompletedTask;
        }
    }
}