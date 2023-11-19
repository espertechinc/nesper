///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.bean
{
    public class SupportTwoKeyEvent
    {
        public SupportTwoKeyEvent(
            string k1,
            int k2,
            int newValue)
        {
            K1 = k1;
            K2 = k2;
            NewValue = newValue;
        }

        public string K1 { get; }

        public int K2 { get; }

        public int NewValue { get; }
    }
} // end of namespace