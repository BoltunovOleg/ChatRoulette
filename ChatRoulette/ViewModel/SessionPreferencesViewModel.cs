using System.Threading.Tasks;
using System.Windows.Input;
using ChatRoulette.Core.Session;
using ChatRoulette.Ioc;
using ChatRoulette.Utils.Commands;

namespace ChatRoulette.ViewModel
{
    public class SessionPreferencesViewModel : BaseViewModel
    {
        private SessionPreference _selectedSessionPreference;
        public override string MenuCaption { get; } = "Выбор типа сессии";
        public override bool IsLoading { get; set; }
        public override bool ShowActionButton { get; set; } = false;
        public override bool RedirectInput { get; } = false;

        public ICommand SelectSessionPreferenceCommand { get; }

        public SessionPreferencesViewModel()
        {
            this.SelectSessionPreferenceCommand = new RelayCommand(this.Select, this.CanSelect);
        }

        private bool CanSelect()
        {
            return this.SelectedSessionPreference != null;
        }

        public override Task Update()
        {
            this.SelectedSessionPreference = null;
            return Task.CompletedTask;
        }

        private Task Select()
        {
            if (this.SelectedSessionPreference != null)
            {
                var mainViewModel = IocKernel.Get<MainViewModel>();

                var sessionViewModel = IocKernel.Get<SessionViewModel>();
                var homeViewModel = IocKernel.Get<HomeViewModel>();

                sessionViewModel.SetPreference(this.SelectedSessionPreference);
                sessionViewModel.DialogResult += (sender, result) =>
                {
                    mainViewModel.Content = homeViewModel;
                    homeViewModel.Update().GetAwaiter().GetResult();
                };

                mainViewModel.Content = sessionViewModel;
                sessionViewModel.Update();
            }

            return Task.CompletedTask;
        }

        public SessionPreference SelectedSessionPreference
        {
            get => this._selectedSessionPreference;
            set
            {
                this.OnPropertyChanging();
                this._selectedSessionPreference = value;
                this.OnPropertyChanged();
            }
        }
    }
}