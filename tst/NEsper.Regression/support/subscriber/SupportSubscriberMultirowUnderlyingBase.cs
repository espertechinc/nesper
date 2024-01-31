///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.runtime.client;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.support.subscriber
{
    public abstract class SupportSubscriberMultirowUnderlyingBase : SupportSubscriberBase
    {
        private readonly List<UniformPair<object[]>> indicate = new List<UniformPair<object[]>>();

        public SupportSubscriberMultirowUnderlyingBase(bool requiresStatementDelivery) : base(requiresStatementDelivery)
        {
        }

        public void AddIndication(
            object[] newEvents,
            object[] oldEvents)
        {
            indicate.Add(new UniformPair<object[]>(newEvents, oldEvents));
        }

        public void AddIndication(
            EPStatement stmt,
            object[] newEvents,
            object[] oldEvents)
        {
            indicate.Add(new UniformPair<object[]>(newEvents, oldEvents));
            AddStmtIndication(stmt);
        }

        public void AssertOneReceivedAndReset(
            EPStatement stmt,
            object[] firstExpected,
            object[] secondExpected)
        {
            AssertStmtOneReceived(stmt);

            ClassicAssert.AreEqual(1, indicate.Count);
            var result = indicate[0];
            AssertValues(firstExpected, result.First);
            AssertValues(secondExpected, result.Second);

            Reset();
        }

        private void AssertValues(
            object[] expected,
            object[] received)
        {
            if (expected == null) {
                ClassicAssert.IsNull(received);
                return;
            }

            EPAssertionUtil.AssertEqualsExactOrder(expected, received);
        }

        private void Reset()
        {
            ResetStmts();
            indicate.Clear();
        }
    }
} // end of namespace