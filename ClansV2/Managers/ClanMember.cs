using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClansV2.Managers
{
    public class ClanMember
    {
        public int UserID { get; set; }
        public Tuple<int, string> Rank { get; set; }
        public Clan Clan { get; set; }

        public ClanMember()
        {

        }
    }
}
