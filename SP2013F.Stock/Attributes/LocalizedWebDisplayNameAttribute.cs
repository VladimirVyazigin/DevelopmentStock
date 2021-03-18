using Microsoft.SharePoint.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls.WebParts;

namespace SP2013F.Stock.Attributes
{
    public class LocalizedWebDisplayNameAttribute : WebDisplayNameAttribute
    {
        private readonly string _resxFileName;
        private bool _isLocalized;

        public LocalizedWebDisplayNameAttribute(string key, string resxFileName) : base(key)
        {
            _resxFileName = resxFileName;
        }

        public override string DisplayName
        {
            get
            {
                if (_isLocalized) return base.DisplayName;
                DisplayNameValue = SPUtility.GetLocalizedString($"$Resources:{base.DisplayName}", _resxFileName,
                    (uint)CultureInfo.CurrentUICulture.LCID);
                _isLocalized = true;
                return base.DisplayName;
            }
        }
    }
}
