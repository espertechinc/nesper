///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.epl.index.@base;


namespace com.espertech.esper.common.@internal.epl.index.sorted
{
    public class PropertySortedFactoryFactory : EventTableFactoryFactoryBase
    {
        private readonly string indexProp;
        private readonly Type indexType;
        private readonly EventPropertyValueGetter valueGetter;
        private readonly DataInputOutputSerde indexSerde;
        private readonly StateMgmtSetting stateMgmtSettings;

        public PropertySortedFactoryFactory(
            int indexedStreamNum,
            int? subqueryNum,
            bool isFireAndForget,
            string indexProp,
            Type indexType,
            EventPropertyValueGetter valueGetter,
            DataInputOutputSerde indexSerde,
            StateMgmtSetting stateMgmtSettings) : base(indexedStreamNum, subqueryNum, isFireAndForget)
        {
            this.indexProp = indexProp;
            this.indexType = indexType;
            this.valueGetter = valueGetter;
            this.indexSerde = indexSerde;
            this.stateMgmtSettings = stateMgmtSettings;
        }

        public override EventTableFactory Create(
            EventType eventType,
            EventTableFactoryFactoryContext eventTableFactoryContext)
        {
            return eventTableFactoryContext.EventTableIndexService.CreateSorted(
                indexedStreamNum,
                eventType,
                indexProp,
                indexType,
                valueGetter,
                indexSerde,
                null,
                isFireAndForget,
                stateMgmtSettings);
        }
    }
} // end of namespace