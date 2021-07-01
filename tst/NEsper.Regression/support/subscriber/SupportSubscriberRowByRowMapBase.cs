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
    public abstract class SupportSubscriberRowByRowMapBase : SupportSubscriberBase
    {
        private readonly List<IDictionary<string, object>> indicateIStream = new List<IDictionary<string, object>>();
        private readonly List<IDictionary<string, object>> indicateRStream = new List<IDictionary<string, object>>();

        public SupportSubscriberRowByRowMapBase(bool requiresStatementDelivery) : base(requiresStatementDelivery)
        {
        }

        protected void AddIndicationIStream(IDictionary<string, object> row)
        {
            indicateIStream.Add(row);
        }

        protected void AddIndicationRStream(IDictionary<string, object> row)
        {
            indicateRStream.Add(row);
        }

        protected void AddIndicationIStream(
            EPStatement stmt,
            IDictionary<string, object> row)
        {
            indicateIStream.Add(row);
            AddStmtIndication(stmt);
        }

        protected void AddIndicationRStream(
            EPStatement stmt,
            IDictionary<string, object> row)
        {
            indicateRStream.Add(row);
            AddStmtIndication(stmt);
        }

        public void AssertIRStreamAndReset(
            EPStatement stmt,
            string[] fields,
            object[] expectedIStream,
            object[] expectedRStream)
        {
            AssertStmtMultipleReceived(stmt, 1 + (expectedRStream == null ? 0 : 1));

            Assert.AreEqual(1, indicateIStream.Count);
            EPAssertionUtil.AssertPropsMap(indicateIStream[0], fields, expectedIStream);

            if (expectedRStream == null) {
                Assert.IsTrue(indicateRStream.IsEmpty());
            }
            else {
                Assert.AreEqual(1, indicateRStream.Count);
                EPAssertionUtil.AssertPropsMap(indicateRStream[0], fields, expectedRStream);
            }

            indicateIStream.Clear();
            indicateRStream.Clear();
            ResetStmts();
        }
    }
} // end of namespace