///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;

using NUnit.Framework;

namespace com.espertech.esper.supportregression.subscriber
{
    public abstract class SupportSubscriberMultirowObjectArrayBase : SupportSubscriberBase
    {
        private readonly List<UniformPair<object[][]>> indicate = new List<UniformPair<object[][]>>();

        protected SupportSubscriberMultirowObjectArrayBase(bool requiresStatementDelivery)
            : base(requiresStatementDelivery)
        {
        }

        protected void AddIndication(object[][] newEvents, object[][] oldEvents)
        {
            indicate.Add(new UniformPair<object[][]>(newEvents, oldEvents));
        }

        protected void AddIndication(EPStatement statement, object[][] newEvents, object[][] oldEvents)
        {
            indicate.Add(new UniformPair<object[][]>(newEvents, oldEvents));
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
            UniformPair<object[][]> result = indicate[0];
            AssertValues(fields, firstExpected, result.First);
            AssertValues(fields, secondExpected, result.Second);

            Reset();
        }

        public List<UniformPair<object[][]>> GetAndResetIndicateArr()
        {
            List<UniformPair<object[][]>> result = new List<UniformPair<object[][]>>(indicate);
            Reset();
            return result;
        }

        private void AssertValues(string[] fields, object[][] expected, object[][] received)
        {
            if (expected == null)
            {
                Assert.IsNull(received);
                return;
            }
            EPAssertionUtil.AssertEqualsExactOrder(received, fields, expected);
        }

        private void Reset()
        {
            ResetStmts();
            indicate.Clear();
        }
    }
} // end of namespace
