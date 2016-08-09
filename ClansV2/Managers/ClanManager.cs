using System;
using System.Collections.Generic;
using System.IO;
using System.Data;
using System.Threading.Tasks;
using TShockAPI;
using TShockAPI.DB;
using Mono.Data.Sqlite;
using MySql.Data.MySqlClient;
using ClansV2.Hooks;

namespace ClansV2.Managers
{
	public class ClanManager
	{
		public List<Clan> ClanData = new List<Clan>();

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

			Task.Run(() => LoadClans());
		}

		/// <summary>
		/// Reloads Clans memory.
		/// </summary>
		/// <returns>A Task.</returns>
		public async Task LoadClans()
		{
			List<Clan> clanList = await GetClansAsync();
			ClanData = clanList;
		}

		/// <summary>
		/// Creates a new <see cref="Clan"/> and stores it into the database.
		/// </summary>
		/// <param name="clan">The <see cref="Clan"/> object.</param>
		/// <param name="founderID">The <see cref="Clan"/>'s owner.</param>
		/// <returns>True or false.</returns>
		public bool AddClan(Clan clan, int founderID)
		{
			try
			{
				Task.Run(() => AddClanAsync(clan, founderID));
				return true;
			}
			catch (Exception ex)
			{
				TShock.Log.Error(ex.ToString());
				return false;
			}
		}

		/// <summary>
		/// Creates a new <see cref="Clan"/> and stores it into the database as an asynchronous operation.
		/// </summary>
		/// <param name="clan">The <see cref="Clan"/> object.</param>
		/// <param name="founderID">The <see cref="Clan"/>'s owner.</param>
		/// <returns>True or false.</returns>
		public async Task<bool> AddClanAsync(Clan clan, int founderID)
		{
			return await Task.Run(() =>
			{
				if (GetClanByName(clan.Name) != null)
				{
					return false;
				}

				ClanHooks.OnClanCreated(clan);
				ClanData.Add(clan);
				db.Query("INSERT INTO Clans (Name, Prefix, MotD, ChatColor) VALUES (@0, @1, @2, @3);", clan.Name, clan.Prefix, clan.MotD, clan.ChatColor);
				ClansV2.Instance.Members.AddMember(new ClanMember() { UserID = founderID, Clan = clan, Rank = new Tuple<int, string>((int)ClanRank.Founder, ClanRank.Founder.ToString()) });
				return true;
			});
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
				Task.Run(() => RemoveClanAsync(clan));
				return true;
			}
			catch (Exception ex)
			{
				TShock.Log.Error(ex.ToString());
				return false;
			}
		}

		/// <summary>
		/// Removes a <see cref="Clan"/> from the database as an asynchronous operation.
		/// </summary>
		/// <param name="clan">The <see cref="Clan"/> object.</param>
		/// <returns>True or false.</returns>
		public async Task<bool> RemoveClanAsync(Clan clan)
		{
			return await Task.Run(() =>
			{
				ClanHooks.OnClanDisbanded(clan);
				foreach (ClanMember member in ClansV2.Instance.Members.GetMembersByClan(clan.Name))
				{
					ClansV2.Instance.Members.RemoveMember(member);
				}

				ClanData.Remove(clan);
				db.Query("DELETE FROM Clans WHERE Name=@0;", clan.Name);
				return true;
			});
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
				Task.Run(() => SetClanPrefixAsync(clan, prefix));
				return true;
			}
			catch (Exception ex)
			{
				TShock.Log.Error(ex.ToString());
				return false;
			}
		}

		/// <summary>
		/// Sets the <see cref="Clan"/>'s prefix as an asynchronous operation.
		/// </summary>
		/// <param name="clan">The <see cref="Clan"/> object.</param>
		/// <param name="prefix">The prefix.</param>
		/// <returns>True or false.</returns>
		public async Task<bool> SetClanPrefixAsync(Clan clan, string prefix)
		{
			return await Task.Run(() =>
			{
				clan.Prefix = prefix;
				db.Query("UPDATE Clans SET Prefix=@0 WHERE Name=@1;", prefix, clan.Name);
				return true;
			});
		}

		/// <summary>
		/// Sets the <see cref="Clan"/>'s MotD.
		/// </summary>
		/// <param name="clan">The <see cref="Clan"/> object.</param>
		/// <param name="motd">The MotD.</param>
		/// <returns>True or false.</returns>
		public bool SetClanMotd(Clan clan, string motd)
		{
			try
			{
				Task.Run(() => SetClanMotdAsync(clan, motd));
				return true;
			}
			catch (Exception ex)
			{
				TShock.Log.Error(ex.ToString());
				return false;
			}
		}

		/// <summary>
		/// Sets the <see cref="Clan"/>'s MotD as an asynchronous operation.
		/// </summary>
		/// <param name="clan">The <see cref="Clan"/> object.</param>
		/// <param name="motd">The MotD.</param>
		/// <returns>True or false.</returns>
		public async Task<bool> SetClanMotdAsync(Clan clan, string motd)
		{
			return await Task.Run(() =>
			{
				clan.MotD = motd;
				db.Query("UPDATE Clans SET MotD=@0 WHERE Name=@1;", motd, clan.Name);
				return true;
			});
		}

		/// <summary>
		/// Sets the <see cref="Clan"/>'s ChatColor.
		/// </summary>
		/// <param name="clan">The <see cref="Clan"/> object.</param>
		/// <param name="color">The ChatColor.</param>
		/// <returns>True or false.</returns>
		public bool SetClanColor(Clan clan, string color)
		{
			try
			{
				Task.Run(() => SetClanColorAsync(clan, color));
				return true;
			}
			catch (Exception ex)
			{
				TShock.Log.Error(ex.ToString());
				return false;
			}
		}

		/// <summary>
		/// Sets the <see cref="Clan"/>'s ChatColor as an asynchronous operation.
		/// </summary>
		/// <param name="clan">The <see cref="Clan"/> object.</param>
		/// <param name="color">The ChatColor.</param>
		/// <returns>True or false.</returns>
		public async Task<bool> SetClanColorAsync(Clan clan, string color)
		{
			return await Task.Run(() =>
			{
				clan.ChatColor = color;
				db.Query("UPDATE Clans SET ChatColor=@0 WHERE Name=@1;", color, clan.Name);
				return true;
			});
		}

		/// <summary>
		/// Returns a <see cref="Clan"/> object with it's set values.
		/// </summary>
		/// <param name="clan">The <see cref="Clan"/> object.</param>
		/// <param name="reader">The QueryResult from which we get the <see cref="Clan"/>'s values.</param>
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
			//using (QueryResult reader = db.QueryReader("SELECT * FROM Clans WHERE Name=@0;", name))
			//{
			//	if (reader.Read())
			//	{
			//		return LoadClanFromResult(new Clan(), reader);
			//	}
			//}

			//return null;

			return ClanData.Find(c => c.Name == name);
		}

		/// <summary>
		/// Returns a list of all <see cref="Clan"/> objects from the database.
		/// </summary>
		/// <returns>A list of <see cref="Clan"/> objects.</returns>
		[Obsolete("Use ClanManager.ClanData instead.")]
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

		/// <summary>
		/// Represents an asynchronous operation which returns a list of <see cref="Clan"/>s from the database.
		/// </summary>
		/// <returns>A Task.</returns>
		public Task<List<Clan>> GetClansAsync()
		{
			return Task.Run(() => 
			{
				var clans = new List<Clan>();
				using (QueryResult reader = db.QueryReader("SELECT * FROM Clans"))
				{
					while (reader.Read())
					{
						clans.Add(LoadClanFromResult(new Clan(), reader));
					}
				}

				return clans;
			});
		}
	}
}
