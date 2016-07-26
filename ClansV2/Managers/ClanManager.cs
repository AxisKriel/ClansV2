using System;
using System.Collections.Generic;
using System.IO;
using System.Data;
using TShockAPI;
using TShockAPI.DB;
using Mono.Data.Sqlite;
using MySql.Data.MySqlClient;
using ClansV2.Hooks;

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
				new SqlColumn("Name", MySqlDbType.VarChar, 50),
				new SqlColumn("Prefix", MySqlDbType.VarChar, 50),
				new SqlColumn("MotD", MySqlDbType.VarChar, 50),
				new SqlColumn("ChatColor", MySqlDbType.VarChar, 50)));
		}

		/// <summary>
		/// Creates a new <see cref="Clan"/> and stores it into the database.
		/// </summary>
		/// <param name="clan">The <see cref="Clan"/> object.</param>
		/// <param name="FounderID">The <see cref="Clan"/>'s owner.</param>
		/// <returns>True or false.</returns>
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

		/// <summary>
		/// Removes a <see cref="Clan"/> from the database.
		/// </summary>
		/// <param name="clan">The <see cref="Clan"/> object.</param>
		/// <returns>True or false.</returns>
		public bool RemoveClan(Clan clan)
		{
			try
			{
				ClanHooks.OnClanDisbanded(clan);
				foreach (ClanMember member in Clans.MembersDb.GetMembersByClan(clan.Name))
				{
					Clans.MembersDb.RemoveMember(member);
				}

				db.Query("DELETE FROM Clans WHERE Name=@0;", clan.Name);

				return true;
			}
			catch (Exception ex)
			{
				TShock.Log.Error(ex.ToString());
				return false;
			}
		}

		/// <summary>
		/// Sets the <see cref="Clan"/>'s prefix.
		/// </summary>
		/// <param name="clan">The <see cref="Clan"/> object.</param>
		/// <param name="prefix">The prefix.</param>
		/// <returns>True or false.</returns>
		public bool SetClanPrefix(Clan clan, string prefix)
		{
			try
			{
				clan.Prefix = prefix;
				db.Query("UPDATE Clans SET Prefix=@0 WHERE Name=@1;", prefix, clan.Name);
				return true;
			}
			catch (Exception ex)
			{
				TShock.Log.Error(ex.ToString());
				return false;
			}
		}

		/// <summary>
		/// Sets the <see cref="Clan"/>'s MotD.
		/// </summary>
		/// <param name="clan">The <see cref="Clan"/> object.</param>
		/// <param name="motd">The <see cref="Clan"/>'s MotD.</param>
		/// <returns>True or false.</returns>
		public bool SetClanMotd(Clan clan, string motd)
		{
			try
			{
				clan.MotD = motd;
				db.Query("UPDATE Clans SET MotD=@0 WHERE Name=@1;", motd, clan.Name);
				return true;
			}
			catch (Exception ex)
			{
				TShock.Log.Error(ex.ToString());
				return false;
			}
		}

		/// <summary>
		/// Sets the <see cref="Clan"/>'s ChatColor.
		/// </summary>
		/// <param name="clan">The <see cref="Clan"/> object.</param>
		/// <param name="color">The <see cref="Clan"/>'s ChatColor.</param>
		/// <returns>True or false.</returns>
		public bool SetClanColor(Clan clan, string color)
		{
			try
			{
				clan.ChatColor = color;
				db.Query("UPDATE Clans SET ChatColor=@0 WHERE Name=@1;", color, clan.Name);
				return true;
			}
			catch (Exception ex)
			{
				TShock.Log.Error(ex.ToString());
				return false;
			}
		}

		/// <summary>
		/// Returns a <see cref="Clan"/> object with it's set values.
		/// </summary>
		/// <param name="clan">The <see cref="Clan"/> object.</param>
		/// <param name="reader">The QueryResult from which we get the <see cref="Clan"/>'s values."/></param>
		/// <returns>A <see cref="Clan"/> object.</returns>
		internal static Clan LoadClanFromResult(Clan clan, QueryResult reader)
		{
			clan.Name = reader.Get<string>("Name");
			clan.Prefix = reader.Get<string>("Prefix");
			clan.MotD = reader.Get<string>("MotD");
			clan.ChatColor = reader.Get<string>("ChatColor");
			return clan;
		}

		/// <summary>
		/// Attempts to return a <see cref="Clan"/> object matching the <paramref name="name"/>.
		/// </summary>
		/// <param name="name">The name to search with.</param>
		/// <returns>A <see cref="Clan"/> object.</returns>
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

		/// <summary>
		/// Returns a list of all <see cref="Clan"/> objects from the database.
		/// </summary>
		/// <returns>A list of <see cref="Clan"/> objects.</returns>
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
