///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.@join.table;

namespace com.espertech.esper.epl.join.pollindex
{
    /// <summary>
    /// Strategy for building an index out of poll-results knowing the properties to base the index on.
    /// </summary>
    public class PollResultIndexingStrategySorted : PollResultIndexingStrategy
    {
        private readonly int _streamNum;
        private readonly EventType _eventType;
        private readonly String _propertyName;
        private readonly Type _coercionType;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="streamNum">is the stream number of the indexed stream</param>
        /// <param name="eventType">is the event type of the indexed stream</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="coercionType">Type of the coercion.</param>
        public PollResultIndexingStrategySorted(int streamNum, EventType eventType, String propertyName, Type coercionType)
        {
            _streamNum = streamNum;
            _eventType = eventType;
            _propertyName = propertyName;
            _coercionType = coercionType;
        }
    
        public EventTable[] Index(IList<EventBean> pollResult, bool isActiveCache, StatementContext statementContext)
        {
            if (!isActiveCache)
            {
                return new EventTable[]
                {
                    new UnindexedEventTableList(pollResult, _streamNum)
                };
            }
            PropertySortedEventTableFactory tableFactory;
            if (_coercionType == null) {
                tableFactory = new PropertySortedEventTableFactory(_streamNum, _eventType, _propertyName);
            }
            else {
                tableFactory = new PropertySortedEventTableCoercedFactory(_streamNum, _eventType, _propertyName, _coercionType);
            }

            EventTable[] tables = tableFactory.MakeEventTables(new EventTableFactoryTableIdentStmt(statementContext));
            foreach (EventTable table in tables)
            {
                table.Add(pollResult.ToArray());
            }

            return tables;
        }
    
        public String ToQueryPlan()
        {
            return GetType().FullName + " property " + _propertyName + " coercion " + _coercionType;
        }
    }
}
