///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client
{
    public static class EventBeanExtensions
    {
        public static EventBean AsEventBean(this object value)
        {
            if (value == null)
            {
                return null;
            }
            else if (value is EventBean eventBean)
            {
                return eventBean;
            }
            else
            {
                throw new ArgumentException("not an event bean", nameof(value));
            }
        }
    }
}