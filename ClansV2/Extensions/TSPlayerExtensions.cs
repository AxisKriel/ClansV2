using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TShockAPI;
using ClansV2.Managers;

namespace ClansV2.Extensions
{
    public static class TSPlayerExtensions
    {
        public static ClanMember GetPlayerInfo(this TSPlayer tsPlayer)
        {
            if (tsPlayer.User == null)
                return null;

            //if (!tsPlayer.ContainsData("Clans_Data"))
            //    tsPlayer.SetData("Clans_Data", Clans.MembersDb.GetMemberByID(tsPlayer.User.ID));
            //return tsPlayer.GetData<ClanMember>("Clans_Data");

            return Clans.MembersDb.GetMemberByID(tsPlayer.User.ID);
        }
    }
}
