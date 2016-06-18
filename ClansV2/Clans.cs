using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public class Clans : TerrariaPlugin
    {
        public override string Name { get { return "Clans"; } }
        public override string Author { get { return "Professor X"; } }
        public override string Description { get { return ""; } }
        public override Version Version { get { return new Version(1, 0, 0, 0); } }

        public static ConfigFile Config;
        public static InviteManager InvitesDb = new InviteManager();
        public static ClanManager ClansDb = new ClanManager();
        public static MemberManager MembersDb = new MemberManager();

        internal static Dictionary<int, ClanMember> players = new Dictionary<int, ClanMember>();

        public Clans(Main game) : base(game)
        {

        }

        #region Initialize/Dispose
        public override void Initialize()
        {
            ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
            ServerApi.Hooks.ServerChat.Register(this, OnChat);
            ServerApi.Hooks.ServerLeave.Register(this, OnLeave);
            PlayerHooks.PlayerPostLogin += OnPostLogin;
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
                ServerApi.Hooks.ServerLeave.Deregister(this, OnLeave);
                PlayerHooks.PlayerPostLogin -= OnPostLogin;
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
            InvitesDb.ConnectDB();

            Commands.ChatCommands.Add(new Command("clans.use", ClanCommands.ClanChatCommand, "c", "csay"));
            Commands.ChatCommands.Add(new Command("clans.use", ClanCommands.MainCommand, "clan"));
        }

        private void OnChat(ServerChatEventArgs args)
        {
            if (args.Handled)
                return;

            TSPlayer tsplr = TShock.Players[args.Who];
            if (!args.Text.StartsWith(TShock.Config.CommandSpecifier) && !args.Text.StartsWith(TShock.Config.CommandSilentSpecifier))
            {
                if (tsplr.IsLoggedIn && !tsplr.mute && tsplr.Group.HasPermission(Permissions.canchat) && players.ContainsKey(tsplr.User.ID))
                {
                    Clan clan = players[tsplr.User.ID].Clan;
                    string message = string.Format(Config.ChatFormat, tsplr.Group.Name, tsplr.Group.Prefix, clan.Prefix, tsplr.Name, tsplr.Group.Suffix, args.Text);
                    TSPlayer.All.SendMessage(message, Config.ChatColorsEnabled ? clan.ChatColor.ParseColor() : tsplr.Group.ChatColor.ParseColor());
                    TSPlayer.Server.SendMessage(message, Config.ChatColorsEnabled ? clan.ChatColor.ParseColor() : tsplr.Group.ChatColor.ParseColor());

                    args.Handled = true;
                }
            }
        }

        private void OnLeave(LeaveEventArgs args)
        {
            if (TShock.Players[args.Who] == null || TShock.Players[args.Who].User == null)
                return;

            if (players.ContainsKey(TShock.Players[args.Who].User.ID))
                players.Remove(TShock.Players[args.Who].User.ID);
        }

        private void OnPostLogin(PlayerPostLoginEventArgs args)
        {
            // TODO: TSPlayer extension so I don't have to add/remove players from the dictionary
            if (players.ContainsKey(args.Player.User.ID))
                players.Remove(args.Player.User.ID);

            if (MembersDb.GetMemberByID(args.Player.User.ID) != null)
            {
                players.Add(args.Player.User.ID, MembersDb.GetMemberByID(args.Player.User.ID));
            }
        }

        private void OnReload(ReloadEventArgs args)
        {
            LoadConfig();
        }
        #endregion

        #region Clan Hooks
        private void OnClanCreated(ClanCreatedEventArgs args)
        {
            TSPlayer.All.SendInfoMessage("Clan '{0}' has been established!", args.Clan.Name);
        }

        private void OnClanDisbanded(ClanDisbandedEventArgs args)
        {
            TSPlayer.All.SendInfoMessage("Clan '{0}' has been disbanded!", args.Clan.Name);
        }

        private void OnClanJoined(ClanJoinedEventArgs args)
        {
            args.Clan.SendClanMessage("(Clan) {0} has joined the clan!", TShock.Users.GetUserByID(args.Player.UserID).Name);
        }

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
        private void LoadConfig()
        {
            string path = Path.Combine(TShock.SavePath, "Clans.json");
            Config = ConfigFile.Read(path);
        }
        #endregion
    }
}
