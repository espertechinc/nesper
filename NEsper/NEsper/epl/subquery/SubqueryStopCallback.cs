///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.join.table;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.subquery
{
    /// <summary>
    /// : a stop callback for use with subqueries to clear their indexes when a statement is stopped.
    /// </summary>
    public class SubqueryStopCallback : StopCallback
    {
        private readonly EventTable[] _eventIndex;

        /// <summary>Ctor. </summary>
        /// <param name="eventIndex">index to clear</param>
        public SubqueryStopCallback(EventTable[] eventIndex)
        {
            _eventIndex = eventIndex;
        }

        // Clear out index on statement stop
        public void Stop()
        {
            if (_eventIndex != null)
            {
                foreach (EventTable table in _eventIndex)
                {
                    table.Destroy();
                }
            }
        }
    }
}
