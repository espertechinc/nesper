///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.schedule
{
    /// <summary>Service for holding expiration dates to adjust. </summary>
    public class ScheduleAdjustmentService
    {
        private ICollection<ScheduleAdjustmentCallback> _callbacks;

        /// <summary>Add a callback </summary>
        /// <param name="callback">to add</param>
        public void AddCallback(ScheduleAdjustmentCallback callback)
        {
            lock (this)
            {
                if (_callbacks == null)
                {
                    _callbacks = new HashSet<ScheduleAdjustmentCallback>();
                }
                _callbacks.Add(callback);
            }
        }

        /// <summary>Make callbacks to adjust expiration dates. </summary>
        /// <param name="delta">to adjust for</param>
        public void Adjust(long delta)
        {
            if (_callbacks == null)
            {
                return;
            }
            foreach (ScheduleAdjustmentCallback callback in _callbacks)
            {
                callback.Adjust(delta);
            }
        }

        public void RemoveCallback(ScheduleAdjustmentCallback callback)
        {
            if (_callbacks == null)
            {
                return;
            }
            _callbacks.Remove(callback);
        }
    }
}
