///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.@join.table;

namespace com.espertech.esper.epl.db
{
    /// <summary>Null implementation for a data cache that doesn't ever hit.</summary>
    public class DataCacheNullImpl : DataCache
    {
        public EventTable[] GetCached(Object[] methodParams, int numLookupKeys)
        {
            return null;
        }

        public void PutCached(Object[] methodParams, int numLookupKeys, EventTable[] rows)
        {
        }

        public void Dispose()
        {
        }

        public bool IsActive
        {
            get { return false; }
        }
    }
} // end of namespace