///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;

namespace com.espertech.esper.epl.join.table
{
    public class PropertySortedEventTableCoercedFactory : PropertySortedEventTableFactory
    {
        private readonly Type _coercionType;
    
       /// <summary>Ctor. </summary>
       /// <param name="streamNum">the stream number that is indexed</param>
       /// <param name="eventType">types of events indexed</param>
       /// <param name="propertyName">property names to use for indexing</param>
       /// <param name="coercionType">property types</param>
        public PropertySortedEventTableCoercedFactory(int streamNum, EventType eventType, String propertyName, Type coercionType)

                    : base(streamNum, eventType, propertyName)
        {
            _coercionType = coercionType;
        }
    
        public override EventTable[] MakeEventTables()
        {
            var organization = new EventTableOrganization(null, false, true, StreamNum, new String[] {PropertyName}, EventTableOrganization.EventTableOrganizationType.BTREE);
            return new EventTable[]
            {
                new PropertySortedEventTableCoerced(PropertyGetter, organization, _coercionType)
            };
        }
    
        public override String ToString()
        {
            return "PropertySortedEventTableCoerced" +
                    " streamNum=" + StreamNum +
                    " propertyName=" + PropertyName +
                    " coercionType=" + _coercionType;
        }
    }
}
