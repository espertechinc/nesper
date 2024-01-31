///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.bean
{
    public class SupportSimpleBeanOne
    {
        public SupportSimpleBeanOne(
            string s1,
            int i1)
        {
            S1 = s1;
            I1 = i1;
        }

        public SupportSimpleBeanOne(
            string s1,
            int i1,
            double d1,
            long l1)
        {
            S1 = s1;
            I1 = i1;
            D1 = d1;
            L1 = l1;
        }

        public string S1 { get; }

        public int I1 { get; }

        public double D1 { get; }

        public long L1 { get; }
    }
} // end of namespace