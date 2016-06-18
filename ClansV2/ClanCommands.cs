using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TShockAPI;
using ClansV2.Managers;
using ClansV2.Extensions;
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
                players[args.Player.User.ID].Clan.SendClanMessage("(Clan) [{0}] {1}: {2}", players[args.Player.User.ID].Rank.Item2.ToString(), args.Player.Name, message);
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
                case "disband":
                    #region Disband Clan
                    {
                        DisbandClan(args);
                    }
                    #endregion
                    break;
                case "motd":
                    #region Print/Display MotD
                    {
                        SetMotd(args);
                    }
                    #endregion
                    break;
                case "prefix":
                    #region Set Clan Prefix
                    {
                        SetPrefix(args);
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
                if (ClanManager.RemoveClan(players[args.Player.User.ID].Clan))
                {
                    args.Player.SendSuccessMessage("Clan disbanded successfully.");
                }
                else
                {
                    args.Player.SendErrorMessage("Something went wrong... Check logs for more details.");
                }
            }
        }

        private static void SetMotd(CommandArgs args)
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
            else
            {
                if (args.Parameters.Count >= 2)
                {
                    args.Parameters.RemoveAt(0);
                    string motd = string.Join(" ", args.Parameters.Select(p => p));
                    if (players[args.Player.User.ID].Rank.Item1 != (int)ClanRank.Founder)
                    {
                        args.Player.SendErrorMessage("Only clan founders can set the clan's MotD!");
                        return;
                    }
                    else
                    {
                        if (ClanManager.SetClanMotd(players[args.Player.User.ID].Clan, motd))
                        {
                            args.Player.SendSuccessMessage("Clan MotD set successfully!");
                        }
                        else
                        {
                            args.Player.SendErrorMessage("Something went wrong... Check logs for more details.");
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(players[args.Player.User.ID].Clan.MotD))
                    {
                        args.Player.SendMessage(players[args.Player.User.ID].Clan.MotD, players[args.Player.User.ID].Clan.ChatColor.ParseColor());
                    }
                    else
                    {
                        args.Player.SendErrorMessage("Your clan does not have a MotD set.");
                    }
                }
            }
        }

        private static void SetPrefix(CommandArgs args)
        {
            if (!args.Player.IsLoggedIn)
            {
                args.Player.SendErrorMessage("You are not logged in!");
                return;
            }

            if (args.Parameters.Count < 2)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}clan prefix <new prefix>", TShock.Config.CommandSpecifier);
                return;
            }

            args.Parameters.RemoveAt(0);
            string prefix = string.Join(" ", args.Parameters.Select(p => p));
            if (!players.ContainsKey(args.Player.User.ID))
            {
                args.Player.SendErrorMessage("You are not in a clan!");
                return;
            }
            else if (players[args.Player.User.ID].Rank.Item1 != (int)ClanRank.Founder)
            {
                args.Player.SendErrorMessage("Only clan founders can change the clan's prefix.");
                return;
            }
            else if (prefix.Length > Config.PrefixLength)
            {
                args.Player.SendErrorMessage("Your clan's prefix cannot be longer than {0} characters.", Config.PrefixLength);
                return;
            }
            else
            {
                if (ClanManager.SetClanPrefix(players[args.Player.User.ID].Clan, prefix))
                {
                    args.Player.SendSuccessMessage("Clan prefix set successfully.");
                }
                else
                {
                    args.Player.SendErrorMessage("Something went wrong... Check logs for more details.");
                }
            }
        }

        private static void SetColor(CommandArgs args)
        {
            if (!args.Player.IsLoggedIn)
            {
                args.Player.SendErrorMessage("You are not logged in!");
                return;
            }

            if (args.Parameters.Count != 2)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}clan color <rrr,ggg,bbb>", TShock.Config.CommandSpecifier);
                return;
            }

            if (!players.ContainsKey(args.Player.User.ID))
            {
                args.Player.SendErrorMessage("You are not in a clan!");
                return;
            }
            else if (players[args.Player.User.ID].Rank.Item1 != (int)ClanRank.Founder)
            {
                args.Player.SendErrorMessage("Only clan founders can change the clan's chat color.");
                return;
            }
            else
            {
                byte r, g, b;
                string color = args.Parameters[1];
                string[] ColorArr = color.Split(',');
                if (ColorArr.Length == 3 && byte.TryParse(ColorArr[0], out r) && byte.TryParse(ColorArr[1], out g) && byte.TryParse(ColorArr[2], out b))
                {
                    if (ClanManager.SetClanColor(players[args.Player.User.ID].Clan, color))
                    {
                        args.Player.SendSuccessMessage("Clan chat color set successfully.");
                    }
                    else
                    {
                        args.Player.SendErrorMessage("Something went wrong... Check logs for more details.");
                    }
                }
                else
                    args.Player.SendErrorMessage("Invalid color format!");
            }
        }
    }
}
