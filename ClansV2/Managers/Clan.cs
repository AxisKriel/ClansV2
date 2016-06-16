using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClansV2.Managers
{
    public class Clan
    {
        public string Name { get; set; }
        public string Prefix { get; set; }
        public string MotD { get; set; }
        public string ChatColor { get; set; }

        public Clan()
        {
            Name = "";
            Prefix = "";
            MotD = "";
            ChatColor = "";
        }
    }
}
