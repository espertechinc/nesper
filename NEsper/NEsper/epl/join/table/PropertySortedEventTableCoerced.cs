///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.join.table
{
    public class PropertySortedEventTableCoerced : PropertySortedEventTable
    {
        private readonly Type _coercionType;
    
        public PropertySortedEventTableCoerced(EventPropertyGetter propertyGetter, EventTableOrganization organization, Type coercionType)
            : base(propertyGetter, organization)
        {
            _coercionType = coercionType;
        }
    
        protected override Object Coerce(Object value)
        {
            if (value != null && value.GetType() != _coercionType)
            {
                if (value.IsNumber())
                {
                    return CoercerFactory.CoerceBoxed(value, _coercionType);
                }
            }
            return value;        
        }
    
        public override String ToString()
        {
            return "PropertySortedEventTableCoerced" +
                    " streamNum=" + Organization.StreamNum +
                    " propertyGetter=" + PropertyGetter +
                    " coercionType=" + _coercionType;
        }
    }
}
