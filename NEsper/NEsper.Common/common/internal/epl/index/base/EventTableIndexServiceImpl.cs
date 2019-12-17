///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.index.advanced.index.service;
using com.espertech.esper.common.@internal.epl.index.composite;
using com.espertech.esper.common.@internal.epl.index.hash;
using com.espertech.esper.common.@internal.epl.index.inkeyword;
using com.espertech.esper.common.@internal.epl.index.sorted;
using com.espertech.esper.common.@internal.epl.index.unindexed;
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
            string[] indexProps,
            Type[] indexTypes,
            bool unique,
            string optionalIndexName,
            EventPropertyValueGetter getter,
            object optionalSerde,
            bool isFireAndForget,
            StatementContext statementContext)
        {
            return new PropertyHashedEventTableFactory(indexedStreamNum, indexProps, unique, optionalIndexName, getter);
        }

        public EventTableFactory CreateUnindexed(
            int indexedStreamNum,
            EventType eventType,
            object optionalSerde,
            bool isFireAndForget,
            StatementContext statementContext)
        {
            return new UnindexedEventTableFactory(indexedStreamNum);
        }

        public EventTableFactory CreateSorted(
            int indexedStreamNum,
            EventType eventType,
            string indexedProp,
            Type indexType,
            EventPropertyValueGetter getter,
            object optionalSerde,
            bool isFireAndForget,
            StatementContext statementContext)
        {
            return new PropertySortedEventTableFactory(indexedStreamNum, indexedProp, getter, indexType);
        }

        public EventTableFactory CreateComposite(
            int indexedStreamNum,
            EventType eventType,
            string[] indexProps,
            Type[] indexCoercionTypes,
            EventPropertyValueGetter indexGetter,
            string[] rangeProps,
            Type[] rangeCoercionTypes,
            EventPropertyValueGetter[] rangeGetters,
            object optionalSerde,
            bool isFireAndForget)
        {
            return new PropertyCompositeEventTableFactory(
                indexedStreamNum,
                indexProps,
                indexCoercionTypes,
                indexGetter,
                rangeProps,
                rangeCoercionTypes,
                rangeGetters);
        }

        public EventTableFactory CreateInArray(
            int streamNum,
            EventType eventType,
            string[] propertyNames,
            Type[] indexTypes,
            bool unique,
            EventPropertyValueGetter[] getters,
            bool isFireAndForget,
            StatementContext statementContext)
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