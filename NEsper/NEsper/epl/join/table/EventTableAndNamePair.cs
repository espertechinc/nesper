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
    public class EventTableAndNamePair
    {
        public EventTableAndNamePair(EventTable eventTable, String indexName)
        {
            EventTable = eventTable;
            IndexName = indexName;
        }

        public EventTable EventTable { get; private set; }

        public string IndexName { get; private set; }
    }
}