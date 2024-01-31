///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.runtime.client;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.support.subscriber
{
    public abstract class SupportSubscriberRowByRowObjectArrayBase : SupportSubscriberBase
    {
        private readonly List<object[]> indicate = new List<object[]>();

        protected SupportSubscriberRowByRowObjectArrayBase(bool requiresStatementDelivery) : base(
            requiresStatementDelivery)
        {
        }

        protected void AddIndication(object[] row)
        {
            indicate.Add(row);
        }

        protected void AddIndication(
            EPStatement stmt,
            object[] row)
        {
            indicate.Add(row);
            AddStmtIndication(stmt);
        }

        public void AssertOneAndReset(
            EPStatement stmt,
            object[] expected)
        {
            AssertStmtOneReceived(stmt);

            ClassicAssert.AreEqual(1, indicate.Count);
            EPAssertionUtil.AssertEqualsAnyOrder(expected, indicate[0]);

            indicate.Clear();
            ResetStmts();
        }
    }
} // end of namespace