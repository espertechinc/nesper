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
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.@join.table;

namespace com.espertech.esper.epl.join.pollindex
{
    /// <summary>
    /// Strategy for building an index out of poll-results knowing the properties to base the index on, and their coercion types.
    /// </summary>
    public class PollResultIndexingStrategyIndexCoerce : PollResultIndexingStrategy
    {
        private readonly int _streamNum;
        private readonly EventType _eventType;
        private readonly IList<String> _propertyNames;
        private readonly IList<Type> _coercionTypes;
    
        /// <summary>Ctor. </summary>
        /// <param name="streamNum">is the stream number of the indexed stream</param>
        /// <param name="eventType">is the event type of the indexed stream</param>
        /// <param name="propertyNames">is the property names to be indexed</param>
        /// <param name="coercionTypes">is the types to coerce to for keys and values</param>
        public PollResultIndexingStrategyIndexCoerce(int streamNum, EventType eventType, IList<string> propertyNames, IList<Type> coercionTypes)
        {
            _streamNum = streamNum;
            _eventType = eventType;
            _propertyNames = propertyNames;
            _coercionTypes = coercionTypes;
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
            
            var factory = new PropertyIndexedEventTableCoerceAllFactory(_streamNum, _eventType, _propertyNames, _coercionTypes);
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
            return GetType().FullName + " properties " + _propertyNames.Render() + " coercion " + _coercionTypes.Render();
        }
    }
}
