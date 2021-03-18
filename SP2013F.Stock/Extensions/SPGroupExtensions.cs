using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP2013F.Stock.Extensions
{
    public static class SPGroupExtensions
    {
        public static void Clear(this SPGroup group)
        {
            var users = group.Users;
            foreach (SPUser user in users)
                group.RemoveUser(user);
        }

        public static void AddUsers(this SPGroup group, params SPUser[] users)
        {
            foreach (var user in users)
                group.AddUser(user);
        }
    }
}
