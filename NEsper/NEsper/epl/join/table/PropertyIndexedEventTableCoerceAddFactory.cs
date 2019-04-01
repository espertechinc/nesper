///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.join.table
{
    /// <summary>
    /// MapIndex that organizes events by the event property values into hash buckets. Based 
    /// on a HashMap with <seealso cref="com.espertech.esper.collection.MultiKeyUntyped"/> keys 
    /// that store the property values. <para/>Performs coercion of the index keys before storing 
    /// the keys.
    /// <para/>
    /// Takes a list of property names as parameter. Doesn't care which event type the events have 
    /// as long as the properties exist. If the same event is added twice, the class throws an 
    /// exception on add.
    /// </summary>
    public class PropertyIndexedEventTableCoerceAddFactory : PropertyIndexedEventTableFactory
    {
        protected readonly Coercer[] Coercers;
        protected readonly Type[] CoercionType;
    
        /// <summary>Ctor. </summary>
        /// <param name="streamNum">is the stream number of the indexed stream</param>
        /// <param name="eventType">is the event type of the indexed stream</param>
        /// <param name="propertyNames">are the property names to get property values</param>
        /// <param name="coercionType">are the classes to coerce indexed values to</param>
        public PropertyIndexedEventTableCoerceAddFactory(int streamNum, EventType eventType, IList<string> propertyNames, IList<Type> coercionType)
            : base(streamNum, eventType, propertyNames, false, null)
        {
            CoercionType = coercionType.ToArray();
            Coercers = new Coercer[coercionType.Count];
            for (int i = 0; i < coercionType.Count; i++)
            {
                if (coercionType[i].IsNumeric())
                {
                    Coercers[i] = CoercerFactory.GetCoercer(null, coercionType[i]);
                }
            }
        }

        public override EventTable[] MakeEventTables(EventTableFactoryTableIdent tableIdent, ExprEvaluatorContext exprEvaluatorContext)
        {
            EventTableOrganization organization = Organization;
            return new EventTable[] { new PropertyIndexedEventTableCoerceAdd(propertyGetters, organization, Coercers, CoercionType) };
        }

        protected override EventTableOrganization Organization
        {
            get
            {
                return new EventTableOrganization(
                    optionalIndexName, unique, true, streamNum, propertyNames, EventTableOrganizationType.HASH);
            }
        }
    }
}
