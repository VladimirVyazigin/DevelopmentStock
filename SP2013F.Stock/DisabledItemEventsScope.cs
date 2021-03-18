using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP2013F.Stock
{
    public class DisabledItemEventsScope : SPItemEventReceiver, IDisposable
    {
        private readonly bool _oldValue;

        public DisabledItemEventsScope()
        {
            _oldValue = EventFiringEnabled;
            EventFiringEnabled = false;
        }

        #region IDisposable Members

        public void Dispose()
        {
            EventFiringEnabled = _oldValue;
        }

        #endregion
    }
}
