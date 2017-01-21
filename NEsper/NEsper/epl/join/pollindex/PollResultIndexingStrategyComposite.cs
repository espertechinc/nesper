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
        private readonly String[] _indexPropertiesJoin;
        private readonly Type[] _keyCoercionTypes;
        private readonly String[] _rangePropertiesJoin;
        private readonly Type[] _rangeCoercionTypes;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="streamNum">is the stream number of the indexed stream</param>
        /// <param name="eventType">is the event type of the indexed stream</param>
        /// <param name="indexPropertiesJoin">The index properties join.</param>
        /// <param name="keyCoercionTypes">The key coercion types.</param>
        /// <param name="rangePropertiesJoin">The range properties join.</param>
        /// <param name="rangeCoercionTypes">The range coercion types.</param>
        public PollResultIndexingStrategyComposite(
            int streamNum,
            EventType eventType,
            String[] indexPropertiesJoin,
            Type[] keyCoercionTypes,
            String[] rangePropertiesJoin,
            Type[] rangeCoercionTypes)
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
            if (!isActiveCache)
            {
                return new EventTable[]
                {
                    new UnindexedEventTableList(pollResult, _streamNum)
                };
            }
            var factory = new PropertyCompositeEventTableFactory(_streamNum, _eventType, _indexPropertiesJoin, _keyCoercionTypes, _rangePropertiesJoin, _rangeCoercionTypes);
            var tables = factory.MakeEventTables(new EventTableFactoryTableIdentStmt(statementContext));
            foreach (var table in tables)
                table.Add(pollResult.ToArray());
            return tables;
        }
    
        public String ToQueryPlan() {
            return GetType().FullName +
                    " hash " + _indexPropertiesJoin.Render() +
                    " btree " + _rangePropertiesJoin.Render() +
                    " key coercion " + _keyCoercionTypes.Render() +
                    " range coercion " + _rangeCoercionTypes.Render();
        }
    }
}
