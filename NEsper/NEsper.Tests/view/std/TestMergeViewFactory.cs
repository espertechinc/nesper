///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
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
    public class TestMergeViewFactory 
    {
        private MergeViewFactory _factory;
        private IList<ViewFactory> _parents;
        private ViewFactoryContext _viewFactoryContext; 
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
            _factory = new MergeViewFactory();
    
            _viewFactoryContext = new ViewFactoryContext(SupportStatementContextFactory.MakeContext(_container), 1, null, null, false, -1, false);

            _parents = new List<ViewFactory>();
            GroupByViewFactory groupByView = new GroupByViewFactory();
            groupByView.SetViewParameters(_viewFactoryContext, TestViewSupport.ToExprListMD(new Object[] { "Symbol", "Feed" }));
            groupByView.Attach(SupportEventTypeFactory.CreateBeanType(typeof(SupportMarketDataBean)), SupportStatementContextFactory.MakeContext(_container), null, null);
            _parents.Add(groupByView);
        }
    
        [Test]
        public void TestSetParameters()
        {
            TryParameter(new Object[] { "Symbol", "Feed" }, new String[] { "Symbol", "Feed" });

            TryInvalidParameter(new Object[] { "Symbol", 1.1d });
            TryInvalidParameter(new Object[] {1.1d});
            TryInvalidParameter(new Object[] {new String[] {}});
            TryInvalidParameter(new Object[] {new String[] {}, new String[] {}});
        }
    
        [Test]
        public void TestCanReuse()
        {
            AgentInstanceContext agentInstanceContext = SupportStatementContextFactory.MakeAgentInstanceContext(_container);
            _factory.SetViewParameters(_viewFactoryContext, TestViewSupport.ToExprListMD(new Object[]{"Symbol", "Feed"}));
            _factory.Attach(SupportEventTypeFactory.CreateBeanType(typeof(SupportMarketDataBean)), SupportStatementContextFactory.MakeContext(_container), null, _parents);
            Assert.IsFalse(_factory.CanReuse(new FirstElementView(null), agentInstanceContext));
            Assert.IsFalse(_factory.CanReuse(new MergeView(SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext(_container), SupportExprNodeFactory.MakeIdentNodesMD("Symbol"), null, true), agentInstanceContext));
            Assert.IsTrue(_factory.CanReuse(new MergeView(SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext(_container), SupportExprNodeFactory.MakeIdentNodesMD("Symbol", "Feed"), null, true), agentInstanceContext));
        }
    
        private void TryInvalidParameter(Object[] parameters)
        {
            try
            {
                MergeViewFactory factory = new MergeViewFactory();
                factory.SetViewParameters(_viewFactoryContext, TestViewSupport.ToExprListMD(parameters));
                factory.Attach(SupportEventTypeFactory.CreateBeanType(typeof(SupportMarketDataBean)), SupportStatementContextFactory.MakeContext(_container), null, _parents);
                Assert.Fail();
            }
            catch (ViewParameterException)
            {
                // expected
            }
        }
    
        private void TryParameter(Object[] parameters, String[] fieldNames)
        {
            MergeViewFactory factory = new MergeViewFactory();
            factory.SetViewParameters(_viewFactoryContext, TestViewSupport.ToExprListMD(parameters));
            factory.Attach(SupportEventTypeFactory.CreateBeanType(typeof(SupportMarketDataBean)), SupportStatementContextFactory.MakeContext(_container), null, _parents);
            MergeView view = (MergeView) factory.MakeView(SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext(_container));
            Assert.AreEqual(fieldNames[0], view.GroupFieldNames[0].ToExpressionStringMinPrecedenceSafe());
            if (fieldNames.Length > 0)
            {
                Assert.AreEqual(fieldNames[1], view.GroupFieldNames[1].ToExpressionStringMinPrecedenceSafe());
            }
        }
    }
}
