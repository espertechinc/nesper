///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.core.context.util;
using com.espertech.esper.core.support;
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.support.epl;
using com.espertech.esper.support.view;

using NUnit.Framework;

namespace com.espertech.esper.epl.expression
{
    [TestFixture]
    public abstract class TestExprAggregateNodeAdapter
    {
        protected ExprAggregateNode ValidatedNodeToTest;

        public virtual void TestEvaluate()
        {
            SupportAggregationResultFuture future = new SupportAggregationResultFuture(new Object[] { 10, 20 });
            ValidatedNodeToTest.SetAggregationResultFuture(future, 1);
            AgentInstanceContext agentInstanceContext = SupportStatementContextFactory.MakeAgentInstanceContext();

            Assert.AreEqual(20, ValidatedNodeToTest.Evaluate(new EvaluateParams(null, false, agentInstanceContext)));
        }
    }
}