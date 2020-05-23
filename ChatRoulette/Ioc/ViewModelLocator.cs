using ChatRoulette.ViewModel;

namespace ChatRoulette.Ioc
{
    public class ViewModelLocator
    {
        public MainViewModel MainViewModel => IocKernel.Get<MainViewModel>();
        public HomeViewModel HomeViewModel => IocKernel.Get<HomeViewModel>();
        public SessionViewModel SessionViewModel => IocKernel.Get<SessionViewModel>();
        public SessionPreferencesViewModel SessionPreferencesViewModel => IocKernel.Get<SessionPreferencesViewModel>();
    }
}