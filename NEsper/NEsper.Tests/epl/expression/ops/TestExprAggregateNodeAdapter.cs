///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.compat.container;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.support;
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.supportunit.epl;
using com.espertech.esper.supportunit.util;
using com.espertech.esper.supportunit.view;

using NUnit.Framework;

namespace com.espertech.esper.epl.expression.ops
{
    [TestFixture]
    public abstract class TestExprAggregateNodeAdapter
    {
        protected ExprAggregateNode ValidatedNodeToTest;

        private IContainer _container;

        [SetUp]
        public virtual void SetUp()
        {
            _container = SupportContainer.Reset();
        }

        public virtual void TestEvaluate()
        {
            SupportAggregationResultFuture future = new SupportAggregationResultFuture(new Object[] { 10, 20 });
            ValidatedNodeToTest.SetAggregationResultFuture(future, 1);
            AgentInstanceContext agentInstanceContext = SupportStatementContextFactory.MakeAgentInstanceContext(_container);

            Assert.AreEqual(20, ValidatedNodeToTest.Evaluate(new EvaluateParams(null, false, agentInstanceContext)));
        }
    }
}