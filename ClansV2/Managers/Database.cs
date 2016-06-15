using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using TShockAPI;
using TShockAPI.DB;
using Mono.Data.Sqlite;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace ClansV2.Managers
{
    public class Database
    {
        private static IDbConnection db;
        internal static void ConnectDB()
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

            sqlcreator.EnsureTableStructure(new SqlTable("ClanMembers",
                new SqlColumn("UserID", MySqlDbType.Int32),
                new SqlColumn("Clan", MySqlDbType.VarChar),
                new SqlColumn("Rank", MySqlDbType.VarChar)));
        }

        internal static Clan LoadClanFromResult(Clan clan, QueryResult reader)
        {
            clan.Name = reader.Get<string>("Name");
            clan.Prefix = reader.Get<string>("Prefix");
            clan.MotD = reader.Get<string>("MotD");
            clan.ChatColor = reader.Get<string>("ChatColor");
            return clan;
        }

        internal static Clan LoadClanFromResult(Clan clan, string name)
        {
            using (QueryResult reader = db.QueryReader("SELECT * FROM Clans WHERE Name=@0;", name))
            {
                if (reader.Read())
                {
                    return LoadClanFromResult(clan, reader);
                }
            }

            return null;
        }

        internal static ClanMember LoadMemberFromResult(ClanMember member, QueryResult reader)
        {
            member.UserID = reader.Get<int>("UserID");
            member.Clan = LoadClanFromResult(new Clan(), reader.Get<string>("Clan"));
            member.Rank = JsonConvert.DeserializeObject<Tuple<int, string>>(reader.Get<string>("Rank"));
            return member;
        }

        internal static ClanMember LoadMemberFromResult(ClanMember member, int ID)
        {
            using (QueryResult reader = db.QueryReader("SELECT * FROM ClanMembers WHERE ID=@0;", ID.ToString()))
            {
                if (reader.Read())
                {
                    return LoadMemberFromResult(member, reader);
                }
            }

            return null;
        }

        internal static List<Clan> GetClans()
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
