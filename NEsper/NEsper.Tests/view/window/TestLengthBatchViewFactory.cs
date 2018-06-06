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
using com.espertech.esper.supportunit.util;
using com.espertech.esper.supportunit.view;
using com.espertech.esper.view.std;

using NUnit.Framework;

namespace com.espertech.esper.view.window
{
    [TestFixture]
    public class TestLengthBatchViewFactory 
    {
        private LengthBatchViewFactory _factory;
        private IContainer _container;
    
        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
            _factory = new LengthBatchViewFactory();
        }
    
        [Test]
        public void TestSetParameters()
        {
            TryParameter(new Object[] {short.Parse("10")}, 10);
            TryParameter(new Object[] {100}, 100);
    
            TryInvalidParameter("TheString");
            TryInvalidParameter(true);
            TryInvalidParameter(1.1d);
            TryInvalidParameter(0);
            TryInvalidParameter(1000L);
        }
    
        [Test]
        public void TestCanReuse()
        {
            AgentInstanceContext agentInstanceContext = SupportStatementContextFactory.MakeAgentInstanceContext(_container);
            _factory.SetViewParameters(SupportStatementContextFactory.MakeViewContext(_container), TestViewSupport.ToExprListBean(new Object[] { 1000 }));
            Assert.False(_factory.CanReuse(new FirstElementView(null), agentInstanceContext));
            Assert.False(_factory.CanReuse(new LengthBatchView(null, _factory, 1, null), agentInstanceContext));
            Assert.True(_factory.CanReuse(new LengthBatchView(null, _factory, 1000, null), agentInstanceContext));
        }
    
        private void TryInvalidParameter(Object param)
        {
            try
            {
                _factory.SetViewParameters(SupportStatementContextFactory.MakeViewContext(_container), TestViewSupport.ToExprListBean(new Object[] {param}));
                Assert.Fail();
            }
            catch (ViewParameterException)
            {
                // expected
            }
        }
    
        private void TryParameter(Object[] param, int size)
        {
            var factory = new LengthBatchViewFactory();
            factory.SetViewParameters(SupportStatementContextFactory.MakeViewContext(_container), TestViewSupport.ToExprListBean(param));
            var view = (LengthBatchView) factory.MakeView(SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext(_container));
            Assert.AreEqual(size, view.Size);
        }
    }
}
