using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClansV2.Extensions
{
    public static class StringExtensions
    {
        public static Color ParseColor(this string str)
        {
            byte r, g, b;
            string[] color = str.Split(',');

            if (color.Length == 3 && byte.TryParse(color[0], out r) && byte.TryParse(color[1], out g) && byte.TryParse(color[2], out b))
                return new Color(r, g, b);
            else
                throw new Exception("[Clans] Cannot parse from string to Color");
        }
    }
}
