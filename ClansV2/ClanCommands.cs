using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TShockAPI;
using TShockAPI.DB;
using ClansV2.Managers;
using ClansV2.Extensions;
using static ClansV2.Clans;

namespace ClansV2
{
    public class ClanCommands
    {
        public static void ClanChatCommand(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("You must enter a valid message.");
                return;
            }
            else if (args.Player.GetPlayerInfo() == null)
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
                args.Player.GetPlayerInfo().Clan.SendClanMessage("(Clan) [{0}] {1}: {2}", args.Player.GetPlayerInfo().Rank.Item2.ToString(), args.Player.Name, message);
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
                default:
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
                case "color":
                    #region Set Clan Color
                    {
                        SetColor(args);
                    }
                    #endregion
                    break;
                case "promote":
                    #region Promote Member
                    {
                        PromoteMember(args);
                    }
                    #endregion
                    break;
                case "demote":
                    #region Demote Member
                    {
                        DemoteMember(args);
                    }
                    #endregion
                    break;
                case "kick":
                    #region Kick Member From Clan
                    {
                        KickMember(args);
                    }
                    #endregion
                    break;
                case "quit":
                case "leave":
                    #region Leave Clan
                    {
                        LeaveClan(args);
                    }
                    #endregion
                    break;
                case "invite":
                    #region Invite Player To Clan
                    {
                        InvitePlayer(args);
                    }
                    #endregion
                    break;
                case "accept":
                    #region Accept Clan Invite
                    {
                        AcceptInvite(args);
                    }
                    #endregion
                    break;
                case "deny":
                case "decline":
                    #region Decline Clan Invite
                    {
                        DeclineInvite(args);
                    }
                    #endregion
                    break;
                case "members":
                    #region List Clan Members
                    {
                        ListMembers(args);
                    }
                    #endregion
                    break;
                case "list":
                    #region List Clans
                    {
                        ListClans(args);
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
                "clan promote <member name> - promotes a player to Leader",
                "clan demote <member name> - demotes a player to Member",
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
            if (args.Parameters.Count < 2)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}clan create <clan name>", TShock.Config.CommandSpecifier);
                return;
            }

            args.Parameters.RemoveAt(0);
            string clanName = string.Join(" ", args.Parameters.Select(p => p));
            if (args.Player.GetPlayerInfo() != null)
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
                if (Clans.ClansDb.AddClan(new Clan() { Name = clanName, Prefix = clanName, MotD = "", ChatColor = "255,255,255" }, args.Player.User.ID))
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
            if (args.Player.GetPlayerInfo() == null)
            {
                args.Player.SendErrorMessage("You are not in a clan!");
                return;
            }
            else if (args.Player.GetPlayerInfo().Rank.Item1 != (int)ClanRank.Founder)
            {
                args.Player.SendErrorMessage("Only clan founders can disband clans!");
                return;
            }
            else
            {
                if (Clans.ClansDb.RemoveClan(args.Player.GetPlayerInfo().Clan))
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
            if (args.Player.GetPlayerInfo() == null)
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
                    if (args.Player.GetPlayerInfo().Rank.Item1 != (int)ClanRank.Founder)
                    {
                        args.Player.SendErrorMessage("Only clan founders can set the clan's MotD!");
                        return;
                    }
                    else
                    {
                        if (Clans.ClansDb.SetClanMotd(args.Player.GetPlayerInfo().Clan, motd))
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
                    if (!string.IsNullOrWhiteSpace(args.Player.GetPlayerInfo().Clan.MotD))
                    {
                        args.Player.SendMessage(args.Player.GetPlayerInfo().Clan.MotD, args.Player.GetPlayerInfo().Clan.ChatColor.ParseColor());
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
            if (args.Parameters.Count < 2)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}clan prefix <new prefix>", TShock.Config.CommandSpecifier);
                return;
            }

            args.Parameters.RemoveAt(0);
            string prefix = string.Join(" ", args.Parameters.Select(p => p));
            if (args.Player.GetPlayerInfo() == null)
            {
                args.Player.SendErrorMessage("You are not in a clan!");
                return;
            }
            else if (args.Player.GetPlayerInfo().Rank.Item1 != (int)ClanRank.Founder)
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
                if (Clans.ClansDb.SetClanPrefix(args.Player.GetPlayerInfo().Clan, prefix))
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
            if (args.Parameters.Count != 2)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}clan color <rrr,ggg,bbb>", TShock.Config.CommandSpecifier);
                return;
            }

            if (args.Player.GetPlayerInfo() == null)
            {
                args.Player.SendErrorMessage("You are not in a clan!");
                return;
            }
            else if (args.Player.GetPlayerInfo().Rank.Item1 != (int)ClanRank.Founder)
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
                    if (Clans.ClansDb.SetClanColor(args.Player.GetPlayerInfo().Clan, color))
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

        private static void PromoteMember(CommandArgs args)
        {
            if (args.Parameters.Count != 2)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}clan promote <player name>", TShock.Config.CommandSpecifier);
                return;
            }

            List<User> userList = TShock.Users.GetUsersByName(args.Parameters[1]);
            if (args.Player.GetPlayerInfo() == null)
            {
                args.Player.SendErrorMessage("You are not in a clan!");
                return;
            }
            else if (args.Player.GetPlayerInfo().Rank.Item1 != (int)ClanRank.Founder)
            {
                args.Player.SendErrorMessage("Only clan founders can promote members!");
                return;
            }
            else if (userList.Count == 0)
            {
                args.Player.SendErrorMessage("No players matched your search string.");
                return;
            }
            else if (userList.Count > 1)
            {
                TShock.Utils.SendMultipleMatchError(args.Player, userList.Select(p => p.Name));
                return;
            }
            else if (Clans.MembersDb.GetMemberByID(userList[0].ID) == null || Clans.MembersDb.GetMemberByID(userList[0].ID).Clan != args.Player.GetPlayerInfo().Clan)
            {
                args.Player.SendErrorMessage("This player is not a member of your clan!");
                return;
            }
            else if (Clans.MembersDb.GetMemberByID(userList[0].ID).Rank.Item1 == (int)ClanRank.Leader)
            {
                args.Player.SendErrorMessage("This player is already a leader.");
                return;
            }
            else
            {
                Clans.MembersDb.SetRank(Clans.MembersDb.GetMemberByID(userList[0].ID), new Tuple<int, string>((int)ClanRank.Leader, ClanRank.Leader.ToString()));
                args.Player.SendInfoMessage("{0} has been promoted to Leader.", userList[0].Name);
            }
        }

        private static void DemoteMember(CommandArgs args)
        {
            if (args.Parameters.Count != 2)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}clan demote <player name>", TShock.Config.CommandSpecifier);
                return;
            }

            List<User> userList = TShock.Users.GetUsersByName(args.Parameters[1]);
            if (args.Player.GetPlayerInfo() == null)
            {
                args.Player.SendErrorMessage("You are not in a clan!");
                return;
            }
            else if (args.Player.GetPlayerInfo().Rank.Item1 != (int)ClanRank.Founder)
            {
                args.Player.SendErrorMessage("Only clan founders can demote members!");
                return;
            }
            else if (userList.Count == 0)
            {
                args.Player.SendErrorMessage("No players matched your search string.");
                return;
            }
            else if (userList.Count > 1)
            {
                TShock.Utils.SendMultipleMatchError(args.Player, userList.Select(p => p.Name));
                return;
            }
            else if (Clans.MembersDb.GetMemberByID(userList[0].ID) == null || Clans.MembersDb.GetMemberByID(userList[0].ID).Clan != args.Player.GetPlayerInfo().Clan)
            {
                args.Player.SendErrorMessage("This player is not a member of your clan!");
                return;
            }
            else if (Clans.MembersDb.GetMemberByID(userList[0].ID).Rank.Item1 != (int)ClanRank.Leader)
            {
                args.Player.SendErrorMessage("This player is not a leader.");
                return;
            }
            else
            {
                Clans.MembersDb.SetRank(Clans.MembersDb.GetMemberByID(userList[0].ID), new Tuple<int, string>((int)ClanRank.Member, ClanRank.Member.ToString()));
                args.Player.SendInfoMessage("{0} has been demoted to Member.", userList[0].Name);
            }
        }

        private static void KickMember(CommandArgs args)
        {
            if (args.Parameters.Count != 2)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}clan kick <member name>", TShock.Config.CommandSpecifier);
                return;
            }

            List<User> userList = TShock.Users.GetUsersByName(args.Parameters[1]);
            if (args.Player.GetPlayerInfo() == null)
            {
                args.Player.SendErrorMessage("You are not in a clan!");
                return;
            }
            else if (args.Player.GetPlayerInfo().Rank.Item1 == (int)ClanRank.Member)
            {
                args.Player.SendErrorMessage("Only clan leaders and founders can kick members!");
                return;
            }
            else if (userList.Count == 0)
            {
                args.Player.SendErrorMessage("No players matched your search string.");
                return;
            }
            else if (userList.Count > 1)
            {
                TShock.Utils.SendMultipleMatchError(args.Player, userList.Select(p => p.Name));
                return;
            }
            else if (Clans.MembersDb.GetMemberByID(userList[0].ID) == null || Clans.MembersDb.GetMemberByID(userList[0].ID).Clan != args.Player.GetPlayerInfo().Clan)
            {
                args.Player.SendErrorMessage("This player is not a part of your clan!");
                return;
            }
            else if (Clans.MembersDb.GetMemberByID(userList[0].ID).Rank.Item1 == (int)ClanRank.Founder)
            {
                args.Player.SendErrorMessage("You can't kick the founder!");
                return;
            }
            else if (Clans.MembersDb.GetMemberByID(userList[0].ID).Rank.Item1 == (int)ClanRank.Leader && args.Player.GetPlayerInfo().Rank.Item1 != (int)ClanRank.Founder)
            {
                args.Player.SendErrorMessage("You can't kick another leader!");
                return;
            }
            else
            {
                Clans.MembersDb.RemoveMember(Clans.MembersDb.GetMemberByID(userList[0].ID), true);
            }
        }

        private static void LeaveClan(CommandArgs args)
        {
            if (args.Player.GetPlayerInfo() == null)
            {
                args.Player.SendErrorMessage("You are not in a clan!");
                return;
            }
            else
            {
                if (args.Player.GetPlayerInfo().Rank.Item1 == (int)ClanRank.Founder)
                {
                    Clans.ClansDb.RemoveClan(args.Player.GetPlayerInfo().Clan);
                }
                else
                {
                    Clans.MembersDb.RemoveMember(args.Player.GetPlayerInfo(), false);
                }
            }
        }

        private static void InvitePlayer(CommandArgs args)
        {
            if (args.Parameters.Count != 2)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: {0}clan invite <player name>", TShock.Config.CommandSpecifier);
                return;
            }

            List<TSPlayer> playerList = TShock.Utils.FindPlayer(args.Parameters[1]);
            if (args.Player.GetPlayerInfo() == null)
            {
                args.Player.SendErrorMessage("You are not in a clan!");
                return;
            }
            else if (args.Player.GetPlayerInfo().Rank.Item1 == (int)ClanRank.Member)
            {
                args.Player.SendErrorMessage("Only clan leaders and founders can invite members!");
                return;
            }
            else if (playerList.Count == 0)
            {
                args.Player.SendErrorMessage("No players matched your search string.");
                return;
            }
            else if (playerList.Count > 1)
            {
                TShock.Utils.SendMultipleMatchError(args.Player, playerList.Select(p => p.Name));
                return;
            }
            else if (!playerList[0].IsLoggedIn)
            {
                args.Player.SendErrorMessage("The player is not logged in.");
                return;
            }
            else if (Clans.MembersDb.GetMemberByID(playerList[0].User.ID) != null)
            {
                args.Player.SendErrorMessage("This player is already in a clan!");
                return;
            }
            else if (Clans.InvitesDb.invites.ContainsKey(playerList[0].User.ID))
            {
                args.Player.SendErrorMessage("This player alredy has a pending invitation.");
                return;
            }
            else
            {
                Clans.InvitesDb.AddInvite(playerList[0].User.ID, args.Player.GetPlayerInfo().Clan.Name);
                playerList[0].SendInfoMessage("You have been invited to join clan: {0}", args.Player.GetPlayerInfo().Clan.Name);
                args.Player.SendSuccessMessage("You have invited {0} to join your clan!", playerList[0].Name);
            }
        }

        private static void AcceptInvite(CommandArgs args)
        {
            if (!args.Player.IsLoggedIn)
            {
                args.Player.SendErrorMessage("You are not logged in!");
                return;
            }

            if (!Clans.InvitesDb.invites.ContainsKey(args.Player.User.ID))
            {
                args.Player.SendErrorMessage("You don't have a pending invitation!");
                return;
            }
            else
            {
                Clans.MembersDb.AddMember(new ClanMember() { UserID = args.Player.User.ID, Clan = Clans.ClansDb.GetClanByName(Clans.InvitesDb.invites[args.Player.User.ID]), Rank = new Tuple<int, string>((int)ClanRank.Member, ClanRank.Member.ToString()) });
                Clans.InvitesDb.RemoveInvite(args.Player.User.ID);
                args.Player.SendSuccessMessage("You have successfully joined the clan!");
            }
        }

        private static void DeclineInvite(CommandArgs args)
        {
            if (!args.Player.IsLoggedIn)
            {
                args.Player.SendErrorMessage("You are not logged in!");
                return;
            }
            
            if (!Clans.InvitesDb.invites.ContainsKey(args.Player.User.ID))
            {
                args.Player.SendErrorMessage("You don't have a pending invitation!");
                return;
            }
            else
            {
                Clans.InvitesDb.RemoveInvite(args.Player.User.ID);
                args.Player.SendSuccessMessage("Clan invite declined successfully.");
            }
        }

        private static void ListMembers(CommandArgs args)
        {
            if (args.Player.GetPlayerInfo() == null)
            {
                args.Player.SendErrorMessage("You are not in a clan!");
                return;
            }
            else
            {
                int pageNum;
                if (!PaginationTools.TryParsePageNumber(args.Parameters, 1, args.Player, out pageNum))
                    return;

                var members = from m in Clans.MembersDb.GetMembersByClan(args.Player.GetPlayerInfo().Clan.Name) orderby TShock.Users.GetUserByID(m.UserID).Name select TShock.Users.GetUserByID(m.UserID).Name;
                PaginationTools.SendPage(args.Player, pageNum, PaginationTools.BuildLinesFromTerms(members), new PaginationTools.Settings()
                {
                    HeaderFormat = "Clan Members ({0}/{1})",
                    FooterFormat = "Type {0}clan members {{0}} for more.".SFormat(TShock.Config.CommandSpecifier)
                });
            }
        }

        private static void ListClans(CommandArgs args)
        {
            int pageNum;
            if (!PaginationTools.TryParsePageNumber(args.Parameters, 1, args.Player, out pageNum))
                return;

            var clanList = from c in Clans.ClansDb.GetClans() orderby c.Name select c.Name;
            PaginationTools.SendPage(args.Player, pageNum, PaginationTools.BuildLinesFromTerms(clanList), new PaginationTools.Settings()
            {
                HeaderFormat = "Clan List ({0}/{1})",
                FooterFormat = "Type {0}clan list {{0}} for more.".SFormat(TShock.Config.CommandSpecifier),
                NothingToDisplayString = "There are currently no clans to list."
            });
        }
    }
}
