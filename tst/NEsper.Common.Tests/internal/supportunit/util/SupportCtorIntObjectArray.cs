///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.supportunit.util
{
    public class SupportCtorIntObjectArray
    {
        public SupportCtorIntObjectArray(int someValue)
        {
            SomeValue = someValue;
        }

        public SupportCtorIntObjectArray(object[] arguments)
        {
            Arguments = arguments;
        }

        public object[] Arguments { get; }

        public int SomeValue { get; }
    }
} // end of namespace
