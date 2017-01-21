///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.core.support;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.epl;
using com.espertech.esper.support.events;
using com.espertech.esper.support.view;

using NUnit.Framework;

namespace com.espertech.esper.view.std
{
    [TestFixture]
    public class TestMergeViewFactory 
    {
        private MergeViewFactory _factory;
        private IList<ViewFactory> _parents;
        private readonly ViewFactoryContext _viewFactoryContext = new ViewFactoryContext(SupportStatementContextFactory.MakeContext(), 1, null, null, false, -1, false);
    
        [SetUp]
        public void SetUp()
        {
            _factory = new MergeViewFactory();
    
            _parents = new List<ViewFactory>();
            GroupByViewFactory groupByView = new GroupByViewFactory();
            groupByView.SetViewParameters(_viewFactoryContext, TestViewSupport.ToExprListMD(new Object[] { "Symbol", "Feed" }));
            groupByView.Attach(SupportEventTypeFactory.CreateBeanType(typeof(SupportMarketDataBean)), SupportStatementContextFactory.MakeContext(), null, null);
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
            _factory.SetViewParameters(_viewFactoryContext, TestViewSupport.ToExprListMD(new Object[] { "Symbol", "Feed" }));
            _factory.Attach(SupportEventTypeFactory.CreateBeanType(typeof(SupportMarketDataBean)), SupportStatementContextFactory.MakeContext(), null, _parents);
            Assert.IsFalse(_factory.CanReuse(new FirstElementView(null)));
            Assert.IsFalse(_factory.CanReuse(new MergeView(SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext(), SupportExprNodeFactory.MakeIdentNodesMD("Symbol"), null, true)));
            Assert.IsTrue(_factory.CanReuse(new MergeView(SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext(), SupportExprNodeFactory.MakeIdentNodesMD("Symbol", "Feed"), null, true)));
        }
    
        private void TryInvalidParameter(Object[] parameters)
        {
            try
            {
                MergeViewFactory factory = new MergeViewFactory();
                factory.SetViewParameters(_viewFactoryContext, TestViewSupport.ToExprListMD(parameters));
                factory.Attach(SupportEventTypeFactory.CreateBeanType(typeof(SupportMarketDataBean)), SupportStatementContextFactory.MakeContext(), null, _parents);
                Assert.Fail();
            }
            catch (ViewParameterException ex)
            {
                // expected
            }
        }
    
        private void TryParameter(Object[] parameters, String[] fieldNames)
        {
            MergeViewFactory factory = new MergeViewFactory();
            factory.SetViewParameters(_viewFactoryContext, TestViewSupport.ToExprListMD(parameters));
            factory.Attach(SupportEventTypeFactory.CreateBeanType(typeof(SupportMarketDataBean)), SupportStatementContextFactory.MakeContext(), null, _parents);
            MergeView view = (MergeView) factory.MakeView(SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext());
            Assert.AreEqual(fieldNames[0], view.GroupFieldNames[0].ToExpressionStringMinPrecedenceSafe());
            if (fieldNames.Length > 0)
            {
                Assert.AreEqual(fieldNames[1], view.GroupFieldNames[1].ToExpressionStringMinPrecedenceSafe());
            }
        }
    }
}
