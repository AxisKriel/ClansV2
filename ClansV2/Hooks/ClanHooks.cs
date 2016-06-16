using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClansV2.Managers;

namespace ClansV2.Hooks
{
    public static class ClanHooks
    {
        public delegate void ClanCreatedD(ClanCreatedEventArgs args);
        public static event ClanCreatedD ClanCreated;

        public delegate void ClanDisbandedD(ClanDisbandedEventArgs args);
        public static event ClanDisbandedD ClanDisbanded;

        public static void OnClanCreated(Clan clan)
        {
            ClanCreated?.Invoke(new ClanCreatedEventArgs(clan));
        }

        public static void OnClanDisbanded(Clan clan)
        {
            ClanDisbanded?.Invoke(new ClanDisbandedEventArgs(clan));
        }
    }

    public class ClanCreatedEventArgs : EventArgs
    {
        public Clan Clan { get; private set; }

        public ClanCreatedEventArgs(Clan clan)
        {
            Clan = clan;
        }
    }

    public class ClanDisbandedEventArgs : EventArgs
    {
        public Clan Clan { get; private set; }

        public ClanDisbandedEventArgs(Clan clan)
        {
            Clan = clan;
        }
    }
}
