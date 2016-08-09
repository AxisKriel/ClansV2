using System;
using System.IO;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;
using ClansV2.Hooks;
using ClansV2.Managers;
using ClansV2.Extensions;

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
			ServerApi.Hooks.ServerChat.Register(this, OnChat);
			GeneralHooks.ReloadEvent += OnReload;

			ClanHooks.ClanCreated += OnClanCreated;
			ClanHooks.ClanDisbanded += OnClanDisbanded;
			ClanHooks.ClanJoined += OnClanJoined;
			ClanHooks.ClanLeft += OnClanLeft;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
				ServerApi.Hooks.ServerChat.Deregister(this, OnChat);
				GeneralHooks.ReloadEvent -= OnReload;

				ClanHooks.ClanCreated -= OnClanCreated;
				ClanHooks.ClanDisbanded -= OnClanDisbanded;
				ClanHooks.ClanJoined -= OnClanJoined;
				ClanHooks.ClanLeft -= OnClanLeft;
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

		private void OnChat(ServerChatEventArgs args)
		{
			// Return if the event has already been handled by another plugin.
			if (args.Handled)
				return;

			// Don't handle if the chat format wasn't specified.
			if (string.IsNullOrWhiteSpace(Config.ChatFormat))
				return;

			// Get the player who triggered the event.
			TSPlayer tsplr = TShock.Players[args.Who];

			// Ensure the text isn't a command.
			if (!args.Text.StartsWith(TShock.Config.CommandSpecifier) && !args.Text.StartsWith(TShock.Config.CommandSilentSpecifier))
			{
				// Ensure the player is in a clan, not muted, and has the permission to speak.
				if (tsplr.GetPlayerInfo() != null && !tsplr.mute && tsplr.HasPermission(TShockAPI.Permissions.canchat))
				{
					Clan clan = tsplr.GetPlayerInfo().Clan;

					// Format the chat message and display it.
					string message = string.Format(Config.ChatFormat, tsplr.Group.Name, tsplr.Group.Prefix, clan.Prefix, tsplr.Name, tsplr.Group.Suffix, args.Text);
					TSPlayer.All.SendMessage(message, Config.ChatColorsEnabled ? clan.ChatColor.ParseColor() : tsplr.Group.ChatColor.ParseColor());
					TSPlayer.Server.SendMessage(message, Config.ChatColorsEnabled ? clan.ChatColor.ParseColor() : tsplr.Group.ChatColor.ParseColor());

					// Handle the event so (hopefully) other plugins don't handle it too.
					args.Handled = true;
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
