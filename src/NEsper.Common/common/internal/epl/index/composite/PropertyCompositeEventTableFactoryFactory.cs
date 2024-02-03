///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.epl.index.@base;


namespace com.espertech.esper.common.@internal.epl.index.composite
{
    public class PropertyCompositeEventTableFactoryFactory : EventTableFactoryFactory
    {
        private readonly int indexedStreamNum;
        private readonly int? subqueryNum;
        private readonly bool isFireAndForget;
        private readonly string[] keyProps;
        private readonly Type[] keyTypes;
        private readonly EventPropertyValueGetter keyGetter;
        private readonly DataInputOutputSerde keySerde;
        private readonly string[] rangeProps;
        private readonly Type[] rangeTypes;
        private readonly EventPropertyValueGetter[] rangeGetters;
        private readonly DataInputOutputSerde[] rangeKeySerdes;

        public PropertyCompositeEventTableFactoryFactory(
            int indexedStreamNum,
            int? subqueryNum,
            bool isFireAndForget,
            string[] keyProps,
            Type[] keyTypes,
            EventPropertyValueGetter keyGetter,
            DataInputOutputSerde keySerde,
            string[] rangeProps,
            Type[] rangeTypes,
            EventPropertyValueGetter[] rangeGetters,
            DataInputOutputSerde[] rangeKeySerdes)
        {
            this.indexedStreamNum = indexedStreamNum;
            this.subqueryNum = subqueryNum;
            this.isFireAndForget = isFireAndForget;
            this.keyProps = keyProps;
            this.keyTypes = keyTypes;
            this.keyGetter = keyGetter;
            this.keySerde = keySerde;
            this.rangeProps = rangeProps;
            this.rangeTypes = rangeTypes;
            this.rangeGetters = rangeGetters;
            this.rangeKeySerdes = rangeKeySerdes;
        }

        public EventTableFactory Create(
            EventType eventType,
            EventTableFactoryFactoryContext eventTableFactoryContext)
        {
            return eventTableFactoryContext.EventTableIndexService.CreateComposite(
                indexedStreamNum,
                eventType,
                keyProps,
                keyTypes,
                keyGetter,
                null,
                keySerde,
                rangeProps,
                rangeTypes,
                rangeGetters,
                rangeKeySerdes,
                null,
                isFireAndForget);
        }
    }
} // end of namespace