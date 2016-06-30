using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace ClansV2
{
	public static class Permissions
	{
		[Description("Allows usage of basic /clan commands")]
		public static readonly string ClansUse = "clans.use";

		[Description("Allows usage of /clan create")]
		public static readonly string ClanCreate = "clans.create";

		[Description("Allows usage of /clan chat")]
		public static readonly string ClanChat = "clans.chat";
	}
}