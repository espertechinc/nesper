///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.join.table
{
    public class PropertyIndexedEventTableSingleCoerceAddFactory : PropertyIndexedEventTableSingleFactory
    {
        internal readonly Coercer Coercer;
        internal readonly Type CoercionType;

        /// <summary>Ctor. </summary>
        /// <param name="streamNum">is the stream number of the indexed stream</param>
        /// <param name="eventType">is the event type of the indexed stream</param>
        /// <param name="propertyName">are the property names to get property values</param>
        /// <param name="coercionType">are the classes to coerce indexed values to</param>
        public PropertyIndexedEventTableSingleCoerceAddFactory(
            int streamNum,
            EventType eventType,
            String propertyName,
            Type coercionType)
            : base(streamNum, eventType, propertyName, false, null)
        {
            CoercionType = coercionType;
            Coercer = coercionType.IsNumeric() ? CoercerFactory.GetCoercer(null, coercionType) : null;
        }

        public override EventTable[] MakeEventTables(EventTableFactoryTableIdent tableIdent, ExprEvaluatorContext exprEvaluatorContext)
        {
            EventTableOrganization organization = GetOrganization();
            return new EventTable[] { new PropertyIndexedEventTableSingleCoerceAdd(PropertyGetter, organization, Coercer, CoercionType) };
        }

        protected virtual EventTableOrganization GetOrganization()
        {
            return new EventTableOrganization(OptionalIndexName, Unique, true, StreamNum, new String[] { PropertyName }, EventTableOrganizationType.HASH);
        }
    }
}