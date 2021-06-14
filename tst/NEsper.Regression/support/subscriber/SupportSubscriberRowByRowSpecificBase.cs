///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.support.subscriber
{
    public abstract class SupportSubscriberRowByRowSpecificBase : SupportSubscriberBase
    {
        private readonly List<object[]> indicate = new List<object[]>();

        public SupportSubscriberRowByRowSpecificBase(bool requiresStatementDelivery) : base(requiresStatementDelivery)
        {
        }

        protected void AddIndication(
            EPStatement statement,
            object[] values)
        {
            indicate.Add(values);
            AddStmtIndication(statement);
        }

        protected void AddIndication(object[] values)
        {
            indicate.Add(values);
        }

        public void Reset()
        {
            indicate.Clear();
            ResetStmts();
        }

        public void AssertOneReceivedAndReset(
            EPStatement stmt,
            object[] objects)
        {
            EPAssertionUtil.AssertEqualsExactOrder(new[] {objects}, indicate);
            AssertStmtOneReceived(stmt);
            Reset();
        }

        public void AssertMultipleReceivedAndReset(
            EPStatement stmt,
            object[][] objects)
        {
            EPAssertionUtil.AssertEqualsExactOrder(objects, indicate);
            AssertStmtMultipleReceived(stmt, objects.Length);
            Reset();
        }

        public void AssertNoneReceived()
        {
            Assert.IsTrue(indicate.IsEmpty());
            AssertStmtNoneReceived();
        }
    }
} // end of namespace