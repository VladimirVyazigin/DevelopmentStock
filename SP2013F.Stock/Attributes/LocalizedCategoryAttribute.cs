using Microsoft.SharePoint.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP2013F.Stock.Attributes
{
    public class LocalizedCategoryAttribute : CategoryAttribute
    {
        private readonly string _resxFileName;

        public LocalizedCategoryAttribute(string key, string resxFileName) : base(key)
        {
            _resxFileName = resxFileName;
        }

        protected override string GetLocalizedString(string value)
        {
            return SPUtility.GetLocalizedString($"$Resources:{value}", _resxFileName,
                (uint)CultureInfo.CurrentUICulture.LCID);
        }
    }
}
