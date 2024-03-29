///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.support;

namespace com.espertech.esper.regressionlib.support.bean
{
    public class SupportBeanWithEnum
    {
        public SupportBeanWithEnum(
            string theString,
            SupportEnum supportEnum)
        {
            TheString = theString;
            SupportEnum = supportEnum;
        }

        public string TheString { get; }

        public SupportEnum SupportEnum { get; }
    }
} // end of namespace