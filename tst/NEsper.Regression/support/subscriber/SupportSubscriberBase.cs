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
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.support.subscriber
{
    public abstract class SupportSubscriberBase
    {
        private readonly bool requiresStatementDelivery;
        private readonly List<EPStatement> statements = new List<EPStatement>();

        public SupportSubscriberBase(bool requiresStatementDelivery)
        {
            this.requiresStatementDelivery = requiresStatementDelivery;
        }

        protected void AddStmtIndication(EPStatement statement)
        {
            statements.Add(statement);
        }

        protected void AssertStmtOneReceived(EPStatement stmt)
        {
            if (requiresStatementDelivery) {
                EPAssertionUtil.AssertEqualsExactOrder(new[] {stmt}, statements.ToArray());
            }
            else {
                ClassicAssert.IsTrue(statements.IsEmpty());
            }
        }

        protected void AssertStmtMultipleReceived(
            EPStatement stmt,
            int size)
        {
            if (requiresStatementDelivery) {
                ClassicAssert.AreEqual(size, statements.Count);
                foreach (var indicated in statements) {
                    ClassicAssert.AreSame(indicated, stmt);
                }
            }
            else {
                ClassicAssert.IsTrue(statements.IsEmpty());
            }
        }

        protected void AssertStmtNoneReceived()
        {
            ClassicAssert.IsTrue(statements.IsEmpty());
        }

        protected void ResetStmts()
        {
            statements.Clear();
        }
    }
} // end of namespace