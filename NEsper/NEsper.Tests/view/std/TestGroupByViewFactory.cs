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
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.epl;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.supportunit.util;
using com.espertech.esper.supportunit.view;

using NUnit.Framework;

namespace com.espertech.esper.view.std
{
    [TestFixture]
    public class TestGroupByViewFactory
    {
        private GroupByViewFactory _factory;
        private ViewFactoryContext _viewFactoryContext;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
            _factory = new GroupByViewFactory();
            _viewFactoryContext = new ViewFactoryContext(
                SupportStatementContextFactory.MakeContext(_container), 1, null, null, false, -1, false);
        }

        private void TryInvalidParameter(Object[] parameters)
        {
            try
            {
                var factory = new GroupByViewFactory();

                factory.SetViewParameters(_viewFactoryContext, TestViewSupport.ToExprListBean(parameters));
                factory.Attach(
                    SupportEventTypeFactory.CreateBeanType(typeof (SupportBean)),
                    SupportStatementContextFactory.MakeContext(_container), null, null);
                Assert.Fail();
            }
            catch (ViewParameterException)
            {
                // expected
            }
        }

        private void TryParameter(Object[] parameters, String[] fieldNames)
        {
            var factory = new GroupByViewFactory();

            factory.SetViewParameters(_viewFactoryContext, TestViewSupport.ToExprListBean(parameters));
            factory.Attach(SupportEventTypeFactory.CreateBeanType(typeof (SupportBean)),
                           SupportStatementContextFactory.MakeContext(_container), null, null);
            var view = (GroupByView) factory.MakeView(
                SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext(_container));

            Assert.AreEqual(fieldNames[0],
                            view.CriteriaExpressions[0].ToExpressionStringMinPrecedenceSafe());
        }

        [Test]
        public void TestAttaches()
        {
            // Should attach to anything as long as the fields exists
            EventType parentType = SupportEventTypeFactory.CreateBeanType(
                typeof (SupportBean));

            _factory.SetViewParameters(_viewFactoryContext, TestViewSupport.ToExprListBean(new Object[] { "IntBoxed" }));
            _factory.Attach(parentType, SupportStatementContextFactory.MakeContext(_container), null, null);
        }

        [Test]
        public void TestCanReuse()
        {
            AgentInstanceContext agentInstanceContext = SupportStatementContextFactory.MakeAgentInstanceContext(_container);
            _factory.SetViewParameters(_viewFactoryContext, TestViewSupport.ToExprListBean(new Object[] { "TheString", "LongPrimitive" }));
            _factory.Attach(SupportEventTypeFactory.CreateBeanType(typeof(SupportBean)), SupportStatementContextFactory.MakeContext(_container), null, null);
            Assert.IsFalse(_factory.CanReuse(new FirstElementView(null), agentInstanceContext));
            Assert.IsFalse(_factory.CanReuse(new GroupByViewImpl(SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext(_container), SupportExprNodeFactory.MakeIdentNodesBean("TheString"), null), agentInstanceContext));
            Assert.IsTrue(_factory.CanReuse(new GroupByViewImpl(SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext(_container), SupportExprNodeFactory.MakeIdentNodesBean("TheString", "LongPrimitive"), null), agentInstanceContext));

            _factory.SetViewParameters(_viewFactoryContext, TestViewSupport.ToExprListBean(new Object[] { SupportExprNodeFactory.MakeIdentNodesBean("TheString", "LongPrimitive") }));
            Assert.IsFalse(_factory.CanReuse(new GroupByViewImpl(SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext(_container), SupportExprNodeFactory.MakeIdentNodesBean("TheString"), null), agentInstanceContext));
            Assert.IsTrue(_factory.CanReuse(new GroupByViewImpl(SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext(_container), SupportExprNodeFactory.MakeIdentNodesBean("TheString", "LongPrimitive"), null), agentInstanceContext));
        }

        [Test]
        public void TestSetParameters()
        {
            TryParameter(new Object[] { "DoublePrimitive" }, new String[] { "DoublePrimitive" });
            TryParameter(new Object[] { "DoublePrimitive", "LongPrimitive" }, new String[] { "DoublePrimitive", "LongPrimitive" });

            TryInvalidParameter(new Object[] { "TheString", 1.1d });
            TryInvalidParameter(new Object[] { 1.1d });
            TryInvalidParameter(new Object[] { new String[] {} });
            TryInvalidParameter(new Object[] { new String[] {}, new String[] {} });
        }
    }
}