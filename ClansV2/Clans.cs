using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;
using ClansV2.Managers;

namespace ClansV2
{
    [ApiVersion(1, 23)]
    public class Clans : TerrariaPlugin
    {
        public override string Name { get { return "Clans"; } }
        public override string Author { get { return "Professor X"; } }
        public override string Description { get { return ""; } }
        public override Version Version { get { return new Version(1, 0, 0, 0); } }

        internal static Dictionary<int, ClanMember> players = new Dictionary<int, ClanMember>();

        public Clans(Main game) : base(game)
        {
            base.Order = -1;
        }

        #region Initialize/Dispose
        public override void Initialize()
        {
            ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
            ServerApi.Hooks.ServerLeave.Register(this, OnLeave);
            PlayerHooks.PlayerPostLogin += OnPostLogin;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
                ServerApi.Hooks.ServerLeave.Deregister(this, OnLeave);
                PlayerHooks.PlayerPostLogin -= OnPostLogin;
            }
            base.Dispose(disposing);
        }
        #endregion

        #region Hooks
        private void OnInitialize(EventArgs args)
        {
            ClanManager.ConnectDB();
            Commands.ChatCommands.Add(new Command("clans.use", ClanCommands.ClanChatCommand, "c", "csay"));
            Commands.ChatCommands.Add(new Command("clans.use", ClanCommands.MainCommand, "clan"));
        }

        private void OnLeave(LeaveEventArgs args)
        {
            if (players.ContainsKey(args.Who))
                players.Remove(args.Who);
        }

        private void OnPostLogin(PlayerPostLoginEventArgs args)
        {
            if (MemberManager.GetMemberByID(args.Player.User.ID) != null)
            {
                players.Add(args.Player.User.ID, MemberManager.GetMemberByID(args.Player.User.ID));
            }
        }
        #endregion
    }
}
