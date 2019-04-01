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
    public class PropertySortedEventTableCoercedFactory : PropertySortedEventTableFactory
    {
        protected readonly Type coercionType;
    
       /// <summary>Ctor. </summary>
       /// <param name="streamNum">the stream number that is indexed</param>
       /// <param name="eventType">types of events indexed</param>
       /// <param name="propertyName">property names to use for indexing</param>
       /// <param name="coercionType">property types</param>
        public PropertySortedEventTableCoercedFactory(int streamNum, EventType eventType, String propertyName, Type coercionType)
            : base(streamNum, eventType, propertyName)
        {
            this.coercionType = coercionType;
        }
    
        public override EventTable[] MakeEventTables(EventTableFactoryTableIdent tableIdent, ExprEvaluatorContext exprEvaluatorContext)
        {
            var organization = Organization;
            return new EventTable[]
            {
                new PropertySortedEventTableCoerced(PropertyGetter, organization, coercionType)
            };
        }
    
        public override String ToString()
        {
            return "PropertySortedEventTableCoerced" +
                    " streamNum=" + StreamNum +
                    " propertyName=" + PropertyName +
                    " coercionType=" + coercionType;
        }

        protected override EventTableOrganization Organization
        {
            get
            {
                return new EventTableOrganization(null, false, true, StreamNum, new String[] { PropertyName }, EventTableOrganizationType.BTREE);
            }
        }

        public Type ProviderClass
        {
            get { return typeof (PropertySortedEventTableCoerced); }
        }
    }
}
