///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.support;

namespace com.espertech.esper.regressionlib.support.bean
{
    public class SupportEventWithCtorSameType
    {
        public SupportEventWithCtorSameType(
            SupportBean b1,
            SupportBean b2)
        {
            B1 = b1;
            B2 = b2;
        }

        public SupportBean B1 { get; }

        public SupportBean B2 { get; }
    }
} // end of namespace