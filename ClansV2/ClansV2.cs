using System;
using System.IO;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;
using ClansV2.Hooks;
using ClansV2.Managers;
using ClansV2.Extensions;
using DiscordBridge.Chat;

namespace ClansV2
{
	[ApiVersion(1, 23)]
	public class ClansV2 : TerrariaPlugin
	{
		public override string Name { get { return "Clans"; } }
		public override string Author { get { return "Ancientgods, maintained by Professor X"; } }
		public override string Description { get { return ""; } }
		public override Version Version { get { return new Version(1, 0, 1, 0); } }

		public ConfigFile Config = new ConfigFile();
		public InviteManager Invites = new InviteManager();
		public ClanManager Clans = new ClanManager();
		public MemberManager Members = new MemberManager();

		public static ClansV2 Instance;
		public ClansV2(Main game) : base(game)
		{
			Instance = this;
		}

		#region Initialize/Dispose
		public override void Initialize()
		{
			ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
			GeneralHooks.ReloadEvent += OnReload;

			ClanHooks.ClanCreated += OnClanCreated;
			ClanHooks.ClanDisbanded += OnClanDisbanded;
			ClanHooks.ClanJoined += OnClanJoined;
			ClanHooks.ClanLeft += OnClanLeft;

			ChatHandler.PlayerChatting += OnChat;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
				GeneralHooks.ReloadEvent -= OnReload;

				ClanHooks.ClanCreated -= OnClanCreated;
				ClanHooks.ClanDisbanded -= OnClanDisbanded;
				ClanHooks.ClanJoined -= OnClanJoined;
				ClanHooks.ClanLeft -= OnClanLeft;

				ChatHandler.PlayerChatting -= OnChat;
			}
			base.Dispose(disposing);
		}
		#endregion

		#region Server Api Hooks
		private void OnInitialize(EventArgs args)
		{
			LoadConfig();
			Invites.ConnectDB();
			Clans.ConnectDB();
			Members.ConnectDB();

			Commands.ChatCommands.Add(new Command(Permissions.ClansUse, ClanCommands.ClanChatCommand, "c", "csay"));
			Commands.ChatCommands.Add(new Command(Permissions.ClanChat, ClanCommands.MainCommand, "clan"));
		}

		private void OnChat(object sender, PlayerChattingEventArgs args)
		{
			// Don't handle if the chat format wasn't specified.
			if (String.IsNullOrWhiteSpace(Config.PrefixFormat) && String.IsNullOrWhiteSpace(Config.SuffixFormat))
				return;

			// Ensure the player is in a clan
			if (args.Player.GetPlayerInfo() != null)
			{
				Clan clan = args.Player.GetPlayerInfo().Clan;

				/* Since ChatHandler only supports one universal chat format with a preset of parameters, we're giving the
				 * user the ability to decide where they want to include the clan prefix, and how to format it alongside
				 * the default group prefix/suffix. This is as flexible as I was able to make it; deprecates ChatFormat.
				 */

				if (!String.IsNullOrWhiteSpace(Config.PrefixFormat))
				{
					// Remove tshock defaults
					args.Message.Prefixes.RemoveAll(p => p.Text == args.Player.Group.Prefix);

					args.Message.Prefix(String.Format(Config.PrefixFormat, args.Player.Group.Prefix, clan.Prefix),
						Config.ChatColorsEnabled ? clan.ChatColor.ParseColor() : args.Player.Group.ChatColor.ParseColor());
				}

				if (!String.IsNullOrWhiteSpace(Config.SuffixFormat))
				{
					// Remove tshock defaults
					args.Message.Suffixes.RemoveAll(s => s.Text == args.Player.Group.Suffix);

					args.Message.Suffix(String.Format(Config.SuffixFormat, args.Player.Group.Suffix, clan.Prefix),
						Config.ChatColorsEnabled ? clan.ChatColor.ParseColor() : args.Player.Group.ChatColor.ParseColor());
				}
			}
		}

		/// <summary>
		/// Internal hook, fired when a player executes /reload
		/// </summary>
		/// <param name="args">The <see cref="ReloadEventArgs"/> object.</param>
		private void OnReload(ReloadEventArgs args)
		{
			LoadConfig();
		}
		#endregion

		#region Clan Hooks
		/// <summary>
		/// Internal hook, fired when a clan has been created.
		/// </summary>
		/// <param name="args">The <see cref="ClanCreatedEventArgs"/> object.</param>
		private void OnClanCreated(ClanCreatedEventArgs args)
		{
			TSPlayer.All.SendInfoMessage("Clan '{0}' has been established!", args.Clan.Name);
		}

		/// <summary>
		/// Internal hook, fired when a clan has been disbanded.
		/// </summary>
		/// <param name="args">The <see cref="ClanDisbandedEventArgs"/> object.</param>
		private void OnClanDisbanded(ClanDisbandedEventArgs args)
		{
			TSPlayer.All.SendInfoMessage("Clan '{0}' has been disbanded!", args.Clan.Name);
		}

		/// <summary>
		/// Internal hook, fired when a player joins a clan.
		/// </summary>
		/// <param name="args">The <see cref="ClanJoinedEventArgs"/> object.</param>
		private void OnClanJoined(ClanJoinedEventArgs args)
		{
			args.Clan.SendClanMessage("(Clan) {0} has joined the clan!", TShock.Users.GetUserByID(args.Player.UserID).Name);
		}

		/// <summary>
		/// Internal hook, fired when a player leaves a clan.
		/// </summary>
		/// <param name="args">The <see cref="ClanLeftEventArgs"/> object.</param>
		private void OnClanLeft(ClanLeftEventArgs args)
		{
			if (args.Kick)
			{
				args.Clan.SendClanMessage("(Clan) {0} has been kicked from the clan!", TShock.Users.GetUserByID(args.Player.UserID).Name);
				return;
			}

			args.Clan.SendClanMessage("(Clan) {0} has left the clan!", TShock.Users.GetUserByID(args.Player.UserID).Name);
		}
		#endregion

		#region Load Config
		/// <summary>
		/// Reloads the configuration file.
		/// </summary>
		private void LoadConfig()
		{
			string path = Path.Combine(TShock.SavePath, "Clans.json");
			Config = ConfigFile.Read(path);
		}
		#endregion
	}
}
