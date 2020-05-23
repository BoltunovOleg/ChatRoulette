using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using ChatRoulette.Core.Settings;
using ChatRoulette.Ioc;
using ChatRoulette.Utils;
using NLog;
using Renci.SshNet.Messages;

namespace ChatRoulette
{
    public partial class App
    {
        private const string SettingsPath = "settings.json";
        public static Version CurrentVersion => Assembly.GetEntryAssembly()?.GetName().Version;

        protected override void OnStartup(StartupEventArgs e)
        {
            IocKernel.Initialize(new IocConfiguration());
            

            LogMachineDetails();
            var settingsService = IocKernel.Get<SettingsService>();
            var path = Path.Combine(Environment.CurrentDirectory, SettingsPath);
            var res = settingsService.LoadAsync(path);

            base.OnStartup(e);
        }

        private static void LogMachineDetails()
        {
            var computer = new Microsoft.VisualBasic.Devices.ComputerInfo();

            var text = "OS: " + computer.OSPlatform + " v" + computer.OSVersion + Environment.NewLine +
                       computer.OSFullName + Environment.NewLine +
                       "RAM: " + computer.TotalPhysicalMemory.ToString() + Environment.NewLine +
                       "Language: " + computer.InstalledUICulture.EnglishName;
            LogManager.GetCurrentClassLogger().Info(text);
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            IocKernel.Get<SettingsService>().SaveAsync(SettingsPath);
        }

        private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            LogManager.GetCurrentClassLogger().Error($"Unhandled exception{Environment.NewLine}{e.Exception}");
            e.Handled = true;
            //MessageBox.Show("Критическая ошибка, программа будет закрыта." +Environment.NewLine +
            //                "Сообщите разработчику.");
        }
    }
}