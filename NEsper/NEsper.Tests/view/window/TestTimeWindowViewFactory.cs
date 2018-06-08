///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat.container;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.support;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.supportunit.util;
using com.espertech.esper.view.std;

using NUnit.Framework;

namespace com.espertech.esper.view.window
{
    [TestFixture]
    public class TestTimeWindowViewFactory
    {
        private IContainer _container;
        private TimeWindowViewFactory _factory;
    
        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
            _factory = new TimeWindowViewFactory();
        }
    
        [Test]
        public void TestSetParameters()
        {
            TryParameter(2d, 2000);
            TryParameter(4, 4000);
            TryParameter(3.3d, 3300);
            TryParameter(1.1f, 1100);
    
            TryInvalidParameter("TheString");
            TryInvalidParameter(true);
        }
    
        [Test]
        public void TestCanReuse()
        {
            AgentInstanceContext agentInstanceContext = SupportStatementContextFactory.MakeAgentInstanceContext(_container);
            _factory.SetViewParameters(SupportStatementContextFactory.MakeViewContext(_container), TestViewSupport.ToExprListBean(new Object[] { 1000 }));
            Assert.IsFalse(_factory.CanReuse(new FirstElementView(null), agentInstanceContext));
            Assert.IsFalse(_factory.CanReuse(new TimeBatchView(null, SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext(_container), new ExprTimePeriodEvalDeltaConstGivenDelta(1000), null, false, false, null), agentInstanceContext));
            Assert.IsTrue(_factory.CanReuse(new TimeWindowView(SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext(_container), _factory, new ExprTimePeriodEvalDeltaConstGivenDelta(1000000), null), agentInstanceContext));
        }
    
        private void TryInvalidParameter(Object param)
        {
            try
            {
                var factory = new TimeWindowViewFactory();
                factory.SetViewParameters(SupportStatementContextFactory.MakeViewContext(_container), TestViewSupport.ToExprListBean(new Object[] {param}));
                Assert.Fail();
            }
            catch (ViewParameterException)
            {
                // expected
            }
        }
    
        private void TryParameter(Object param, long msec)
        {
            var factory = new TimeWindowViewFactory();
            factory.SetViewParameters(SupportStatementContextFactory.MakeViewContext(_container), TestViewSupport.ToExprListBean(new Object[] {param}));
            var view = (TimeWindowView) factory.MakeView(SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext(_container));
            Assert.AreEqual(msec, view.TimeDeltaComputation.DeltaAdd(0));
        }
    }
}
