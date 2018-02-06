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
    /// Strategy for building an index out of poll-results knowing the properties to base the index on.
    /// </summary>
    public class PollResultIndexingStrategyComposite : PollResultIndexingStrategy
    {
        private readonly int _streamNum;
        private readonly EventType _eventType;
        private readonly IList<string> _indexPropertiesJoin;
        private readonly IList<Type> _keyCoercionTypes;
        private readonly IList<string> _rangePropertiesJoin;
        private readonly IList<Type> _rangeCoercionTypes;

        public PollResultIndexingStrategyComposite(
            int streamNum,
            EventType eventType,
            IList<string> indexPropertiesJoin,
            IList<Type> keyCoercionTypes,
            IList<string> rangePropertiesJoin,
            IList<Type> rangeCoercionTypes)
        {
            _streamNum = streamNum;
            _eventType = eventType;
            _keyCoercionTypes = keyCoercionTypes;
            _indexPropertiesJoin = indexPropertiesJoin;
            _rangePropertiesJoin = rangePropertiesJoin;
            _rangeCoercionTypes = rangeCoercionTypes;
        }
    
        public EventTable[] Index(IList<EventBean> pollResult, bool isActiveCache, StatementContext statementContext)
        {
            if (!isActiveCache) {
                return new EventTable[]{new UnindexedEventTableList(pollResult, _streamNum)};
            }
            var factory = new PropertyCompositeEventTableFactory(
                _streamNum, 
                _eventType, 
                _indexPropertiesJoin, 
                _keyCoercionTypes, 
                _rangePropertiesJoin, 
                _rangeCoercionTypes);
            var evaluatorContextStatement = new ExprEvaluatorContextStatement(statementContext, false);
            EventTable[] tables = factory.MakeEventTables(new EventTableFactoryTableIdentStmt(statementContext), evaluatorContextStatement);
            foreach (var table in tables)
            {
                table.Add(pollResult.ToArray(), evaluatorContextStatement);
            }
            return tables;
        }
    
        public string ToQueryPlan()
        {
            return string.Format(
                "{0} hash {1} btree {2} key coercion {3} range coercion {4}", GetType().Name,
                CompatExtensions.Render(_indexPropertiesJoin), 
                CompatExtensions.Render(_rangePropertiesJoin),
                CompatExtensions.Render(_keyCoercionTypes), 
                CompatExtensions.Render(_rangeCoercionTypes));
        }
    }
} // end of namespace
