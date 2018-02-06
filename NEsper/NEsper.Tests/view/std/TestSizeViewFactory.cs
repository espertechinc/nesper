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
using com.espertech.esper.supportunit.util;
using com.espertech.esper.supportunit.view;

using NUnit.Framework;

namespace com.espertech.esper.view.std
{
    [TestFixture]
    public class TestSizeViewFactory 
    {
        private SizeViewFactory _factory;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
            _factory = new SizeViewFactory();
        }
    
        [Test]
        public void TestSetParameters()
        {
            TryParameter(new Object[] {});
        }
    
        [Test]
        public void TestCanReuse()
        {
            AgentInstanceContext agentInstanceContext = SupportStatementContextFactory.MakeAgentInstanceContext(_container);
            Assert.False(_factory.CanReuse(new LastElementView(null), agentInstanceContext));
            EventType type = SizeView.CreateEventType(SupportStatementContextFactory.MakeContext(_container), null, 1);
            Assert.IsTrue(_factory.CanReuse(new SizeView(SupportStatementContextFactory.MakeAgentInstanceContext(_container), type, null), agentInstanceContext));
        }
    
        private void TryParameter(Object[] param)
        {
            SizeViewFactory factory = new SizeViewFactory();
            factory.SetViewParameters(SupportStatementContextFactory.MakeViewContext(_container), TestViewSupport.ToExprListBean(param));
            Assert.IsTrue(factory.MakeView(SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext(_container)) is SizeView);
        }
    }
}
