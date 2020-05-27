using ChatRoulette.Core.Settings;
using ChatRoulette.Repository;
using ChatRoulette.ViewModel;
using Exort.GithubBugtracker;
using Ninject.Modules;

namespace ChatRoulette.Ioc
{
    public class IocConfiguration : NinjectModule
    {
        public override void Load()
        {
            this.Bind<ChatRepository>().ToSelf().InThreadScope();

            this.Bind<MainViewModel>().ToSelf().InSingletonScope();
            this.Bind<HomeViewModel>().ToSelf().InSingletonScope();
            this.Bind<SessionViewModel>().ToSelf().InSingletonScope();
            this.Bind<SessionPreferencesViewModel>().ToSelf().InSingletonScope();

            this.Bind<SettingsService>().ToSelf().InSingletonScope();

            this.Bind<GithubBugtracker>()
                .ToConstant(new GithubBugtracker(App.GtCredentials.Login, App.GtCredentials.Password));
        }
    }
}