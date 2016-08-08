using System;
using ClansV2.Managers;

namespace ClansV2.Hooks
{
	public static class ClanHooks
	{
		/// <summary>
		/// The clan created delegate.
		/// </summary>
		/// <param name="args">The <see cref="ClanCreatedEventArgs"/> object.</param>
		public delegate void ClanCreatedD(ClanCreatedEventArgs args);

		/// <summary>
		/// Occurs after a <see cref="Clan"/> has been created.
		/// </summary>
		public static event ClanCreatedD ClanCreated;

		/// <summary>
		/// The clan disbanded delegate.
		/// </summary>
		/// <param name="args">The <see cref="ClanDisbandedEventArgs"/> object.</param>
		public delegate void ClanDisbandedD(ClanDisbandedEventArgs args);

		/// <summary>
		/// Occurs after a <see cref="Clan"/> has been disbanded.
		/// </summary>
		public static event ClanDisbandedD ClanDisbanded;

		/// <summary>
		/// The clan joined delegate.
		/// </summary>
		/// <param name="args">The <see cref="ClanJoinedEventArgs"/> object.</param>
		public delegate void ClanJoinedD(ClanJoinedEventArgs args);

		/// <summary>
		/// Occurs when a <see cref="ClanMember"/> joins a <see cref="Clan"/>.
		/// </summary>
		public static event ClanJoinedD ClanJoined;

		/// <summary>
		/// The clan left delegate.
		/// </summary>
		/// <param name="args">The <see cref="ClanLeftEventArgs"/> object.</param>
		public delegate void ClanLeftD(ClanLeftEventArgs args);

		/// <summary>
		/// Occurs when a <see cref="ClanMember"/> leaves a <see cref="Clan"/>.
		/// </summary>
		public static event ClanLeftD ClanLeft;

		/// <summary>
		/// Invokes the ClanCreated event.
		/// </summary>
		/// <param name="clan">The <see cref="Clan"/> that has been created.</param>
		public static void OnClanCreated(Clan clan)
		{
			ClanCreated?.Invoke(new ClanCreatedEventArgs(clan));
		}

		/// <summary>
		/// Invokes the ClanDisbanded event.
		/// </summary>
		/// <param name="clan">The <see cref="Clan"/> that has been disbanded.</param>
		public static void OnClanDisbanded(Clan clan)
		{
			ClanDisbanded?.Invoke(new ClanDisbandedEventArgs(clan));
		}

		/// <summary>
		/// Invokes the ClanJoined event.
		/// </summary>
		/// <param name="clan">The <see cref="Clan"/> that the player joined.</param>
		/// <param name="player">The <see cref="ClanMember"/> who joined the clan.</param>
		public static void OnClanJoined(Clan clan, ClanMember player)
		{
			ClanJoined?.Invoke(new ClanJoinedEventArgs(clan, player));
		}

		/// <summary>
		/// Invokes the ClanLeft event.
		/// </summary>
		/// <param name="clan">The <see cref="ClanMember"/> that the player left.</param>
		/// <param name="player">The <see cref="ClanMember"/> who left the clan.</param>
		/// <param name="kick">Whether the player was kicked or not.</param>
		public static void OnClanLeft(Clan clan, ClanMember player, bool kick)
		{
			ClanLeft?.Invoke(new ClanLeftEventArgs(clan, player, kick));
		}
	}

	/// <summary>
	/// Provides data for the <see cref="ClanHooks.ClanCreated"/> event.
	/// </summary>
	public class ClanCreatedEventArgs : EventArgs
	{
		/// <summary>
		/// The <see cref="Clan"/> that has been created.
		/// </summary>
		public Clan Clan { get; private set; }

		/// <summary>
		/// Intializes a new instance of the <see cref="ClanCreatedEventArgs"/> class.
		/// </summary>
		/// <param name="clan">The <see cref="Clan"/> that has been created.</param>
		public ClanCreatedEventArgs(Clan clan)
		{
			Clan = clan;
		}
	}

	/// <summary>
	/// Provides data for the <see cref="ClanHooks.ClanDisbanded"/> event.
	/// </summary>
	public class ClanDisbandedEventArgs : EventArgs
	{
		/// <summary>
		/// The <see cref="Clan"/> that has been disbanded.
		/// </summary>
		public Clan Clan { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ClanDisbandedEventArgs"/> class.
		/// </summary>
		/// <param name="clan">The <see cref="Clan"/> that has been disbanded.</param>
		public ClanDisbandedEventArgs(Clan clan)
		{
			Clan = clan;
		}
	}

	/// <summary>
	/// Provides data for the <see cref="ClanHooks.ClanJoined"/> event.
	/// </summary>
	public class ClanJoinedEventArgs : EventArgs
	{
		/// <summary>
		/// The <see cref="Clan"/> that the <see cref="Player"/> joined.
		/// </summary>
		public Clan Clan { get; private set; }

		/// <summary>
		/// The <see cref="ClanMember"/> who joined the <see cref="Clan"/>.
		/// </summary>
		public ClanMember Player { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ClanJoinedEventArgs"/> class.
		/// </summary>
		/// <param name="clan">The <see cref="Clan"/> that the <see cref="Player"/> joined.</param>
		/// <param name="player">The <see cref="ClanMember"/> who joined the <see cref="Clan"/>.</param>
		public ClanJoinedEventArgs(Clan clan, ClanMember player)
		{
			Clan = clan;
			Player = player;
		}
	}

	/// <summary>
	/// Provides data for the <see cref="ClanHooks.ClanLeft"/> event.
	/// </summary>
	public class ClanLeftEventArgs : EventArgs
	{
		/// <summary>
		/// The <see cref="Clan"/> the <see cref="Player"/> left.
		/// </summary>
		public Clan Clan { get; private set; }

		/// <summary>
		/// The <see cref="ClanMember"/> who left the <see cref="Clan"/>.
		/// </summary>
		public ClanMember Player { get; private set; }

		/// <summary>
		/// Whether the <see cref="Player"/> was kicked or not.
		/// </summary>
		public bool Kick { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ClanLeftEventArgs"/> class.
		/// </summary>
		/// <param name="clan">The <see cref="Clan"/> the <see cref="Player"/> left.</param>
		/// <param name="player">The <see cref="ClanMember"/> who left the <see cref="Clan"/>.</param>
		/// <param name="kick">Whether the <see cref="Player"/> was kicked or not.</param>
		public ClanLeftEventArgs(Clan clan, ClanMember player, bool kick)
		{
			Clan = clan;
			Player = player;
			Kick = kick;
		}
	}
}
