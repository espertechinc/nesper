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
    public abstract class SupportSubscriberBase
    {
        private readonly bool _requiresStatementDelivery;
        private readonly List<EPStatement> _statements = new List<EPStatement>();

        protected SupportSubscriberBase(bool requiresStatementDelivery)
        {
            _requiresStatementDelivery = requiresStatementDelivery;
        }

        protected void AddStmtIndication(EPStatement statement)
        {
            _statements.Add(statement);
        }

        protected void AssertStmtOneReceived(EPStatement stmt)
        {
            if (_requiresStatementDelivery)
            {
                EPAssertionUtil.AssertEqualsExactOrder(
                    new EPStatement[]
                    {
                        stmt
                    }, _statements.ToArray());
            }
            else
            {
                Assert.IsTrue(_statements.IsEmpty());
            }
        }

        protected void AssertStmtMultipleReceived(EPStatement stmt, int size)
        {
            if (_requiresStatementDelivery)
            {
                Assert.AreEqual(size, _statements.Count);
                foreach (EPStatement indicated in _statements)
                {
                    Assert.AreSame(indicated, stmt);
                }
            }
            else
            {
                Assert.IsTrue(_statements.IsEmpty());
            }
        }

        protected void AssertStmtNoneReceived()
        {
            Assert.IsTrue(_statements.IsEmpty());
        }

        protected void ResetStmts()
        {
            _statements.Clear();
        }
    }
} // end of namespace
