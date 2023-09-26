///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.index.hash;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.index.inkeyword
{
    public class PropertyHashedArrayFactory : EventTableFactory
    {
        private readonly PropertyHashedEventTableFactory[] factories;
        private readonly string optionalIndexName;
        private readonly EventPropertyValueGetter[] propertyGetters;
        private readonly string[] propertyNames;
        private readonly int streamNum;
        private readonly bool unique;

        public PropertyHashedArrayFactory(
            int streamNum,
            string[] propertyNames,
            bool unique,
            string optionalIndexName,
            EventPropertyValueGetter[] propertyGetters)
        {
            this.streamNum = streamNum;
            this.propertyNames = propertyNames;
            this.unique = unique;
            this.optionalIndexName = optionalIndexName;
            this.propertyGetters = propertyGetters;
            factories = new PropertyHashedEventTableFactory[propertyGetters.Length];
            for (var i = 0; i < factories.Length; i++) {
                factories[i] = new PropertyHashedEventTableFactory(
                    streamNum,
                    new[] { propertyNames[i] },
                    unique,
                    null,
                    propertyGetters[i],
                    null);
            }
        }

        public EventTable[] MakeEventTables(
            ExprEvaluatorContext exprEvaluatorContext,
            int? subqueryNumber)
        {
            var tables = new EventTable[propertyGetters.Length];
            if (unique) {
                for (var i = 0; i < tables.Length; i++) {
                    tables[i] = new PropertyHashedEventTableUnique(factories[i]);
                }
            }
            else {
                for (var i = 0; i < tables.Length; i++) {
                    tables[i] = new PropertyHashedEventTableUnadorned(factories[i]);
                }
            }

            return tables;
        }

        public Type EventTableClass {
            get {
                if (unique) {
                    return typeof(PropertyHashedEventTableUnique);
                }

                return typeof(PropertyHashedEventTable);
            }
        }

        public string ToQueryPlan()
        {
            return GetType().GetSimpleName() +
                   (unique ? " unique" : " non-unique") +
                   " streamNum=" +
                   streamNum +
                   " propertyNames=" +
                   propertyNames.RenderAny();
        }
    }
} // end of namespace