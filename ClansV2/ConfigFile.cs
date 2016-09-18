using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;

namespace ClansV2
{
	public class ConfigFile
	{
		/// <summary>
		/// Gets or sets the ChatFormat.
		/// </summary>
		public string ChatFormat { get; set; }

		/// <summary>
		/// Whether clan colors will be used instead of the original group colors.
		/// </summary>
		public bool ChatColorsEnabled { get; set; }

		/// <summary>
		/// The maximum clan name length.
		/// </summary>
		public int NameLength { get; set; }

		/// <summary>
		/// The maximum clan prefix length.
		/// </summary>
		public int PrefixLength { get; set; }

		public string PrefixFormat { get; set; }

		public string SuffixFormat { get; set; }

		/// <summary>
		/// Reads the <see cref="ConfigFile"/> from the given path.
		/// If the file doesn't exist it creates and writes to a new one.
		/// </summary>
		/// <param name="path">The path to read from.</param>
		/// <returns>The <see cref="ConfigFile"/> object.</returns>
		public static ConfigFile Read(string path)
		{
			if (!File.Exists(path))
			{
				ConfigFile config = new ConfigFile();
				File.WriteAllText(path, JsonConvert.SerializeObject(config, Formatting.Indented));
				return config;
			}

			return JsonConvert.DeserializeObject<ConfigFile>(File.ReadAllText(path));
		}
	}
}
