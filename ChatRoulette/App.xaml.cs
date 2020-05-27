using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using ChatRoulette.Core.Settings;
using ChatRoulette.Ioc;
using ChatRoulette.Utils;
using Exort.AutoUpdate.Wpf;
using Exort.GithubBugtracker;
using Meziantou.Framework.Win32;
using Newtonsoft.Json;
using NLog;
using Octokit;

namespace ChatRoulette
{
    public partial class App
    {
        private const string SettingsPath = "settings.json";
        public static Version CurrentVersion => Assembly.GetEntryAssembly()?.GetName().Version;
        public static GithubBugtracker Bugtracker => IocKernel.Get<GithubBugtracker>();
        public static bool IsDebug { get; private set; }
        public static bool IsConsole { get; private set; }
        public static Credentials GtCredentials;

        public App()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            AppDomain.CurrentDomain.AssemblyResolve += Resolver;
            this.SetCred();
            IocKernel.Initialize(new IocConfiguration());

            Current.DispatcherUnhandledException += this.CurrentOnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += this.CurrentDomainOnUnhandledException;

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
            this.FixPreferences();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            IsDebug = e.Args.Contains("debug");
            IsConsole = e.Args.Contains("console");

            if (IsConsole && !ConsoleManager.HasConsole)
                ConsoleManager.Show();
        }

        private void FixPreferences()
        {
            var service = IocKernel.Get<SettingsService>();
            var minTime = TimeSpan.FromMinutes(29);
            foreach (var sessionPreference in service.Settings.SessionPreferences)
            {
                if (sessionPreference.WorkTime < minTime)
                {
                    SendBugReport("Время сессии менее 29 минут");
                }
                if (sessionPreference.Mod == "0")
                {
                    sessionPreference.WithBan = true;
                    sessionPreference.WithReport = true;
                }
                else
                {
                    sessionPreference.WithBan = false;
                    sessionPreference.WithReport = false;
                }
            }
        }

        private void SetCred()
        {
            var cred = CredentialManager.ReadCredential("GitHub");
            if (cred == null)
            {
                var result = CredentialManager.PromptForCredentials(captionText: "GitHub аккаунт");
                if (result == null)
                {
                    this.Shutdown(0);
                    return;
                }

                CredentialManager.WriteCredential("GitHub", result.UserName, result.Password,
                    CredentialPersistence.LocalMachine);

                GtCredentials = new Credentials(result.UserName, result.Password);
            }
            else
            {
                GtCredentials = new Credentials(cred.UserName, cred.Password);
            }
        }

        public static async void SendBugReport(object obj)
        {
            try
            {
                var userId = 0;
                var settingsService = IocKernel.Get<SettingsService>();
                if (settingsService?.Settings != null)
                    userId = settingsService.Settings.UserId;
                await Bugtracker.CreateIssue("BoltunovOleg", "ChatRoulette", "Unhandled exception",
                    $"UserId: {userId}{Environment.NewLine}" +
                    $"{JsonConvert.SerializeObject(obj)}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Critical error");
                App.Current.Shutdown(0);
            }
        }

        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            SendBugReport(e.ExceptionObject);
            LogManager.GetCurrentClassLogger()
                .Error($"Unhandled exception{Environment.NewLine}{JsonConvert.SerializeObject(e.ExceptionObject)}");
        }

        private void CurrentOnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            SendBugReport(e.Exception);
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

                LogManager.GetCurrentClassLogger()
                    .Info($"Try to load assembly: {assemblyName}\t|\t{archSpecificPath}\t|\t{args.Name} ");

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