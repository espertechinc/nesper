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
    public class PollResultIndexingStrategyIndexSingle : PollResultIndexingStrategy
    {
        private readonly int _streamNum;
        private readonly EventType _eventType;
        private readonly String _propertyName;
    
        /// <summary>Ctor. </summary>
        /// <param name="streamNum">is the stream number of the indexed stream</param>
        /// <param name="eventType">is the event type of the indexed stream</param>
        /// <param name="propertyName">is the property names to be indexed</param>
        public PollResultIndexingStrategyIndexSingle(int streamNum, EventType eventType, String propertyName)
        {
            _streamNum = streamNum;
            _eventType = eventType;
            _propertyName = propertyName;
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
            var factory = new PropertyIndexedEventTableSingleFactory(_streamNum, _eventType, _propertyName, false, null);
            var evaluatorContextStatement = new ExprEvaluatorContextStatement(statementContext, false);
            var tables = factory.MakeEventTables(new EventTableFactoryTableIdentStmt(statementContext), evaluatorContextStatement);
            foreach (var table in tables)
            {
                table.Add(pollResult.ToArray(), evaluatorContextStatement);
            }

            return tables;
        }
    
        public String ToQueryPlan()
        {
            return GetType().FullName + " properties " + _propertyName;
        }
    }
}
