using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP2013F.Stock.Extensions
{
    public static class SPFolderExtensions
    {
        public static SPFolder EnsureSubFolder(this SPFolder folder, string subFolderName)
        {
            var existingFolders = folder.SubFolders.OfType<SPFolder>().ToList();
            var f = existingFolders.FirstOrDefault(x => x.Name.Equals(subFolderName, StringComparison.InvariantCultureIgnoreCase));
            if (f != null) return f;
            f = folder.SubFolders.Add(subFolderName);
            f.Update();
            return f;
        }

        public static SPFolder FindSubFolder(this SPFolder folder, string subFolderName)
        {
            var existingFolders = folder.SubFolders.OfType<SPFolder>().ToList();
            return existingFolders.FirstOrDefault(x => x.Name.Equals(subFolderName, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
