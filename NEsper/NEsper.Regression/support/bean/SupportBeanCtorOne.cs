///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;

namespace com.espertech.esper.regressionlib.support.bean
{
    public class SupportBeanCtorOne
    {
        public SupportBeanCtorOne(
            string theString,
            int? intBoxed,
            int intPrimitive,
            bool boolPrimitive)
        {
            TheString = theString;
            IntBoxed = intBoxed;
            IntPrimitive = intPrimitive;
            BoolPrimitive = boolPrimitive;
        }

        public SupportBeanCtorOne(
            string theString,
            int? intBoxed,
            int intPrimitive)
        {
            TheString = theString;
            IntBoxed = intBoxed;
            IntPrimitive = intPrimitive;
            BoolPrimitive = false;
        }

        public SupportBeanCtorOne(
            string theString,
            int? intBoxed)
        {
            TheString = theString;
            IntBoxed = intBoxed;
            IntPrimitive = 99;
            BoolPrimitive = false;
        }

        public SupportBeanCtorOne(string theString)
        {
            throw new EPException("This is a test exception");
        }

        public string TheString { get; }

        public int? IntBoxed { get; }

        public int IntPrimitive { get; }

        public bool BoolPrimitive { get; }
    }
} // end of namespace