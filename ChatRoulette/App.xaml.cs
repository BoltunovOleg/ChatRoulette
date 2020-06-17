using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using ChatRoulette.Core.Session;
using ChatRoulette.Core.Settings;
using ChatRoulette.Ioc;
using ChatRoulette.Repository.Model;
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
        public static bool NoUpdate { get; private set; }
        public static Credentials GtCredentials;

        public App()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            AppDomain.CurrentDomain.AssemblyResolve += Resolver;
            this.SetCred();
            IocKernel.Initialize(new IocConfiguration());

            Current.DispatcherUnhandledException += this.CurrentOnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += this.CurrentDomainOnUnhandledException;

            LogMachineDetails();
            var settingsService = IocKernel.Get<SettingsService>();
            var path = Path.Combine(Environment.CurrentDirectory, SettingsPath);
            settingsService.LoadAsync(path);
            this.FixPreferences();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            IsDebug = e.Args.Contains("debug");
            IsConsole = e.Args.Contains("console");
            NoUpdate = e.Args.Contains("noupdate");

            if (IsConsole && !ConsoleManager.HasConsole)
                ConsoleManager.Show();

            if (!NoUpdate)
            {
                var autoUpdater = new AutoUpdater("BoltunovOleg", "ChatRoulette", Assembly.GetExecutingAssembly());
                var t = await autoUpdater.CheckUpdate();
                if (t)
                {
                    var release = await autoUpdater.GetLatestRelease();
                    autoUpdater.ShowReleaseInfo(release);
                    App.Current.Shutdown(0);
                }
            }

            base.OnStartup(e);
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

            if (service.Settings.SessionPreferences.All(x => x.Name != "Like USER"))
            {
                var newPref = new SessionPreference
                {
                    Mod = "-1",
                    Name = "Like USER",
                    WorkTime = TimeSpan.FromMinutes(55),
                    WithBan = false,
                    WithReport = false,
                    AllowedResults = new List<ChatConnectionResultEnum>
                    {
                        ChatConnectionResultEnum.Male,
                        ChatConnectionResultEnum.Female,
                        ChatConnectionResultEnum.OnePlus,
                        ChatConnectionResultEnum.Nobody,
                        ChatConnectionResultEnum.Age13,
                        ChatConnectionResultEnum.Age16,
                        ChatConnectionResultEnum.Text,
                        ChatConnectionResultEnum.Inappropriate,
                        ChatConnectionResultEnum.HiddenInappropriate,
                        ChatConnectionResultEnum.Spam1,
                        ChatConnectionResultEnum.Spam2,
                        ChatConnectionResultEnum.Spam3,
                        ChatConnectionResultEnum.PartnerDisconnected
                    },
                    KeyToResultBinds = new Dictionary<Key, ChatConnectionResultEnum>
                    {
                        {Key.W, ChatConnectionResultEnum.Male},
                        {Key.F, ChatConnectionResultEnum.Female},
                        {Key.A, ChatConnectionResultEnum.OnePlus},
                        {Key.S, ChatConnectionResultEnum.Nobody},
                        {Key.Space, ChatConnectionResultEnum.Inappropriate},
                        {Key.D, ChatConnectionResultEnum.HiddenInappropriate},

                        {Key.C, ChatConnectionResultEnum.Text},

                        {Key.Q, ChatConnectionResultEnum.Age13},
                        {Key.E, ChatConnectionResultEnum.Age16},

                        {Key.Left, ChatConnectionResultEnum.Spam1},
                        {Key.Up, ChatConnectionResultEnum.Spam2},
                        {Key.Right, ChatConnectionResultEnum.Spam3},
                    }
                };
                service.Settings.SessionPreferences.Add(newPref);
            }

            if (service.Settings.SessionPreferences.All(x => x.Name != "30 min bannable"))
            {
                var newPref = new SessionPreference
                {
                    Mod = "1",
                    Name = "30 min bannable",
                    WorkTime = TimeSpan.FromMinutes(30),
                    WithBan = true,
                    WithReport = false,
                    AllowedResults = new List<ChatConnectionResultEnum>
                    {
                        ChatConnectionResultEnum.Male,
                        ChatConnectionResultEnum.Female,
                        ChatConnectionResultEnum.OnePlus,
                        ChatConnectionResultEnum.Nobody,
                        ChatConnectionResultEnum.Inappropriate,
                        ChatConnectionResultEnum.HiddenInappropriate,
                        ChatConnectionResultEnum.Cp,
                        ChatConnectionResultEnum.Blanket,
                        ChatConnectionResultEnum.Performer,
                        ChatConnectionResultEnum.PartnerDisconnected
                    },
                    KeyToResultBinds = new Dictionary<Key, ChatConnectionResultEnum>
                    {
                        {Key.W, ChatConnectionResultEnum.Male},
                        {Key.F, ChatConnectionResultEnum.Female},
                        {Key.A, ChatConnectionResultEnum.OnePlus},
                        {Key.S, ChatConnectionResultEnum.Nobody},
                        {Key.Space, ChatConnectionResultEnum.Inappropriate},
                        {Key.D, ChatConnectionResultEnum.HiddenInappropriate},

                        {Key.D1, ChatConnectionResultEnum.Cp},
                        {Key.D2, ChatConnectionResultEnum.Blanket},
                        {Key.D3, ChatConnectionResultEnum.Performer},
                    }
                };
                service.Settings.SessionPreferences.Add(newPref);
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
                File.WriteAllText(
                    Path.Combine(Environment.CurrentDirectory, $"crash_{DateTime.Now:dd_MM_yyyy_HH_mm_ss}.data"),
                    $"UserId: {userId}{Environment.NewLine}{JsonConvert.SerializeObject(obj)}");
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