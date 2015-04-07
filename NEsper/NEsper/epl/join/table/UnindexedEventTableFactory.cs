///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.epl.join.table
{
    /// <summary>
    /// Factory for simple table of events without an index.
    /// </summary>
    public class UnindexedEventTableFactory : EventTableFactory
    {
        private readonly int _streamNum;

        public UnindexedEventTableFactory(int streamNum)
        {
            _streamNum = streamNum;
        }

        public EventTable[] MakeEventTables()
        {
            return new EventTable[]
            {
                new UnindexedEventTable(_streamNum)
            };
        }

        public Type EventTableType
        {
            get { return typeof(UnindexedEventTable); }
        }

        public String ToQueryPlan()
        {
            return GetType().Name + " StreamNum=" + _streamNum;
        }
    }
}
