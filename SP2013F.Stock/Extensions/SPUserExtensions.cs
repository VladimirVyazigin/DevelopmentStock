using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP2013F.Stock.Extensions
{
    public static class SPUserExtensions
    {
        public static bool IsInGroup(this SPUser user, string groupName)
        {
            return user.Groups.OfType<SPGroup>().Any(x => x.Name == groupName);
        }
    }
}
