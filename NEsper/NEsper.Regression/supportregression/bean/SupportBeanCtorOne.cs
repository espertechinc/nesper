///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.supportregression.bean
{
    public class SupportBeanCtorOne
    {
        public SupportBeanCtorOne(String stringValue,
                                  int? intBoxed,
                                  int intPrimitive,
                                  bool boolPrimitive)
        {
            TheString = stringValue;
            IntBoxed = intBoxed;
            IntPrimitive = intPrimitive;
            IsBoolPrimitive = boolPrimitive;
        }

        public SupportBeanCtorOne(String stringValue,
                                  int? intBoxed,
                                  int intPrimitive)
        {
            TheString = stringValue;
            IntBoxed = intBoxed;
            IntPrimitive = intPrimitive;
            IsBoolPrimitive = false;
        }

        public SupportBeanCtorOne(String stringValue,
                                  int? intBoxed)
        {
            TheString = stringValue;
            IntBoxed = intBoxed;
            IntPrimitive = 99;
            IsBoolPrimitive = false;
        }

        public SupportBeanCtorOne(String stringValue)
        {
            throw new ApplicationException("This is a test exception");
        }

        public string TheString { get; private set; }

        public int? IntBoxed { get; private set; }

        public int IntPrimitive { get; private set; }

        public bool IsBoolPrimitive { get; private set; }
    }
}