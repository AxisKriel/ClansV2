using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TShockAPI;
using ClansV2.Extensions;

namespace ClansV2.Managers
{
    public class Clan
    {
        public string Name { get; set; }
        public string Prefix { get; set; }
        public string MotD { get; set; }
        public string ChatColor { get; set; }

        public static bool operator ==(Clan clan, Clan clan2)
        {
            return clan.Name == clan2.Name;
        }

        public static bool operator !=(Clan clan, Clan clan2)
        {
            return clan.Name != clan2.Name;
        }

        public void SendClanMessage(string message, params object[] args)
        {
            foreach (TSPlayer tsplr in TShock.Players.Where(tsplr => tsplr != null && tsplr.IsLoggedIn))
            {
                if (tsplr.GetPlayerInfo() != null && tsplr.GetPlayerInfo().Clan == this)
                {
                    tsplr.SendMessage(string.Format(message, args), ChatColor.ParseColor());
                }
            }
        }

        public Clan()
        {
            Name = "";
            Prefix = "";
            MotD = "";
            ChatColor = "";
        }
    }
}
