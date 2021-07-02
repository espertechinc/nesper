///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.index.hash
{
    public class PropertyHashedEventTableFactory : EventTableFactory
    {
        internal readonly string optionalIndexName;
        internal readonly EventPropertyValueGetter propertyGetter;
        internal readonly string[] propertyNames;
        internal readonly int streamNum;
        internal readonly bool unique;
        internal readonly MultiKeyFromObjectArray multiKeyTransform;

        public PropertyHashedEventTableFactory(
            int streamNum,
            string[] propertyNames,
            bool unique,
            string optionalIndexName,
            EventPropertyValueGetter propertyGetter,
            MultiKeyFromObjectArray multiKeyTransform)
        {
            this.streamNum = streamNum;
            this.propertyNames = propertyNames;
            this.unique = unique;
            this.optionalIndexName = optionalIndexName;
            this.propertyGetter = propertyGetter;
            this.multiKeyTransform = multiKeyTransform;

            if (propertyGetter == null) {
                throw new ArgumentException("Property-getter is null");
            }
        }

        public EventTableOrganization Organization => new EventTableOrganization(
            optionalIndexName,
            unique,
            false,
            streamNum,
            propertyNames,
            EventTableOrganizationType.HASH);

        public EventTable[] MakeEventTables(
            ExprEvaluatorContext exprEvaluatorContext,
            int? subqueryNumber)
        {
            if (unique) {
                return new EventTable[] {new PropertyHashedEventTableUnique(this)};
            }

            return new EventTable[] {new PropertyHashedEventTableUnadorned(this)};
        }

        public string ToQueryPlan()
        {
            return GetType().Name +
                   (unique ? " unique" : " non-unique") +
                   " streamNum=" +
                   streamNum +
                   " propertyNames=" +
                   Arrays.AsList(propertyNames);
        }

        public Type EventTableClass =>
            unique ? typeof(PropertyHashedEventTableUnique) : typeof(PropertyHashedEventTableUnadorned);
    }
} // end of namespace