using System;
using System.Collections.Generic;
using System.Windows.Input;
using ChatRoulette.Repository.Model;

namespace ChatRoulette.Core.Session
{
    public class SessionPreference
    {
        public string Mod { get; set; }
        public string Name { get; set; }
        public TimeSpan WorkTime { get; set; }
        public List<ChatConnectionResultEnum> AllowedResults { get; set; }
        public Dictionary<Key, ChatConnectionResultEnum> KeyToResultBinds { get; set; }
        public bool WithBan { get; set; }
        public bool WithReport { get; set; }

        public SessionPreference()
        {
            this.Mod = "0";
            this.Name = "unnamed";
            this.WorkTime = TimeSpan.Zero;
            this.AllowedResults = new List<ChatConnectionResultEnum>();
            this.KeyToResultBinds = new Dictionary<Key, ChatConnectionResultEnum>();
            this.WithBan = true;
            this.WithReport = true;
        }
    }
}