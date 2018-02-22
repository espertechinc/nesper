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
using com.espertech.esper.core.service;
using com.espertech.esper.core.support;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.epl;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.supportunit.util;
using com.espertech.esper.view.std;

using NUnit.Framework;

namespace com.espertech.esper.view.ext
{
    [TestFixture]
    public class TestSortWindowViewFactory
    {
        private IContainer _container;
        private SortWindowViewFactory _factory;
    
        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
            _factory = new SortWindowViewFactory();
        }
    
        [Test]
        public void TestSetParameters()
        {
            TryParameter(
                new Object[] {100, "Price", "Volume"},
                new String[] {"Price", "Volume"}, 100);

            TryInvalidParameter(new Object[] { "Price", "Symbol", "Volume" });
            TryInvalidParameter(new Object[] {});
            TryInvalidParameter(new Object[] {100, "Price", 100});
            TryInvalidParameter(new Object[] {100, 100});
            TryInvalidParameter(new Object[] {100, "Price", true});
        }
    
        [Test]
        public void TestAttaches()
        {
            // Should attach to anything as long as the fields exists
            EventType parentType = SupportEventTypeFactory.CreateBeanType(typeof(SupportMarketDataBean));
    
            _factory.SetViewParameters(null, TestViewSupport.ToExprListMD(new Object[] {100, "Price"}));
            _factory.Attach(parentType, SupportStatementContextFactory.MakeContext(_container), null, null);
    
            try
            {
                _factory.SetViewParameters(null, TestViewSupport.ToExprListMD(new Object[] {true, "Price"}));
                _factory.Attach(parentType, SupportStatementContextFactory.MakeContext(_container), null, null);
                Assert.Fail();
            }
            catch (ViewParameterException)
            {
                // expected;
            }
        }
    
        [Test]
        public void TestCanReuse()
        {
            StatementContext context = SupportStatementContextFactory.MakeContext(_container);

            AgentInstanceContext agentInstanceContext = SupportStatementContextFactory.MakeAgentInstanceContext(_container);
            _factory.SetViewParameters(null, TestViewSupport.ToExprListMD(new Object[] { 100, "Price" }));
            _factory.Attach(SupportEventTypeFactory.CreateBeanType(typeof(SupportMarketDataBean)), SupportStatementContextFactory.MakeContext(_container), null, null);
            Assert.IsFalse(_factory.CanReuse(new FirstElementView(null), agentInstanceContext));
            Assert.IsTrue(_factory.CanReuse(new SortWindowView(_factory, SupportExprNodeFactory.MakeIdentNodesMD("Price"), new ExprEvaluator[0], new bool[] { false }, 100, null, false, null), agentInstanceContext));
            Assert.IsFalse(_factory.CanReuse(new SortWindowView(_factory, SupportExprNodeFactory.MakeIdentNodesMD("Volume"), new ExprEvaluator[0], new bool[] { true }, 100, null, false, null), agentInstanceContext));
            Assert.IsFalse(_factory.CanReuse(new SortWindowView(_factory, SupportExprNodeFactory.MakeIdentNodesMD("Price"), new ExprEvaluator[0], new bool[] { false }, 99, null, false, null), agentInstanceContext));
            Assert.IsFalse(_factory.CanReuse(new SortWindowView(_factory, SupportExprNodeFactory.MakeIdentNodesMD("Symbol"), new ExprEvaluator[0], new bool[] { false }, 100, null, false, null), agentInstanceContext));

            _factory.SetViewParameters(null, TestViewSupport.ToExprListMD(new Object[]{100, "Price", "Volume" }));
            _factory.Attach(SupportEventTypeFactory.CreateBeanType(typeof(SupportMarketDataBean)), SupportStatementContextFactory.MakeContext(_container), null, null);
            Assert.IsTrue(_factory.CanReuse(new SortWindowView(_factory, SupportExprNodeFactory.MakeIdentNodesMD("Price", "Volume"), new ExprEvaluator[0], new bool[] { false, false }, 100, null, false, null), agentInstanceContext));
            Assert.IsFalse(_factory.CanReuse(new SortWindowView(_factory, SupportExprNodeFactory.MakeIdentNodesMD("Price", "Symbol"), new ExprEvaluator[0], new bool[] { true, false }, 100, null, false, null), agentInstanceContext));
        }
    
        private void TryInvalidParameter(Object[] paramList)
        {
            try
            {
                _factory.SetViewParameters(null, TestViewSupport.ToExprListMD(paramList));
                _factory.Attach(SupportEventTypeFactory.CreateBeanType(typeof(SupportMarketDataBean)), SupportStatementContextFactory.MakeContext(_container), null, null);
                Assert.Fail();
            }
            catch (ViewParameterException)
            {
                // expected
            }
        }
    
        private void TryParameter(Object[] paramList, String[] fieldNames, int size)
        {
            _factory.SetViewParameters(null, TestViewSupport.ToExprListMD(paramList));
            _factory.Attach(SupportEventTypeFactory.CreateBeanType(typeof(SupportMarketDataBean)), SupportStatementContextFactory.MakeContext(_container), null, null);
            SortWindowView view = (SortWindowView) _factory.MakeView(SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext(_container));
            Assert.AreEqual(size, view.SortWindowSize);
            Assert.AreEqual(fieldNames[0], view.SortCriteriaExpressions[0].ToExpressionStringMinPrecedenceSafe());
            if (fieldNames.Length > 0)
            {
                Assert.AreEqual(fieldNames[1], view.SortCriteriaExpressions[1].ToExpressionStringMinPrecedenceSafe());
            }
        }
    }
}
