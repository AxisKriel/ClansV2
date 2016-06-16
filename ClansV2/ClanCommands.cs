using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TShockAPI;
using ClansV2.Managers;
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
            else if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("You must enter a valid message.");
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
                ClanManager.SendClanMessage(players[args.Player.User.ID].Clan, "(Clan) [{0}] {1}: {2}", players[args.Player.User.ID].Rank.Item2.ToString(), args.Player.Name, message);
            }
        }

        public static void MainCommand(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("Invalid syntax! Type {0}clan help for a list of valid commands.", TShock.Config.CommandSpecifier);
                return;
            }

            switch (args.Parameters[0].ToLower())
            {
                case "help":
                    #region Send Help
                    {
                        SendHelp(args);
                    }
                    #endregion
                    break;
                case "create":
                    #region Create New Clan
                    {
                        CreateClan(args);
                    }
                    #endregion
                    break;
            }
        }

        private static void SendHelp(CommandArgs args)
        {
            int pageNum;
            if (!PaginationTools.TryParsePageNumber(args.Parameters, 1, args.Player, out pageNum))
                return;

            List<string> help = new List<string>()
            {
                "clan create <name> - creates a new clan with the given name",
                "clan disband - disbands your current clan",
                "clan motd [message] - prints or sets the clan's MotD",
                "clan prefix <prefix> - sets the clan's prefix",
                "clan color <rrr,ggg,bbb> - sets the chat color of the clan",
                "clan kick <member name> - kicks a member from the clan",
                "clan quit - leaves your current clan",
                "clan invite <player name> - invites a player to your clan",
                "clan accept - accepts the current clan invite",
                "clan decline - declines the current clan invite",
                "clan members - lists your clan's members",
                "clan list - lists all clans on the server",
                "c <message> - prints a message to your clan"
            };

            PaginationTools.SendPage(args.Player, pageNum, help, new PaginationTools.Settings()
            {
                HeaderFormat = "Clan Help ({0}/{1})",
                FooterFormat = "Type {0}clan help {{0}} for more.".SFormat(TShock.Config.CommandSpecifier)
            });
        }

        private static void CreateClan(CommandArgs args)
        {
            if (!args.Player.IsLoggedIn)
            {
                args.Player.SendErrorMessage("You are not logged in!");
                return;
            }

            if (args.Parameters.Count < 2)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}clan create <clan name>", TShock.Config.CommandSpecifier);
                return;
            }

            args.Parameters.RemoveAt(0);
            string clanName = string.Join(" ", args.Parameters.Select(p => p));
            if (players.ContainsKey(args.Player.User.ID))
            {
                args.Player.SendErrorMessage("You are already in a clan!");
                return;
            }
            else if (clanName.Length > Config.NameLength)
            {
                args.Player.SendErrorMessage("Your clan's name cannot be longer than {0} characters.", Config.NameLength);
                return;
            }
            else
            {
                if (ClanManager.AddClan(new Clan() { Name = clanName, Prefix = clanName, MotD = "", ChatColor = "255,255,255" }, args.Player.User.ID))
                {
                    args.Player.SendInfoMessage("Your clan was created successfully.");
                }
                else
                {
                    args.Player.SendErrorMessage("A clan by this name already exists.");
                }
            }
        }

        private static void DisbandClan(CommandArgs args)
        {
            if (!args.Player.IsLoggedIn)
            {
                args.Player.SendErrorMessage("You are not logged in!");
                return;
            }

            if (!players.ContainsKey(args.Player.User.ID))
            {
                args.Player.SendErrorMessage("You are not in a clan!");
                return;
            }
            else if (players[args.Player.User.ID].Rank.Item1 != (int)ClanRank.Founder)
            {
                args.Player.SendErrorMessage("Only clan founders can disband clans!");
                return;
            }
            else
            {
                ClanManager.RemoveClan(players[args.Player.User.ID].Clan);
            }
        }
    }
}
