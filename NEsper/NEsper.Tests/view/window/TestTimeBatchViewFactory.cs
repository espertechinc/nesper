///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.core.support;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.support.view;
using com.espertech.esper.view.std;

using NUnit.Framework;

namespace com.espertech.esper.view.window
{
    [TestFixture]
    public class TestTimeBatchViewFactory 
    {
        private TimeBatchViewFactory _factory;
    
        [SetUp]
        public void SetUp()
        {
            _factory = new TimeBatchViewFactory();
        }
    
        [Test]
        public void TestSetParameters()
        {
            TryParameter(new Object[] {2d}, 2000, null);
            TryParameter(new Object[] {4}, 4000, null);
            TryParameter(new Object[] {3.3d}, 3300, null);
            TryParameter(new Object[] {new float?(1.1f)}, 1100, null);
            TryParameter(new Object[] {99.9d, 364466464L}, 99900, 364466464L);
    
            TryInvalidParameter("TheString");
            TryInvalidParameter(true);
            TryInvalidParameter(0);
        }
    
        [Test]
        public void TestCanReuse()
        {
            _factory.SetViewParameters(SupportStatementContextFactory.MakeViewContext(), TestViewSupport.ToExprListBean(new Object[] {1000}));
            Assert.IsFalse(_factory.CanReuse(new FirstElementView(null)));
            Assert.IsFalse(_factory.CanReuse(new TimeBatchView(_factory, SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext(), new ExprTimePeriodEvalDeltaConstMsec(1), null, false, false, null)));
            Assert.IsTrue(_factory.CanReuse(new TimeBatchView(_factory, SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext(), new ExprTimePeriodEvalDeltaConstMsec(1000000), null, false, false, null)));
    
            _factory.SetViewParameters(SupportStatementContextFactory.MakeViewContext(), TestViewSupport.ToExprListBean(new Object[] {1000, 2000L}));
            Assert.IsFalse(_factory.CanReuse(new TimeBatchView(_factory, SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext(), new ExprTimePeriodEvalDeltaConstMsec(1), null, false, false, null)));
            Assert.IsTrue(_factory.CanReuse(new TimeBatchView(_factory, SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext(), new ExprTimePeriodEvalDeltaConstMsec(1000000), 2000L, false, false, null)));
        }
    
        private void TryInvalidParameter(Object param)
        {
            try
            {
                TimeBatchViewFactory factory = new TimeBatchViewFactory();
                factory.SetViewParameters(SupportStatementContextFactory.MakeViewContext(), TestViewSupport.ToExprListBean(new Object[] {param}));
                Assert.Fail();
            }
            catch (ViewParameterException ex)
            {
                // expected
            }
        }
    
        private void TryParameter(Object[] param, long msec, long? referencePoint)
        {
            TimeBatchViewFactory factory = new TimeBatchViewFactory();
            factory.SetViewParameters(SupportStatementContextFactory.MakeViewContext(), TestViewSupport.ToExprListBean(param));
            TimeBatchView view = (TimeBatchView) factory.MakeView(SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext());
            Assert.IsTrue(new ExprTimePeriodEvalDeltaConstMsec(msec).EqualsTimePeriod(view.TimeDeltaComputation));
            Assert.AreEqual(referencePoint, view.InitialReferencePoint);
        }
    }
}
