///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.index.@base;

namespace com.espertech.esper.common.@internal.epl.historical.datacache
{
    /// <summary>
    ///     Null implementation for a data cache that doesn't ever hit.
    /// </summary>
    public class HistoricalDataCacheNullImpl : HistoricalDataCache
    {
        public EventTable[] GetCached(object methodParams)
        {
            return null;
        }

        public void Put(
            object methodParams,
            EventTable[] rows)
        {
        }

        public bool IsActive => false;

        public void Destroy()
        {
        }
    }
} // end of namespace