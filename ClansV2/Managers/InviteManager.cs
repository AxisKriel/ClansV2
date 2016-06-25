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
using static ClansV2.Clans;

namespace ClansV2.Managers
{
    public class InviteManager
    {
        public Dictionary<int, string> Invites = new Dictionary<int, string>();

        private static IDbConnection db;
        internal void ConnectDB()
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

            sqlcreator.EnsureTableStructure(new SqlTable("ClanInvites",
                new SqlColumn("UserID", MySqlDbType.Int32),
                new SqlColumn("Clan", MySqlDbType.VarChar, 50)));
        }

        public void AddInvite(int userID, string clan)
        {
            Invites.Add(userID, clan);
            db.Query("INSERT INTO ClanInvites (UserID, Clan) VALUES (@0, @1);", userID.ToString(), clan);
        }

        public void RemoveInvite(int userID)
        {
            Invites.Remove(userID);
            db.Query("DELETE FROM ClanInvites WHERE UserID=@0;", userID.ToString());
        }
    }
}
