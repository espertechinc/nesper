///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.epl.index.@base;

namespace com.espertech.esper.common.@internal.epl.index.unindexed
{
    public class UnindexedEventTableFactoryFactory : EventTableFactoryFactoryBase
    {
        private readonly StateMgmtSetting _stateMgmtSettings;

        public UnindexedEventTableFactoryFactory(
            int indexedStreamNum,
            int? subqueryNum,
            bool isFireAndForget,
            StateMgmtSetting stateMgmtSettings)
            : base(indexedStreamNum, subqueryNum, isFireAndForget)
        {
            _stateMgmtSettings = stateMgmtSettings;
        }

        public override EventTableFactory Create(
            EventType eventType,
            EventTableFactoryFactoryContext eventTableFactoryContext)
        {
            return eventTableFactoryContext.EventTableIndexService.CreateUnindexed(
                indexedStreamNum,
                eventType,
                null,
                isFireAndForget,
                _stateMgmtSettings);
        }
    }
} // end of namespace