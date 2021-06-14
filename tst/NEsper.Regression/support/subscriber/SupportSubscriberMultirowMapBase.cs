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
using com.espertech.esper.compat.collections;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.support.subscriber
{
    public abstract class SupportSubscriberMultirowMapBase : SupportSubscriberBase
    {
        private readonly List<UniformPair<IDictionary<string, object>[]>> indicate =
            new List<UniformPair<IDictionary<string, object>[]>>();

        protected SupportSubscriberMultirowMapBase(bool requiresStatementDelivery) : base(requiresStatementDelivery)
        {
        }

        protected void AddIndication(
            IDictionary<string, object>[] newEvents,
            IDictionary<string, object>[] oldEvents)
        {
            indicate.Add(new UniformPair<IDictionary<string, object>[]>(newEvents, oldEvents));
        }

        protected void AddIndication(
            EPStatement statement,
            IDictionary<string, object>[] newEvents,
            IDictionary<string, object>[] oldEvents)
        {
            indicate.Add(new UniformPair<IDictionary<string, object>[]>(newEvents, oldEvents));
            AddStmtIndication(statement);
        }

        public void AssertNoneReceived()
        {
            Assert.IsTrue(indicate.IsEmpty());
            AssertStmtNoneReceived();
        }

        public void AssertOneReceivedAndReset(
            EPStatement stmt,
            string[] fields,
            object[][] firstExpected,
            object[][] secondExpected)
        {
            AssertStmtOneReceived(stmt);

            Assert.AreEqual(1, indicate.Count);
            var result = indicate[0];
            AssertValues(fields, firstExpected, result.First);
            AssertValues(fields, secondExpected, result.Second);

            Reset();
        }

        private void AssertValues(
            string[] fields,
            object[][] expected,
            IDictionary<string, object>[] received)
        {
            if (expected == null) {
                Assert.IsNull(received);
                return;
            }

            EPAssertionUtil.AssertPropsPerRow(received, fields, expected);
        }

        private void Reset()
        {
            ResetStmts();
            indicate.Clear();
        }
    }
} // end of namespace