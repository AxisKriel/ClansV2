using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using System.Threading.Tasks;
using TShockAPI;
using TShockAPI.DB;
using Mono.Data.Sqlite;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using ClansV2.Hooks;
using ClansV2.Extensions;
using static ClansV2.Clans;

namespace ClansV2.Managers
{
    public class ClanManager
    {
        private static IDbConnection db;
        public void ConnectDB()
        {
            switch (TShock.Config.StorageType.ToLower())
            {
                case "mysql":
                    string[] dbHost = TShock.Config.MySqlHost.Split(':');
                    db = new MySqlConnection()
                    {
                        ConnectionString = string.Format("Server={0}; Port={1}; Database={2}; Uid={3}; Pwd={4};",
                            dbHost[0],
                            dbHost.Length == 1 ? "3306" : dbHost[1],
                            TShock.Config.MySqlDbName,
                            TShock.Config.MySqlUsername,
                            TShock.Config.MySqlPassword)

                    };
                    break;

                case "sqlite":
                    string sql = Path.Combine(TShock.SavePath, "tshock.sqlite");
                    db = new SqliteConnection(string.Format("uri=file://{0},Version=3", sql));
                    break;
            }

            SqlTableCreator sqlcreator = new SqlTableCreator(db, db.GetSqlType() == SqlType.Sqlite ? (IQueryBuilder)new SqliteQueryCreator() : new MysqlQueryCreator());

            sqlcreator.EnsureTableStructure(new SqlTable("Clans",
                new SqlColumn("Name", MySqlDbType.VarChar),
                new SqlColumn("Prefix", MySqlDbType.VarChar),
                new SqlColumn("MotD", MySqlDbType.VarChar),
                new SqlColumn("ChatColor", MySqlDbType.VarChar)));
        }

        public bool AddClan(Clan clan, int FounderID)
        {
            if (GetClanByName(clan.Name) != null)
            {
                return false;
            }

            try
            {
                ClanHooks.OnClanCreated(clan);
                db.Query("INSERT INTO Clans (Name, Prefix, MotD, ChatColor) VALUES (@0, @1, @2, @3);", clan.Name, clan.Prefix, clan.MotD, clan.ChatColor);
                Clans.MembersDb.AddMember(new ClanMember() { UserID = FounderID, Clan = clan, Rank = new Tuple<int, string>((int)ClanRank.Founder, ClanRank.Founder.ToString()) });
                return true;
            }
            catch (Exception ex)
            {
                TShock.Log.Error(ex.ToString());
                return false;
            }
        }

        public bool RemoveClan(Clan clan)
        {
            try
            {
                ClanHooks.OnClanDisbanded(clan);
                db.Query("DELETE FROM Clans WHERE Name=@0;", clan.Name);
                foreach (ClanMember member in Clans.MembersDb.GetMembersByClan(clan.Name))
                {
                    Clans.MembersDb.RemoveMember(member);
                }

                return true;
            }
            catch (Exception ex)
            {
                TShock.Log.Error(ex.ToString());
                return false;
            }
        }

        public bool SetClanPrefix(Clan clan, string prefix)
        {
            try
            {
                clan.Prefix = prefix;
                db.Query("UPDATE Clans SET Prefix=@0 WHERE Name=@1;", prefix, clan.Name);

                foreach (int key in players.Keys.ToList())
                {
                    if (players[key].Clan == clan)
                    {
                        players[key] = Clans.MembersDb.GetMemberByID(key);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                TShock.Log.Error(ex.ToString());
                return false;
            }
        }

        public bool SetClanMotd(Clan clan, string motd)
        {
            try
            {
                clan.MotD = motd;
                db.Query("UPDATE Clans SET MotD=@0 WHERE Name=@1;", motd, clan.Name);

                foreach (int key in players.Keys.ToList())
                {
                    if (players[key].Clan == clan)
                    {
                        players[key] = Clans.MembersDb.GetMemberByID(key);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                TShock.Log.Error(ex.ToString());
                return false;
            }
        }

        public bool SetClanColor(Clan clan, string color)
        {
            try
            {
                clan.ChatColor = color;
                db.Query("UPDATE Clans SET ChatColor=@0 WHERE Name=@1;", color, clan.Name);

                foreach (int key in players.Keys.ToList())
                {
                    if (players[key].Clan == clan)
                    {
                        players[key] = Clans.MembersDb.GetMemberByID(key);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                TShock.Log.Error(ex.ToString());
                return false;
            }
        }

        internal static Clan LoadClanFromResult(Clan clan, QueryResult reader)
        {
            clan.Name = reader.Get<string>("Name");
            clan.Prefix = reader.Get<string>("Prefix");
            clan.MotD = reader.Get<string>("MotD");
            clan.ChatColor = reader.Get<string>("ChatColor");
            return clan;
        }

        public Clan GetClanByName(string name)
        {
            using (QueryResult reader = db.QueryReader("SELECT * FROM Clans WHERE Name=@0;", name))
            {
                if (reader.Read())
                {
                    return LoadClanFromResult(new Clan(), reader);
                }
            }

            return null;
        }

        public List<Clan> GetClans()
        {
            List<Clan> clans = new List<Clan>();
            using (QueryResult reader = db.QueryReader("SELECT * FROM Clans"))
            {
                while (reader.Read())
                {
                    clans.Add(LoadClanFromResult(new Clan(), reader));
                }
            }

            return clans;
        }
    }
}
