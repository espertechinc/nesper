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

namespace com.espertech.esper.epl.join.table
{
    public class PropertyIndexedEventTableSingleCoerceAllFactory : PropertyIndexedEventTableSingleCoerceAddFactory
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="streamNum">is the stream number of the indexed stream</param>
        /// <param name="eventType">is the event type of the indexed stream</param>
        /// <param name="propertyName">are the property names to get property values</param>
        /// <param name="coercionType">are the classes to coerce indexed values to</param>
        public PropertyIndexedEventTableSingleCoerceAllFactory(int streamNum, EventType eventType, String propertyName, Type coercionType)
            : base(streamNum, eventType, propertyName, coercionType)
        {
        }

        public override EventTable[] MakeEventTables(EventTableFactoryTableIdent tableIdent, ExprEvaluatorContext exprEvaluatorContext)
        {
            EventTableOrganization organization = GetOrganization();
            return new EventTable[] { new PropertyIndexedEventTableSingleCoerceAll(PropertyGetter, organization, Coercer, CoercionType) };
        }

        protected override EventTableOrganization GetOrganization()
        {
            return new EventTableOrganization(OptionalIndexName, Unique, true, StreamNum, new String[] { PropertyName }, EventTableOrganizationType.HASH);
        }
    }
}
