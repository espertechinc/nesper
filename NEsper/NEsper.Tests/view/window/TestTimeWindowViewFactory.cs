///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.support.view;
using com.espertech.esper.view.std;

using NUnit.Framework;

namespace com.espertech.esper.view.window
{
    [TestFixture]
    public class TestTimeWindowViewFactory 
    {
        private TimeWindowViewFactory _factory;
    
        [SetUp]
        public void SetUp()
        {
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
            _factory.SetViewParameters(SupportStatementContextFactory.MakeViewContext(), TestViewSupport.ToExprListBean(new Object[] {1000}));
            Assert.IsFalse(_factory.CanReuse(new FirstElementView(null)));
            Assert.IsFalse(_factory.CanReuse(new TimeBatchView(null, SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext(), new ExprTimePeriodEvalDeltaConstMsec(1000), null, false, false, null)));
            Assert.IsTrue(_factory.CanReuse(new TimeWindowView(SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext(), _factory, new ExprTimePeriodEvalDeltaConstMsec(1000000), null)));
        }
    
        private void TryInvalidParameter(Object param)
        {
            try
            {
                var factory = new TimeWindowViewFactory();
                factory.SetViewParameters(SupportStatementContextFactory.MakeViewContext(), TestViewSupport.ToExprListBean(new Object[] {param}));
                Assert.Fail();
            }
            catch (ViewParameterException ex)
            {
                // expected
            }
        }
    
        private void TryParameter(Object param, long msec)
        {
            var factory = new TimeWindowViewFactory();
            factory.SetViewParameters(SupportStatementContextFactory.MakeViewContext(), TestViewSupport.ToExprListBean(new Object[] {param}));
            var view = (TimeWindowView) factory.MakeView(SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext());
            Assert.AreEqual(msec, view.TimeDeltaComputation.DeltaMillisecondsAdd(0));
        }
    }
}
