///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.index.@base;

namespace com.espertech.esper.common.@internal.epl.index.unindexed
{
    public class UnindexedEventTableFactoryFactory : EventTableFactoryFactoryBase
    {
        public UnindexedEventTableFactoryFactory(
            int indexedStreamNum,
            int? subqueryNum,
            object optionalSerde,
            bool isFireAndForget)
            : base(indexedStreamNum, subqueryNum, optionalSerde, isFireAndForget)
        {
        }

        public override EventTableFactory Create(
            EventType eventType,
            StatementContext statementContext)
        {
            return statementContext.EventTableIndexService.CreateUnindexed(
                indexedStreamNum,
                eventType,
                optionalSerde,
                isFireAndForget,
                statementContext);
        }
    }
} // end of namespace