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
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.index.@base;


namespace com.espertech.esper.common.@internal.epl.index.hash
{
    public class PropertyHashedFactoryFactory : EventTableFactoryFactoryBase
    {
        private readonly string[] indexProps;
        private readonly bool unique;
        private readonly EventPropertyValueGetter valueGetter;
        private readonly MultiKeyFromObjectArray transformFireAndForget;
        private readonly DataInputOutputSerde keySerde;
        private readonly StateMgmtSetting stateMgmtSettings;

        public PropertyHashedFactoryFactory(
            int indexedStreamNum,
            int? subqueryNum,
            bool isFireAndForget,
            string[] indexProps,
            bool unique,
            EventPropertyValueGetter valueGetter,
            MultiKeyFromObjectArray transformFireAndForget,
            DataInputOutputSerde keySerde,
            StateMgmtSetting stateMgmtSettings) : base(indexedStreamNum, subqueryNum, isFireAndForget)
        {
            this.indexProps = indexProps;
            this.unique = unique;
            this.valueGetter = valueGetter;
            this.transformFireAndForget = transformFireAndForget;
            this.keySerde = keySerde;
            this.stateMgmtSettings = stateMgmtSettings;
        }

        public override EventTableFactory Create(
            EventType eventType,
            EventTableFactoryFactoryContext eventTableFactoryContext)
        {
            return eventTableFactoryContext.EventTableIndexService.CreateHashedOnly(
                indexedStreamNum,
                eventType,
                indexProps,
                transformFireAndForget,
                keySerde,
                unique,
                null,
                valueGetter,
                null,
                isFireAndForget,
                stateMgmtSettings);
        }
    }
} // end of namespace