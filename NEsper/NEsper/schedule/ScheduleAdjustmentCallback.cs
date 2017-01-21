///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.schedule
{
    /// <summary>Callback for views that adjust an expiration date on event objects. </summary>
    public interface ScheduleAdjustmentCallback
    {
        /// <summary>Adjust expiration date. </summary>
        /// <param name="delta">to adjust</param>
        void Adjust(long delta);
    }
}