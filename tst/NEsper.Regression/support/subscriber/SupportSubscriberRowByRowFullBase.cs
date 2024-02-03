///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.support.subscriber
{
    public class SupportSubscriberRowByRowFullBase : SupportSubscriberBase
    {
        private readonly List<object> indicateEnd = new List<object>();
        private readonly List<object[]> indicateIStream = new List<object[]>();
        private readonly List<object[]> indicateRStream = new List<object[]>();
        private readonly List<UniformPair<int?>> indicateStart = new List<UniformPair<int?>>();

        public SupportSubscriberRowByRowFullBase(bool requiresStatementDelivery) : base(requiresStatementDelivery)
        {
        }

        protected void AddUpdateStart(
            int lengthIStream,
            int lengthRStream)
        {
            indicateStart.Add(new UniformPair<int?>(lengthIStream, lengthRStream));
        }

        protected void AddUpdate(object[] values)
        {
            indicateIStream.Add(values);
        }

        protected void AddUpdateRStream(object[] values)
        {
            indicateRStream.Add(values);
        }

        protected void AddUpdateEnd()
        {
            indicateEnd.Add(this);
        }

        protected void AddUpdateStart(
            EPStatement statement,
            int lengthIStream,
            int lengthRStream)
        {
            indicateStart.Add(new UniformPair<int?>(lengthIStream, lengthRStream));
            AddStmtIndication(statement);
        }

        protected void AddUpdate(
            EPStatement statement,
            object[] values)
        {
            indicateIStream.Add(values);
            AddStmtIndication(statement);
        }

        protected void AddUpdateRStream(
            EPStatement statement,
            object[] values)
        {
            indicateRStream.Add(values);
            AddStmtIndication(statement);
        }

        protected void AddUpdateEnd(EPStatement statement)
        {
            indicateEnd.Add(this);
            AddStmtIndication(statement);
        }

        public void Reset()
        {
            indicateStart.Clear();
            indicateIStream.Clear();
            indicateRStream.Clear();
            indicateEnd.Clear();
            ResetStmts();
        }

        public void AssertNoneReceived()
        {
            ClassicAssert.IsTrue(indicateStart.IsEmpty());
            ClassicAssert.IsTrue(indicateIStream.IsEmpty());
            ClassicAssert.IsTrue(indicateRStream.IsEmpty());
            ClassicAssert.IsTrue(indicateEnd.IsEmpty());
        }

        public void AssertOneReceivedAndReset(
            EPStatement stmt,
            int expectedLenIStream,
            int expectedLenRStream,
            object[][] expectedIStream,
            object[][] expectedRStream)
        {
            var stmtCount = 2 + expectedLenIStream + expectedLenRStream;
            AssertStmtMultipleReceived(stmt, stmtCount);

            ClassicAssert.AreEqual(1, indicateStart.Count);
            var pairLength = indicateStart[0];
            ClassicAssert.AreEqual(expectedLenIStream, (int) pairLength.First);
            ClassicAssert.AreEqual(expectedLenRStream, (int) pairLength.Second);

            EPAssertionUtil.AssertEqualsExactOrder(expectedIStream, indicateIStream);
            EPAssertionUtil.AssertEqualsExactOrder(expectedRStream, indicateRStream);

            ClassicAssert.AreEqual(1, indicateEnd.Count);

            Reset();
        }
    }
} // end of namespace