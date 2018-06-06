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

using NUnit.Framework;


namespace com.espertech.esper.view.std
{
    [TestFixture]
    public class TestLastElementViewFactory 
    {
        private LastElementViewFactory _factory;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
            _factory = new LastElementViewFactory();
        }
    
        [Test]
        public void TestSetParameters()
        {
            TryParameter(new Object[] {});
            TryInvalidParameter(1.1d);
        }
    
        [Test]
        public void TestCanReuse()
        {
            AgentInstanceContext agentInstanceContext = SupportStatementContextFactory.MakeAgentInstanceContext(_container);
            Assert.IsFalse(_factory.CanReuse(new FirstElementView(null), agentInstanceContext));
            Assert.IsTrue(_factory.CanReuse(new LastElementView(null), agentInstanceContext));
        }
    
        private void TryInvalidParameter(Object param)
        {
            try
            {
                LastElementViewFactory factory = new LastElementViewFactory();
                factory.SetViewParameters(SupportStatementContextFactory.MakeViewContext(_container), TestViewSupport.ToExprListBean(new Object[] {param}));
                Assert.Fail();
            }
            catch (ViewParameterException)
            {
                // expected
            }
        }
    
        private void TryParameter(Object[] param)
        {
            LastElementViewFactory factory = new LastElementViewFactory();
            factory.SetViewParameters(SupportStatementContextFactory.MakeViewContext(_container), TestViewSupport.ToExprListBean(param));
            Assert.IsTrue(factory.MakeView(SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext(_container)) is LastElementView);
        }
    }
}
