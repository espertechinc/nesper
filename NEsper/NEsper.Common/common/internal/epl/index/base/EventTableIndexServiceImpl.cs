///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.index.advanced.index.service;
using com.espertech.esper.common.@internal.epl.index.composite;
using com.espertech.esper.common.@internal.epl.index.hash;
using com.espertech.esper.common.@internal.epl.index.inkeyword;
using com.espertech.esper.common.@internal.epl.index.sorted;
using com.espertech.esper.common.@internal.epl.index.unindexed;
using com.espertech.esper.common.@internal.epl.subselect;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.index.@base
{
    public class EventTableIndexServiceImpl : EventTableIndexService
    {
        public readonly static EventTableIndexServiceImpl INSTANCE = new EventTableIndexServiceImpl();

        public bool AllowInitIndex(bool isRecoveringResilient)
        {
            return true;
        }

        public EventTableFactory CreateHashedOnly(
            int indexedStreamNum,
            EventType eventType,
            String[] indexProps,
            Type[] indexTypes,
            MultiKeyFromObjectArray transformFireAndForget,
            DataInputOutputSerde<Object> keySerde,
            bool unique,
            String optionalIndexName,
            EventPropertyValueGetter getter,
            DataInputOutputSerde<Object> optionalValueSerde,
            bool isFireAndForget,
            EventTableFactoryFactoryContext eventTableFactoryContext)
        {
            return new PropertyHashedEventTableFactory(
                indexedStreamNum,
                indexProps,
                unique,
                optionalIndexName,
                getter,
                transformFireAndForget);
        }

        public EventTableFactory CreateUnindexed(
            int indexedStreamNum,
            EventType eventType,
            DataInputOutputSerde<Object> optionalValueSerde,
            bool isFireAndForget,
            EventTableFactoryFactoryContext eventTableFactoryContext)
        {
            return new UnindexedEventTableFactory(indexedStreamNum);
        }

        public EventTableFactory CreateSorted(
            int indexedStreamNum,
            EventType eventType,
            String indexedProp,
            Type indexType,
            EventPropertyValueGetter getter,
            DataInputOutputSerde<Object> serde,
            DataInputOutputSerde<Object> optionalValueSerde,
            bool isFireAndForget,
            EventTableFactoryFactoryContext eventTableFactoryContext)
        {
            return new PropertySortedEventTableFactory(indexedStreamNum, indexedProp, getter, indexType);
        }

        public EventTableFactory CreateComposite(
            int indexedStreamNum,
            EventType eventType,
            String[] indexProps,
            Type[] indexCoercionTypes,
            EventPropertyValueGetter indexGetter,
            MultiKeyFromObjectArray transformFireAndForget,
            DataInputOutputSerde<Object> keySerde,
            String[] rangeProps,
            Type[] rangeCoercionTypes,
            EventPropertyValueGetter[] rangeGetters,
            DataInputOutputSerde<Object>[] rangeSerdes,
            DataInputOutputSerde<Object> optionalValueSerde,
            bool isFireAndForget)
        {
            return new PropertyCompositeEventTableFactory(
                indexedStreamNum,
                indexProps,
                indexCoercionTypes,
                indexGetter,
                transformFireAndForget,
                rangeProps,
                rangeCoercionTypes,
                rangeGetters);
        }

        public EventTableFactory CreateInArray(
            int streamNum,
            EventType eventType,
            String[] propertyNames,
            Type[] indexTypes,
            DataInputOutputSerde<Object>[] indexSerdes,
            bool unique,
            EventPropertyValueGetter[] getters,
            bool isFireAndForget,
            EventTableFactoryFactoryContext eventTableFactoryContext)
        {
            return new PropertyHashedArrayFactory(streamNum, propertyNames, unique, null, getters);
        }

        public EventTableFactory CreateCustom(
            string indexName,
            int indexedStreamNum,
            EventType eventType,
            bool unique,
            EventAdvancedIndexProvisionRuntime advancedIndexProvisionDesc)
        {
            return new EventTableFactoryCustomIndex(
                indexName,
                indexedStreamNum,
                eventType,
                unique,
                advancedIndexProvisionDesc);
        }
    }
} // end of namespace