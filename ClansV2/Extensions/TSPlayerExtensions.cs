using TShockAPI;
using ClansV2.Managers;

namespace ClansV2.Extensions
{
	public static class TSPlayerExtensions
	{
		/// <summary>
		/// Attempts to get the player's clan info from the database.
		/// </summary>
		/// <param name="tsPlayer">The player whose information we want to fetch.</param>
		/// <returns>The player's clan info.</returns>
		public static ClanMember GetPlayerInfo(this TSPlayer tsPlayer)
		{
			if (tsPlayer.User == null)
				return null;

			return ClansV2.Instance.Members.GetMemberByID(tsPlayer.User.ID);
		}
	}
}
