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
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.@join.rep;
using com.espertech.esper.epl.@join.table;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.events;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.join.exec.@base
{
    public class IndexedTableLookupStrategySingle : JoinExecTableLookupStrategy
    {
        private readonly EventType _eventType;
        private readonly String _property;
        private readonly PropertyIndexedEventTableSingle _index;
        private readonly EventPropertyGetter _propertyGetter;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="eventType">event type to expect for lookup</param>
        /// <param name="property">The property.</param>
        /// <param name="index">index to look up in</param>
        public IndexedTableLookupStrategySingle(EventType eventType, String property, PropertyIndexedEventTableSingle index)
        {
            _eventType = eventType;
            _property = property;
            if (index == null) {
                throw new ArgumentException("Unexpected null index received");
            }
            _index = index;
            _propertyGetter = EventBeanUtility.GetAssertPropertyGetter(eventType, property);
        }

        /// <summary>Returns event type of the lookup event. </summary>
        /// <value>event type of the lookup event</value>
        public EventType EventType
        {
            get { return _eventType; }
        }

        /// <summary>Returns index to look up in. </summary>
        /// <value>index to use</value>
        public PropertyIndexedEventTableSingle Index
        {
            get { return _index; }
        }

        public ICollection<EventBean> Lookup(EventBean theEvent, Cursor cursor, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QIndexJoinLookup(this, _index);}
    
            var key = GetKey(theEvent);
            var result = _index.Lookup(key);
    
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AIndexJoinLookup(result, key); }
            return result;
        }

        public LookupStrategyDesc StrategyDesc
        {
            get
            {
                return new LookupStrategyDesc(
                    LookupStrategyType.SINGLEPROP, new String[]
                    {
                        _property
                    });
            }
        }

        private Object GetKey(EventBean theEvent)
        {
            return _propertyGetter.Get(theEvent);
        }
    
        public override String ToString()
        {
            return "IndexedTableLookupStrategy indexProp=" + _property +
                    " index=(" + _index + ')';
        }
    }
}
