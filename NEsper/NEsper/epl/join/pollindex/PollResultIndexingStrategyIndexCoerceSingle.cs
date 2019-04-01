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
    /// Strategy for building an index out of poll-results knowing the properties to base the index on, and their
    /// coercion types.
    /// </summary>
    public class PollResultIndexingStrategyIndexCoerceSingle : PollResultIndexingStrategy
    {
        private readonly int _streamNum;
        private readonly EventType _eventType;
        private readonly string _propertyName;
        private readonly Type _coercionType;

        public PollResultIndexingStrategyIndexCoerceSingle(
            int streamNum,
            EventType eventType,
            string propertyName,
            Type coercionType)
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
            var factory = new PropertyIndexedEventTableSingleCoerceAllFactory(
                _streamNum, _eventType, _propertyName, _coercionType);
            var evaluatorContextStatement = new ExprEvaluatorContextStatement(statementContext, false);
            EventTable[] tables = factory.MakeEventTables(new EventTableFactoryTableIdentStmt(statementContext), evaluatorContextStatement);
            foreach (EventTable table in tables)
            {
                table.Add(pollResult.ToArray(), evaluatorContextStatement);
            }
            return tables;
        }

        public string ToQueryPlan()
        {
            return GetType().Name + " property " + _propertyName + " coercion " + _coercionType;
        }
    }
} // end of namespace
