using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using ChatRoulette.Core.Settings;
using ChatRoulette.Ioc;
using ChatRoulette.Utils;
using Exort.AutoUpdate.Wpf;
using Newtonsoft.Json;
using NLog;

namespace ChatRoulette
{
    public partial class App
    {
        private const string SettingsPath = "settings.json";
        public static Version CurrentVersion => Assembly.GetEntryAssembly()?.GetName().Version;

        public App()
        {
            if (File.Exists(Path.Combine(Environment.CurrentDirectory, "console")))
            {
                if (!ConsoleManager.HasConsole)
                    ConsoleManager.Show();
            }
            IocKernel.Initialize(new IocConfiguration());
            AppDomain.CurrentDomain.AssemblyResolve += Resolver;

            Current.DispatcherUnhandledException += CurrentOnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

            var autoUpdater = new AutoUpdater("BoltunovOleg", "ChatRoulette", Assembly.GetExecutingAssembly());
            var t = autoUpdater.CheckUpdate().GetAwaiter().GetResult();
            if (t)
            {
                var release = autoUpdater.GetLatestRelease().GetAwaiter().GetResult();
                autoUpdater.ShowReleaseInfo(release);
                App.Current.Shutdown(0);
            }
            LogMachineDetails();
            var settingsService = IocKernel.Get<SettingsService>();
            var path = Path.Combine(Environment.CurrentDirectory, SettingsPath);
            settingsService.LoadAsync(path);
        }

        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogManager.GetCurrentClassLogger().Error($"Unhandled exception{Environment.NewLine}{JsonConvert.SerializeObject(e.ExceptionObject)}");
        }

        private void CurrentOnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            LogManager.GetCurrentClassLogger().Error($"Unhandled exception{Environment.NewLine}{e.Exception}");
            e.Handled = true;
        }

        private static Assembly Resolver(object sender, ResolveEventArgs args)
        {
            if (args.Name.StartsWith("CefSharp"))
            {
                var assemblyName = args.Name.Split(new[] {','}, 2)[0] + ".dll";
                var archSpecificPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
                    Environment.Is64BitProcess ? "x64" : "x86", assemblyName);

                LogManager.GetCurrentClassLogger().Info($"Try to load assembly: {assemblyName}\t|\t{archSpecificPath}\t|\t{args.Name} ");

                if (File.Exists(archSpecificPath))
                {
                    return Assembly.LoadFile(archSpecificPath);
                }
                else
                {
                    LogManager.GetCurrentClassLogger().Info($"Assembly not found {archSpecificPath}");
                    return null;
                }
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
    }
}