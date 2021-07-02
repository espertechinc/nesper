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
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.index.advanced.index.service;

namespace com.espertech.esper.common.@internal.epl.index.@base
{
    public interface EventTableIndexService
    {
        bool AllowInitIndex(bool isRecoveringResilient);

        EventTableFactory CreateHashedOnly(
            int indexedStreamNum,
            EventType eventType,
            string[] indexProps,
            MultiKeyFromObjectArray transformFireAndForget,
            DataInputOutputSerde keySerde,
            bool unique,
            string optionalIndexName,
            EventPropertyValueGetter getter,
            DataInputOutputSerde optionalValueSerde,
            bool isFireAndForget,
            StateMgmtSetting stateMgmtSettings);

        EventTableFactory CreateUnindexed(
            int indexedStreamNum,
            EventType eventType,
            DataInputOutputSerde optionalValueSerde,
            bool isFireAndForget,
            StateMgmtSetting stateMgmtSettings);


        EventTableFactory CreateSorted(
            int indexedStreamNum,
            EventType eventType,
            string indexedProp,
            Type indexType,
            EventPropertyValueGetter getter,
            DataInputOutputSerde serde,
            DataInputOutputSerde optionalValueSerde,
            bool isFireAndForget,
            EventTableFactoryFactoryContext eventTableFactoryContext,
            StateMgmtSetting stateMgmtSettings);

        EventTableFactory CreateComposite(
            int indexedStreamNum,
            EventType eventType,
            string[] indexProps,
            Type[] indexCoercionTypes,
            EventPropertyValueGetter indexGetter,
            MultiKeyFromObjectArray transformFireAndForget,
            DataInputOutputSerde keySerde,
            string[] rangeProps,
            Type[] rangeCoercionTypes,
            EventPropertyValueGetter[] rangeGetters,
            DataInputOutputSerde[] rangeSerdes,
            DataInputOutputSerde optionalValueSerde,
            bool isFireAndForget);

        EventTableFactory CreateInArray(
            int streamNum,
            EventType eventType,
            string[] propertyNames,
            Type[] indexTypes,
            DataInputOutputSerde[] indexSerdes,
            bool unique,
            EventPropertyValueGetter[] getters,
            bool isFireAndForget,
            EventTableFactoryFactoryContext eventTableFactoryContext,
            StateMgmtSetting stateMgmtSettings);

        EventTableFactory CreateCustom(
            string optionalIndexName,
            int indexedStreamNum,
            EventType eventType,
            bool unique,
            EventAdvancedIndexProvisionRuntime advancedIndexProvisionDesc);
    }
} // end of namespace