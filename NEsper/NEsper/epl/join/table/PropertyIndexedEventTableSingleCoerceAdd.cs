///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.events;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.join.table
{
    public class PropertyIndexedEventTableSingleCoerceAdd : PropertyIndexedEventTableSingleUnadorned
    {
        private readonly Coercer _coercer;
        private readonly Type _coercionType;
    
        public PropertyIndexedEventTableSingleCoerceAdd(EventPropertyGetter propertyGetter, EventTableOrganization organization, Coercer coercer, Type coercionType)
            : base(propertyGetter, organization)
        {
            _coercer = coercer;
            _coercionType = coercionType;
        }
    
        protected override Object GetKey(EventBean theEvent)
        {
            var keyValue = base.GetKey(theEvent);
            if ((keyValue != null) && (keyValue.GetType() != _coercionType))
            {
                keyValue = keyValue.IsNumber() 
                    ? _coercer.Invoke(keyValue) 
                    : EventBeanUtility.Coerce(keyValue, _coercionType);
            }
            return keyValue;
        }
    }
}
