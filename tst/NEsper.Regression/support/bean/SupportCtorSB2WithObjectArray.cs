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
    public class SupportCtorSB2WithObjectArray
    {
        private readonly SupportBean_S2 _sb;

        public SupportCtorSB2WithObjectArray(
            SupportBean_S2 sb,
            object[] arr)
        {
            _sb = sb;
            Arr = arr;
        }

        public object[] Arr { get; }

        public SupportBean_S2 GetSb()
        {
            return _sb;
        }
    }
} // end of namespace