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
    using DataMap = IDictionary<string, object>;

    public abstract class SupportSubscriberRowByRowMapBase : SupportSubscriberBase
    {
        private readonly List<DataMap> _indicateIStream = new List<DataMap>();
        private readonly List<DataMap> _indicateRStream = new List<DataMap>();

        protected SupportSubscriberRowByRowMapBase(bool requiresStatementDelivery)
            : base(requiresStatementDelivery)
        {
        }

        protected void AddIndicationIStream(DataMap row)
        {
            _indicateIStream.Add(row);
        }

        protected void AddIndicationRStream(DataMap row)
        {
            _indicateRStream.Add(row);
        }

        protected void AddIndicationIStream(EPStatement stmt, DataMap row)
        {
            _indicateIStream.Add(row);
            AddStmtIndication(stmt);
        }

        protected void AddIndicationRStream(EPStatement stmt, DataMap row)
        {
            _indicateRStream.Add(row);
            AddStmtIndication(stmt);
        }

        public void AssertIRStreamAndReset(
            EPStatement stmt,
            string[] fields,
            object[] expectedIStream,
            object[] expectedRStream)
        {
            AssertStmtMultipleReceived(stmt, 1 + (expectedRStream == null ? 0 : 1));

            Assert.AreEqual(1, _indicateIStream.Count);
            EPAssertionUtil.AssertPropsMap(_indicateIStream[0], fields, expectedIStream);

            if (expectedRStream == null)
            {
                Assert.IsTrue(_indicateRStream.IsEmpty());
            }
            else
            {
                Assert.AreEqual(1, _indicateRStream.Count);
                EPAssertionUtil.AssertPropsMap(_indicateRStream[0], fields, expectedRStream);
            }

            _indicateIStream.Clear();
            _indicateRStream.Clear();
            ResetStmts();
        }
    }
} // end of namespace
