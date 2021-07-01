///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.support;

namespace com.espertech.esper.regressionlib.support.filter
{
    public class FilterTestMultiStmtAssertItem
    {
        public FilterTestMultiStmtAssertItem(
            SupportBean bean,
            params bool[] expectedPerStmt)
        {
            Bean = bean;
            ExpectedPerStmt = expectedPerStmt;
        }

        public SupportBean Bean { get; }

        public bool[] ExpectedPerStmt { get; }

        public static FilterTestMultiStmtAssertItem MakeItem(
            SupportBean bean,
            params bool[] expectedPerStmt)
        {
            return new FilterTestMultiStmtAssertItem(bean, expectedPerStmt);
        }
    }
} // end of namespace