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
using com.espertech.esper.common.@internal.epl.index.advanced.index.service;
using com.espertech.esper.common.@internal.epl.subselect;

namespace com.espertech.esper.common.@internal.epl.index.@base
{
    public interface EventTableIndexService
    {
        bool AllowInitIndex(bool isRecoveringResilient);

        EventTableFactory CreateHashedOnly(
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
            EventTableFactoryFactoryContext eventTableFactoryContext);

        EventTableFactory CreateUnindexed(
            int indexedStreamNum,
            EventType eventType,
            DataInputOutputSerde<Object> optionalValueSerde,
            bool isFireAndForget,
            EventTableFactoryFactoryContext eventTableFactoryContext);

        EventTableFactory CreateSorted(
            int indexedStreamNum,
            EventType eventType,
            String indexedProp,
            Type indexType,
            EventPropertyValueGetter getter,
            DataInputOutputSerde<Object> serde,
            DataInputOutputSerde<Object> optionalValueSerde,
            bool isFireAndForget,
            EventTableFactoryFactoryContext eventTableFactoryContext);

        EventTableFactory CreateComposite(
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
            bool isFireAndForget);

        EventTableFactory CreateInArray(
            int streamNum,
            EventType eventType,
            String[] propertyNames,
            Type[] indexTypes,
            DataInputOutputSerde<Object>[] indexSerdes,
            bool unique,
            EventPropertyValueGetter[] getters,
            bool isFireAndForget,
            EventTableFactoryFactoryContext eventTableFactoryContext);

        EventTableFactory CreateCustom(
            string optionalIndexName,
            int indexedStreamNum,
            EventType eventType,
            bool unique,
            EventAdvancedIndexProvisionRuntime advancedIndexProvisionDesc);
    }
} // end of namespace