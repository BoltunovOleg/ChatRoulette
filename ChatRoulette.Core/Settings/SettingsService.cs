﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Input;
using ChatRoulette.Core.Session;
using ChatRoulette.Repository.Model;
using Newtonsoft.Json;

namespace ChatRoulette.Core.Settings
{
    public class SettingsService
    {
        public AppSettings Settings { get; private set; }

        public bool SaveAsync(string filePath,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var serialized = JsonConvert.SerializeObject(this.Settings, Formatting.Indented);
            File.WriteAllText(filePath, serialized);
            return true;
        }

        public bool LoadAsync(string filePath,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var text = File.Exists(filePath) ? File.ReadAllText(filePath) : "";
            this.Settings = JsonConvert.DeserializeObject<AppSettings>(text);
            if (this.Settings == null)
                this.Settings = new AppSettings(0);
            return true;
        }
    }

    public class AppSettings
    {
        public int UserId { get; set; }
        public List<SessionPreference> SessionPreferences { get; set; }

        public AppSettings()
        {
        }

        public AppSettings(int userId)
        {
            this.UserId = userId;
            this.SessionPreferences = new List<SessionPreference>
            {
                new SessionPreference
                {
                    Mod = "0v2",
                    Name = "Default v2",
                    WorkTime = TimeSpan.FromMinutes(55),
                    WithBan = true,
                    WithReport = false,
                    WithSpam = true,
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

                        {Key.D1, ChatConnectionResultEnum.Cp},
                        {Key.D2, ChatConnectionResultEnum.Blanket},
                        {Key.D3, ChatConnectionResultEnum.Performer},
                    }
                },
                new SessionPreference
                {
                    Mod = "-1v2",
                    Name = "User Perspective V2",
                    WorkTime = TimeSpan.FromMinutes(30),
                    WithBan = true,
                    WithReport = false,
                    WithSpam = true,
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

                        {Key.C, ChatConnectionResultEnum.Text},

                        {Key.Q, ChatConnectionResultEnum.Age13},
                        {Key.E, ChatConnectionResultEnum.Age16},

                        {Key.Left, ChatConnectionResultEnum.Spam1},
                        {Key.Up, ChatConnectionResultEnum.Spam2},
                        {Key.Right, ChatConnectionResultEnum.Spam3},

                        {Key.D1, ChatConnectionResultEnum.Cp},
                        {Key.D2, ChatConnectionResultEnum.Blanket},
                        {Key.D3, ChatConnectionResultEnum.Performer},
                    }
                },
                new SessionPreference
                {
                    Mod = "0v2",
                    Name = "Unfiltered v2",
                    WorkTime = TimeSpan.FromMinutes(30),
                    WithBan = true,
                    WithReport = false,
                    WithSpam = true,
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

                        {Key.C, ChatConnectionResultEnum.Text},

                        {Key.Q, ChatConnectionResultEnum.Age13},
                        {Key.E, ChatConnectionResultEnum.Age16},

                        {Key.Left, ChatConnectionResultEnum.Spam1},
                        {Key.Up, ChatConnectionResultEnum.Spam2},
                        {Key.Right, ChatConnectionResultEnum.Spam3},

                        {Key.D1, ChatConnectionResultEnum.Cp},
                        {Key.D2, ChatConnectionResultEnum.Blanket},
                        {Key.D3, ChatConnectionResultEnum.Performer},
                    }
                }
            };
        }
    }
}