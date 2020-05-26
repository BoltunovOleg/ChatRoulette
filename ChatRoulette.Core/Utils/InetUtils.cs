using System;

namespace ChatRoulette.Core.Utils
{
    public class InetUtils
    {
        private string GetMyIp()
        {
            try
            {
                return new System.Net.WebClient().DownloadString("https://api.ipify.org");
            }
            catch (Exception)
            {
                return "0.0.0.0";
            }
        }
    }
}