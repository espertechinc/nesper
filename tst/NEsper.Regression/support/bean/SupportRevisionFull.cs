///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.bean
{
    public class SupportRevisionFull : ISupportRevisionFull
    {
        public SupportRevisionFull(
            string k0,
            string p0,
            string p1,
            string p2,
            string p3,
            string p4,
            string p5)
        {
            K0 = k0;
            P0 = p0;
            P1 = p1;
            P2 = p2;
            P3 = p3;
            P4 = p4;
            P5 = p5;
        }

        public SupportRevisionFull(
            string k0,
            string p1,
            string p5)
        {
            K0 = k0;
            P0 = null;
            P1 = p1;
            P2 = null;
            P3 = null;
            P4 = null;
            P5 = p5;
        }

        public string P2 { get; }

        public string P3 { get; }

        public string P4 { get; }

        public string P5 { get; }

        public string K0 { get; }

        public string P0 { get; }

        public string P1 { get; }
    }
} // end of namespace