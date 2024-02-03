///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.serde.compiletime.resolve
{
    public interface SerdeCompileTimeResolver
    {
        DataInputOutputSerdeForge SerdeForFilter(
            Type evaluationType,
            StatementRawInfo raw);

        DataInputOutputSerdeForge[] SerdeForDataWindowSortCriteria(
            Type[] types,
            StatementRawInfo raw);

        DataInputOutputSerdeForge[] SerdeForMultiKey(
            Type[] types,
            StatementRawInfo raw);

        DataInputOutputSerdeForge SerdeForKeyNonArray(
            Type paramType,
            StatementRawInfo raw);

        DataInputOutputSerdeForge SerdeForDerivedViewAddProp(
            Type evalType,
            StatementRawInfo raw);

        DataInputOutputSerdeForge SerdeForBeanEventType(
            StatementRawInfo raw,
            Type underlyingType,
            string eventTypeName,
            IList<EventType> eventTypeSupertypes);

        DataInputOutputSerdeForge SerdeForEventProperty(
            Type typedProperty,
            string eventTypeName,
            string propertyName,
            StatementRawInfo raw);

        DataInputOutputSerdeForge SerdeForAggregation(
            Type type,
            StatementRawInfo raw);

        DataInputOutputSerdeForge SerdeForAggregationDistinct(
            Type type,
            StatementRawInfo raw);

        DataInputOutputSerdeForge SerdeForIndexBtree(
            Type rangeType,
            StatementRawInfo raw);

        DataInputOutputSerdeForge SerdeForIndexHashNonArray(
            Type propType,
            StatementRawInfo raw);

        DataInputOutputSerdeForge SerdeForVariable(
            Type type,
            string variableName,
            StatementRawInfo raw);

        DataInputOutputSerdeForge SerdeForEventTypeExternalProvider(
            BaseNestableEventType eventType,
            StatementRawInfo raw);

        bool IsTargetHA { get; }
    }
} // end of namespace