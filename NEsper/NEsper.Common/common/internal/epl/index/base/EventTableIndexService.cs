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

namespace com.espertech.esper.common.@internal.epl.index.@base
{
    public interface EventTableIndexService
    {
        bool AllowInitIndex(bool isRecoveringResilient);

        EventTableFactory CreateHashedOnly(
            int indexedStreamNum,
            EventType eventType,
            string[] indexProps,
            Type[] indexTypes,
            bool unique,
            string optionalIndexName,
            EventPropertyValueGetter getter,
            object optionalSerde,
            bool isFireAndForget,
            StatementContext statementContext);

        EventTableFactory CreateUnindexed(
            int indexedStreamNum,
            EventType eventType,
            object optionalSerde,
            bool isFireAndForget,
            StatementContext statementContext);

        EventTableFactory CreateSorted(
            int indexedStreamNum,
            EventType eventType,
            string indexedProp,
            Type indexType,
            EventPropertyValueGetter getter,
            object optionalSerde,
            bool isFireAndForget,
            StatementContext statementContext);

        EventTableFactory CreateComposite(
            int indexedStreamNum,
            EventType eventType,
            string[] indexProps,
            Type[] indexCoercionTypes,
            EventPropertyValueGetter indexGetter,
            string[] rangeProps,
            Type[] rangeCoercionTypes,
            EventPropertyValueGetter[] rangeGetters,
            object optionalSerde,
            bool isFireAndForget);

        EventTableFactory CreateInArray(
            int streamNum,
            EventType eventType,
            string[] propertyNames,
            Type[] indexTypes,
            bool unique,
            EventPropertyValueGetter[] getters,
            bool isFireAndForget,
            StatementContext statementContext);

        EventTableFactory CreateCustom(
            string optionalIndexName,
            int indexedStreamNum,
            EventType eventType,
            bool unique,
            EventAdvancedIndexProvisionRuntime advancedIndexProvisionDesc);
    }
} // end of namespace