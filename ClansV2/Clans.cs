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

        internal static ConfigFile Config;
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
            }
            base.Dispose(disposing);
        }
        #endregion

        #region Server Api Hooks
        private void OnInitialize(EventArgs args)
        {
            LoadConfig();
            ClanManager.ConnectDB();
            MemberManager.ConnectDB();
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
                    TSPlayer.All.SendMessage(message, clan.ChatColor.ParseColor());
                    TSPlayer.Server.SendMessage(message, clan.ChatColor.ParseColor());

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
            if (players.ContainsKey(args.Player.User.ID))
                players.Remove(args.Player.User.ID);

            if (MemberManager.GetMemberByID(args.Player.User.ID) != null)
            {
                players.Add(args.Player.User.ID, MemberManager.GetMemberByID(args.Player.User.ID));
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
