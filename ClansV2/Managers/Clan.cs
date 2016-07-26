using System.Linq;
using TShockAPI;
using ClansV2.Extensions;

namespace ClansV2.Managers
{
	public class Clan
	{
		/// <summary>
		/// Gets or sets the <see cref="Clan"/>'s name.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the <see cref="Clan"/>'s prefix.
		/// </summary>
		public string Prefix { get; set; }

		/// <summary>
		/// Gets or sets the <see cref="Clan"/>'s MotD.
		/// </summary>
		public string MotD { get; set; }

		/// <summary>
		/// Gets or sets the <see cref="Clan"/>'s ChatColor.
		/// </summary>
		public string ChatColor { get; set; }

		/// <summary>
		/// Compares two <see cref="Clan"/>s.
		/// </summary>
		/// <param name="clan"></param>
		/// <param name="clan2"></param>
		/// <returns>True or false.</returns>
		public static bool operator ==(Clan clan, Clan clan2)
		{
			if (ReferenceEquals(clan, clan2))
			{
				return true;
			}

			if ((object)clan == null || (object)clan2 == null)
			{
				return false;
			}

			return clan.Name == clan2.Name;
		}

		/// <summary>
		/// Compares two <see cref="Clan"/>s.
		/// </summary>
		/// <param name="clan"></param>
		/// <param name="clan2"></param>
		/// <returns>True or false.</returns>
		public static bool operator !=(Clan clan, Clan clan2)
		{
			return !(clan == clan2);
		}

		/// <summary>
		/// Prints a message to the <see cref="Clan"/>.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="args">The message parameters.</param>
		public void SendClanMessage(string message, params object[] args)
		{
			foreach (TSPlayer tsplr in TShock.Players.Where(tsplr => tsplr != null))
			{
				if (tsplr.GetPlayerInfo() != null && tsplr.GetPlayerInfo().Clan == this)
				{
					tsplr.SendMessage(string.Format(message, args), ChatColor.ParseColor());
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Clan"/> class, without any pre-set values.
		/// </summary>
		public Clan()
		{
			Name = "";
			Prefix = "";
			MotD = "";
			ChatColor = "";
		}
	}
}
