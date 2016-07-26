using System;

namespace ClansV2.Managers
{
	public class ClanMember
	{
		/// <summary>
		/// The player's UserID.
		/// </summary>
		public int UserID { get; set; }

		/// <summary>
		/// The <see cref="Clan"/> object tied to the player.
		/// </summary>
		public Clan Clan { get; set; }

		/// <summary>
		/// The player's clan rank.
		/// </summary>
		public Tuple<int, string> Rank { get; set; }

		/// <summary>
		/// Initializes a new <see cref="ClanMember"/> instance, without any pre-set values.
		/// </summary>
		public ClanMember()
		{
			UserID = 0;
			Clan = null;
			Rank = null;
		}
	}
}
