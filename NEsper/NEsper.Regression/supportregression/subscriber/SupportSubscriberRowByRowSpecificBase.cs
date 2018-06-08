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
using com.espertech.esper.compat.collections;

using NUnit.Framework;

namespace com.espertech.esper.supportregression.subscriber
{
    public abstract class SupportSubscriberRowByRowSpecificBase : SupportSubscriberBase
    {
        private readonly List<object[]> _indicate = new List<object[]>();

        protected SupportSubscriberRowByRowSpecificBase(bool requiresStatementDelivery)
            : base(requiresStatementDelivery)
        {
        }

        protected void AddIndication(EPStatement statement, object[] values)
        {
            _indicate.Add(values);
            AddStmtIndication(statement);
        }

        protected void AddIndication(object[] values)
        {
            _indicate.Add(values);
        }

        public void Reset()
        {
            _indicate.Clear();
            ResetStmts();
        }

        public void AssertOneReceivedAndReset(EPStatement stmt, object[] objects)
        {
            EPAssertionUtil.AssertEqualsExactOrder(
                new object[][]
                {
                    objects
                }, _indicate);
            AssertStmtOneReceived(stmt);
            Reset();
        }

        public void AssertMultipleReceivedAndReset(EPStatement stmt, object[][] objects)
        {
            EPAssertionUtil.AssertEqualsExactOrder(objects, _indicate);
            AssertStmtMultipleReceived(stmt, objects.Length);
            Reset();
        }

        public void AssertNoneReceived()
        {
            Assert.IsTrue(_indicate.IsEmpty());
            AssertStmtNoneReceived();
        }
    }
} // end of namespace
