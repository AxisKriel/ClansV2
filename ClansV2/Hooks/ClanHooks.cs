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

		public delegate void ClanJoinedD(ClanJoinedEventArgs args);
		public static event ClanJoinedD ClanJoined;

		public delegate void ClanLeftD(ClanLeftEventArgs args);
		public static event ClanLeftD ClanLeft;

		public static void OnClanCreated(Clan clan)
		{
			ClanCreated?.Invoke(new ClanCreatedEventArgs(clan));
		}

		public static void OnClanDisbanded(Clan clan)
		{
			ClanDisbanded?.Invoke(new ClanDisbandedEventArgs(clan));
		}

		public static void OnClanJoined(Clan clan, ClanMember player)
		{
			ClanJoined?.Invoke(new ClanJoinedEventArgs(clan, player));
		}

		public static void OnClanLeft(Clan clan, ClanMember player, bool kick)
		{
			ClanLeft?.Invoke(new ClanLeftEventArgs(clan, player, kick));
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

	public class ClanJoinedEventArgs : EventArgs
	{
		public Clan Clan { get; private set; }
		public ClanMember Player { get; private set; }

		public ClanJoinedEventArgs(Clan clan, ClanMember player)
		{
			Clan = clan;
			Player = player;
		}
	}

	public class ClanLeftEventArgs : EventArgs
	{
		public Clan Clan { get; private set; }
		public ClanMember Player { get; private set; }
		public bool Kick { get; private set; }

		public ClanLeftEventArgs(Clan clan, ClanMember player, bool kick)
		{
			Clan = clan;
			Player = player;
			Kick = kick;
		}
	}
}
