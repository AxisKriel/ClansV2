using System;
using System.Collections.Generic;
using System.IO;
using System.Data;
using System.Threading.Tasks;
using TShockAPI;
using TShockAPI.DB;
using Mono.Data.Sqlite;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using ClansV2.Hooks;

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

		/// <summary>
		/// Inserts the <paramref name="member"/> into the database.
		/// </summary>
		/// <param name="member">The <see cref="ClanMember"/> to store.</param>
		public void AddMember(ClanMember member)
		{
			ClanHooks.OnClanJoined(member.Clan, member);
			db.Query("INSERT INTO ClanMembers (UserID, Clan, Rank) VALUES (@0, @1, @2);", member.UserID.ToString(), member.Clan.Name, JsonConvert.SerializeObject(member.Rank, Formatting.Indented));
		}

		/// <summary>
		/// Inserts the <paramref name="member"/> into the database as an asynchronous operation.
		/// </summary>
		/// <param name="member">The <see cref="ClanMember"/> to store.</param>
		public Task AddMemberAsync(ClanMember member)
		{
			return Task.Run(() => 
			{
				ClanHooks.OnClanJoined(member.Clan, member);
				db.Query("INSERT INTO ClanMembers (UserID, Clan, Rank) VALUES (@0, @1, @2);", member.UserID.ToString(), member.Clan.Name, JsonConvert.SerializeObject(member.Rank, Formatting.Indented));
			});
		}

		/// <summary>
		/// Removes the <paramref name="member"/> from the database.
		/// </summary>
		/// <param name="member">The <see cref="ClanMember"/> object to remove.</param>
		/// <param name="kick">Whether the <paramref name="member"/> was kicked or not.</param>
		public void RemoveMember(ClanMember member, bool kick = false)
		{
			db.Query("DELETE FROM ClanMembers WHERE UserID=@0;", member.UserID.ToString());
			ClanHooks.OnClanLeft(member.Clan, member, kick);
		}

		/// <summary>
		/// Removes the <paramref name="member"/> from the database as an asynchronous operation.
		/// </summary>
		/// <param name="member">The <see cref="ClanMember"/> object to remove.</param>
		/// <param name="kick">Whether the <paramref name="member"/> was kicked or not.</param>
		public Task RemoveMemberAsync(ClanMember member, bool kick = false)
		{
			return Task.Run(() => 
			{
				db.Query("DELETE FROM ClanMembers WHERE UserID=@0;", member.UserID.ToString());
				ClanHooks.OnClanLeft(member.Clan, member, kick);
			});
		}

		/// <summary>
		/// Sets the <paramref name="member"/>'s rank.
		/// </summary>
		/// <param name="member">The <see cref="ClanMember"/> object to modify.</param>
		/// <param name="rank">The new rank.</param>
		public void SetRank(ClanMember member, ClanRank rank)
		{
			member.Rank = new Tuple<int, string>((int)rank, rank.ToString());
			db.Query("UPDATE ClanMembers SET Rank=@0 WHERE UserID=@1;", JsonConvert.SerializeObject(member.Rank, Formatting.Indented), member.UserID.ToString());
		}

		/// <summary>
		/// Sets the <paramref name="member"/>'s rank as an asynchronous operation.
		/// </summary>
		/// <param name="member">The <see cref="ClanMember"/> object to modify.</param>
		/// <param name="rank">The new rank.</param>
		public Task SetRankAsync(ClanMember member, ClanRank rank)
		{
			return Task.Run(() =>
			{
				member.Rank = new Tuple<int, string>((int)rank, rank.ToString());
				db.Query("UPDATE ClanMembers SET Rank=@0 WHERE UserID=@1;", JsonConvert.SerializeObject(member.Rank, Formatting.Indented), member.UserID.ToString());
			});
		}

		/// <summary>
		/// Returns a <see cref="ClanMember"/> object with it's set values.
		/// </summary>
		/// <param name="member">The <see cref="Clan"/> object.</param>
		/// <param name="reader">The QueryResult from which we get the <see cref="ClanMember"/>'s values.</param>
		/// <returns>A <see cref="ClanMember"/> object.</returns>
		internal static ClanMember LoadMemberFromResult(ClanMember member, QueryResult reader)
		{
			member.UserID = reader.Get<int>("UserID");
			member.Clan = ClansV2.Instance.Clans.GetClanByName(reader.Get<string>("Clan"));
			member.Rank = JsonConvert.DeserializeObject<Tuple<int, string>>(reader.Get<string>("Rank"));
			return member;
		}

		/// <summary>
		/// Returns a <see cref="ClanMember"/> object matching the <paramref name="ID"/>.
		/// </summary>
		/// <param name="ID">The ID to match with.</param>
		/// <returns>A <see cref="ClanMember"/> object.</returns>
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

		/// <summary>
		/// Returns a list of <see cref="ClanMember"/> objects matching the <paramref name="clanName"/>.
		/// </summary>
		/// <param name="clanName">The clan name to match with.</param>
		/// <returns>A list of <see cref="ClanMember"/> objects.</returns>
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
