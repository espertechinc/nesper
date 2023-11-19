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
    public class SupportBeanSourceEvent
    {
        private readonly SupportBean_S0[] _s0Arr;

        public SupportBeanSourceEvent(
            SupportBean sb,
            SupportBean_S0[] s0Arr)
        {
            Sb = sb;
            _s0Arr = s0Arr;
        }

        public SupportBean Sb { get; }

        public SupportBean_S0[] GetS0Arr()
        {
            return _s0Arr;
        }
    }
} // end of namespace