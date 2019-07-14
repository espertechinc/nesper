///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;

namespace com.espertech.esper.regressionlib.support.bean
{
    [Serializable]
    public class SupportInstanceMethodBean
    {
        public SupportInstanceMethodBean(int x)
        {
            X = x;
        }

        public int X { get; }

        public bool MyInstanceMethodAlwaysTrue()
        {
            return true;
        }

        public bool MyInstanceMethodEventBean(
            EventBean @event,
            string propertyName,
            int expected)
        {
            var value = @event.Get(propertyName);
            return value.Equals(expected);
        }
    }
} // end of namespace