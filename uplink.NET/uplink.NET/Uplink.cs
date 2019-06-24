using System;
using System.Collections.Generic;
using System.Text;

namespace uplink
{
    public class Uplink:IDisposable
    {
        private UplinkRef _uplinkRef = null;

        public Uplink(UplinkConfig uplinkConfig)
        {
            string error;

            _uplinkRef = storj_uplink.new_uplink(uplinkConfig, out error);

            if (!string.IsNullOrEmpty(error))
                throw new ArgumentException(error);
            if (_uplinkRef == null)
                throw new NullReferenceException("No Uplink-reference created");
        }

        public void Dispose()
        {
            if(_uplinkRef != null)
            {
                string error;
                storj_uplink.close_uplink(_uplinkRef, out error);
                _uplinkRef = null;
            }
        }
    }
}
