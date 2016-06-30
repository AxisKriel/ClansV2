﻿using System;
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
using ClansV2.Hooks;
using static ClansV2.Clans;

namespace ClansV2.Managers
{
	public class MemberManager
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

			sqlcreator.EnsureTableStructure(new SqlTable("ClanMembers",
				new SqlColumn("UserID", MySqlDbType.Int32),
				new SqlColumn("Clan", MySqlDbType.VarChar, 50),
				new SqlColumn("Rank", MySqlDbType.VarChar, 50)));
		}

		public void AddMember(ClanMember member)
		{
			ClanHooks.OnClanJoined(member.Clan, member);
			db.Query("INSERT INTO ClanMembers (UserID, Clan, Rank) VALUES (@0, @1, @2);", member.UserID.ToString(), member.Clan.Name, JsonConvert.SerializeObject(member.Rank, Formatting.Indented));
		}

		public void RemoveMember(ClanMember member, bool kick = false)
		{
			db.Query("DELETE FROM ClanMembers WHERE UserID=@0;", member.UserID.ToString());
			ClanHooks.OnClanLeft(member.Clan, member, kick);
		}

		public void SetRank(ClanMember member, ClanRank rank)
		{
			member.Rank = new Tuple<int, string>((int)rank, rank.ToString());
			db.Query("UPDATE ClanMembers SET Rank=@0 WHERE UserID=@1;", JsonConvert.SerializeObject(member.Rank, Formatting.Indented), member.UserID.ToString());
		}

		internal static ClanMember LoadMemberFromResult(ClanMember member, QueryResult reader)
		{
			member.UserID = reader.Get<int>("UserID");
			member.Clan = Clans.ClansDb.GetClanByName(reader.Get<string>("Clan"));
			member.Rank = JsonConvert.DeserializeObject<Tuple<int, string>>(reader.Get<string>("Rank"));
			return member;
		}

		public ClanMember GetMemberByID(int ID)
		{
			using (QueryResult reader = db.QueryReader("SELECT * FROM ClanMembers WHERE UserID=@0;", ID.ToString()))
			{
				if (reader.Read())
				{
					return LoadMemberFromResult(new ClanMember(), reader);
				}
			}

			return null;
		}

		public List<ClanMember> GetMembersByClan(string clanName)
		{
			List<ClanMember> members = new List<ClanMember>();
			using (QueryResult reader = db.QueryReader("SELECT * FROM ClanMembers WHERE Clan=@0;", clanName))
			{
				while (reader.Read())
				{
					members.Add(LoadMemberFromResult(new ClanMember(), reader));
				}
			}

			return members;
		}
	}
}
