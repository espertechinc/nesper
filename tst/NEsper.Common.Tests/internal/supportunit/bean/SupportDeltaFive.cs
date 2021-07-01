///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.supportunit.bean
{
    public class SupportDeltaFive : ISupportDeltaFive
    {
        public SupportDeltaFive(
            string k0,
            string p1,
            string p5)
        {
            K0 = k0;
            P1 = p1;
            P5 = p5;
        }

        public string K0 { get; }

        public string P1 { get; }

        public string P5 { get; }
    }
} // end of namespace
