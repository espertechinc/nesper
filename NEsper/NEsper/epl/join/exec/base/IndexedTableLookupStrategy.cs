///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.@join.rep;
using com.espertech.esper.epl.@join.table;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.events;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.join.exec.@base
{
    /// <summary>
    /// Lookup on an index using a set of properties as key values.
    /// </summary>
    public class IndexedTableLookupStrategy : JoinExecTableLookupStrategy
    {
        private readonly EventType _eventType;
        private readonly String[] _properties;
        private readonly PropertyIndexedEventTable _index;
        private readonly EventPropertyGetter[] _propertyGetters;
    
        /// <summary>Ctor. </summary>
        /// <param name="eventType">event type to expect for lookup</param>
        /// <param name="properties">key properties</param>
        /// <param name="index">index to look up in</param>
        public IndexedTableLookupStrategy(EventType eventType, String[] properties, PropertyIndexedEventTable index)
        {
            _eventType = eventType;
            _properties = properties;
            if (index == null) {
                throw new ArgumentException("Unexpected null index received");
            }
            _index = index;
    
            _propertyGetters = new EventPropertyGetter[properties.Length];
            for (int i = 0; i < properties.Length; i++)
            {
                _propertyGetters[i] = eventType.GetGetter(properties[i]);
    
                if (_propertyGetters[i] == null)
                {
                    throw new ArgumentException("Property named '" + properties[i] + "' is invalid for type " + eventType);
                }
            }
        }

        /// <summary>Returns event type of the lookup event. </summary>
        /// <value>event type of the lookup event</value>
        public EventType EventType
        {
            get { return _eventType; }
        }

        /// <summary>Returns properties to use from lookup event to look up in index. </summary>
        /// <value>properties to use from lookup event</value>
        public string[] Properties
        {
            get { return _properties; }
        }

        /// <summary>Returns index to look up in. </summary>
        /// <value>index to use</value>
        public PropertyIndexedEventTable Index
        {
            get { return _index; }
        }

        public ICollection<EventBean> Lookup(EventBean theEvent, Cursor cursor, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (InstrumentationHelper.ENABLED) {InstrumentationHelper.Get().QIndexJoinLookup(this, _index);}
    
            var keys = GetKeys(theEvent);
            var result = _index.Lookup(keys);
    
            if (InstrumentationHelper.ENABLED) {InstrumentationHelper.Get().AIndexJoinLookup(result, keys);}
            return result;
        }
    
        private Object[] GetKeys(EventBean theEvent)
        {
            return EventBeanUtility.GetPropertyArray(theEvent, _propertyGetters);
        }
    
        public override String ToString()
        {
            return "IndexedTableLookupStrategy indexProps=" + _properties.Render() +
                    " index=(" + _index + ')';
        }

        public LookupStrategyDesc StrategyDesc
        {
            get { return new LookupStrategyDesc(LookupStrategyType.MULTIPROP, _properties); }
        }
    }
}
