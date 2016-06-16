using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TShockAPI;
using static ClansV2.Clans;

namespace ClansV2
{
    public class ClanCommands
    {
        public static void ClanChatCommand(CommandArgs args)
        {
            if (!args.Player.IsLoggedIn)
            {
                args.Player.SendErrorMessage("You are not logged in!");
                return;
            }
            else if (!players.ContainsKey(args.Player.User.ID))
            {
                args.Player.SendErrorMessage("You are not in a clan!");
                return;
            }
            else if (args.Player.mute)
            {
                args.Player.SendErrorMessage("You are muted!");
                return;
            }
            else
            {
                string message = string.Join(" ", args.Parameters.Select(p => p));
            }
        }

        public static void MainCommand(CommandArgs args)
        {

        }
    }
}
