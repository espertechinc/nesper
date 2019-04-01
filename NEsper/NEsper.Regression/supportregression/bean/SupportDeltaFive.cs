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
    public class SupportDeltaFive : ISupportDeltaFive
    {
        public SupportDeltaFive(String k0, String p1, String p5)
        {
            K0 = k0;
            P1 = p1;
            P5 = p5;
        }

        public string K0 { get; }

        public string P1 { get; }

        public string P5 { get; }
    }
}
