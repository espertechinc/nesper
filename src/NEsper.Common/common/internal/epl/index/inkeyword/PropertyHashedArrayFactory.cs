///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.index.hash;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.index.inkeyword
{
    public class PropertyHashedArrayFactory : EventTableFactory
    {
        private readonly PropertyHashedEventTableFactory[] _factories;
        private readonly string _optionalIndexName;
        private readonly EventPropertyValueGetter[] _propertyGetters;
        private readonly string[] _propertyNames;
        private readonly int _streamNum;
        private readonly bool _unique;

        public PropertyHashedArrayFactory(
            int streamNum,
            string[] propertyNames,
            bool unique,
            string optionalIndexName,
            EventPropertyValueGetter[] propertyGetters)
        {
            _streamNum = streamNum;
            _propertyNames = propertyNames;
            _unique = unique;
            _optionalIndexName = optionalIndexName;
            _propertyGetters = propertyGetters;
            _factories = new PropertyHashedEventTableFactory[propertyGetters.Length];
            for (var i = 0; i < _factories.Length; i++) {
                _factories[i] = new PropertyHashedEventTableFactory(
                    streamNum,
                    new[] {propertyNames[i]},
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
            var tables = new EventTable[_propertyGetters.Length];
            if (_unique) {
                for (var i = 0; i < tables.Length; i++) {
                    tables[i] = new PropertyHashedEventTableUnique(_factories[i]);
                }
            }
            else {
                for (var i = 0; i < tables.Length; i++) {
                    tables[i] = new PropertyHashedEventTableUnadorned(_factories[i]);
                }
            }

            return tables;
        }

        public Type EventTableClass {
            get {
                if (_unique) {
                    return typeof(PropertyHashedEventTableUnique);
                }

                return typeof(PropertyHashedEventTable);
            }
        }

        public string ToQueryPlan()
        {
            return GetType().GetSimpleName() +
                   (_unique ? " unique" : " non-unique") +
                   " streamNum=" +
                   _streamNum +
                   " propertyNames=" +
                   _propertyNames.RenderAny();
        }
    }
} // end of namespace