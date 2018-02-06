///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.supportunit.util
{
    public class SupportCtorIntObjectArray
    {
        private Object[] arguments;
        private int someValue;
    
        public SupportCtorIntObjectArray(int someValue)
        {
            this.someValue = someValue;
        }
    
        public SupportCtorIntObjectArray(Object[] arguments)
        {
            this.arguments = arguments;
        }

        public object[] Arguments
        {
            get { return arguments; }
        }

        public int SomeValue
        {
            get { return someValue; }
        }
    }
}
