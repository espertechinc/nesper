///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.regressionlib.support.patternassert
{
    public class EventDescriptor
    {
        private readonly IDictionary<string, object> eventProperties;

        public EventDescriptor()
        {
            eventProperties = new Dictionary<string, object>();
        }

        public IDictionary<string, object> GetEventProperties()
        {
            return eventProperties;
        }

        public void Put(
            string propertyName,
            object value)
        {
            eventProperties.Put(propertyName, value);
        }
    }
} // end of namespace