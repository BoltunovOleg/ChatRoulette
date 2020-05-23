using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using ChatRoulette.Core.Settings;
using ChatRoulette.Ioc;
using NLog;

namespace ChatRoulette
{
    public partial class App
    {
        private const string SettingsPath = "settings.json";
        public static Version CurrentVersion => Assembly.GetEntryAssembly()?.GetName().Version;

        public App()
        {
            IocKernel.Initialize(new IocConfiguration());
            AppDomain.CurrentDomain.AssemblyResolve += Resolver;
            LogMachineDetails();
            var settingsService = IocKernel.Get<SettingsService>();
            var path = Path.Combine(Environment.CurrentDirectory, SettingsPath);
            settingsService.LoadAsync(path);
        }

        private static Assembly Resolver(object sender, ResolveEventArgs args)
        {
            if (args.Name.StartsWith("CefSharp"))
            {
                var assemblyName = args.Name.Split(new[] {','}, 2)[0] + ".dll";
                var archSpecificPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
                    Environment.Is64BitProcess ? "x64" : "x86", assemblyName);

                return File.Exists(archSpecificPath)
                    ? Assembly.LoadFile(archSpecificPath)
                    : null;
            }

            return null;
        }

        private static void LogMachineDetails()
        {
            var computer = new Microsoft.VisualBasic.Devices.ComputerInfo();

            var text = "OS: " + computer.OSPlatform + " v" + computer.OSVersion + Environment.NewLine +
                       computer.OSFullName + Environment.NewLine +
                       "RAM: " + computer.TotalPhysicalMemory + Environment.NewLine +
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