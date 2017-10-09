///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.epl;
using com.espertech.esper.supportunit.@events;

using NUnit.Framework;

namespace com.espertech.esper.epl.join.@base
{
    [TestFixture]
    public class TestQueryPlanExecStrategy
    {
        private ExecNodeQueryStrategy _strategy;
        private SupportQueryExecNode _supportQueryExecNode;

        [SetUp]
        public void SetUp()
        {
            _supportQueryExecNode = new SupportQueryExecNode(null);
            _strategy = new ExecNodeQueryStrategy(4, 20, _supportQueryExecNode);
        }

        [Test]
        public void TestLookup()
        {
            EventBean lookupEvent = SupportEventBeanFactory.CreateObject(new SupportBean());

            _strategy.Lookup(new[] { lookupEvent }, null, null);

            Assert.AreSame(lookupEvent, _supportQueryExecNode.LastPrefillPath[4]);
        }
    }
}
