using System;

namespace ClansV2.Extensions
{
	public static class StringExtensions
	{
		/// <summary>
		/// Attempts to parse a color from the string.
		/// </summary>
		/// <param name="str">The string to parse. Must be in rrr,ggg,bbb format.</param>
		/// <returns>A <see cref="Color"/> object.</returns>
		public static Color ParseColor(this string str)
		{
			byte r, g, b;
			string[] color = str.Split(',');

			if (color.Length == 3 && byte.TryParse(color[0], out r) && byte.TryParse(color[1], out g) && byte.TryParse(color[2], out b))
				return new Color(r, g, b);
			else
				throw new Exception("[Clans] Cannot parse Color from string");
		}
	}
}
