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
    [Serializable]
    public class SupportSimpleBeanOne
    {
        public SupportSimpleBeanOne(String s1, int i1, double d1, long l1)
        {
            S1 = s1;
            I1 = i1;
            D1 = d1;
            L1 = l1;
        }

        public string S1 { get; private set; }

        public int I1 { get; private set; }

        public double D1 { get; private set; }

        public long L1 { get; private set; }
    }
}