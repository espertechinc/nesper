///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.bean
{
    public class SupportBeanWithThis
    {
        public SupportBeanWithThis(
            string theString,
            int intPrimitive)
        {
            TheString = theString;
            IntPrimitive = intPrimitive;
        }

        public int IntPrimitive { get; }

        public string TheString { get; }

        public SupportBeanWithThis This => this;
    }
} // end of namespace