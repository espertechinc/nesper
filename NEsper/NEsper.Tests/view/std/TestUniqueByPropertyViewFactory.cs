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
    public class TestUniqueByPropertyViewFactory 
    {
        private UniqueByPropertyViewFactory _factory;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
            _factory = new UniqueByPropertyViewFactory();
        }
    
        [Test]
        public void TestSetParameters()
        {
            TryParameter("LongPrimitive", "LongPrimitive");
            TryInvalidParameter(1.1d);
        }
    
        [Test]
        public void TestCanReuse()
        {
           AgentInstanceContext agentInstanceContext = SupportStatementContextFactory.MakeAgentInstanceContext(_container);
            _factory.SetViewParameters(null, TestViewSupport.ToExprListBean(new Object[]{"IntPrimitive"}));
            _factory.Attach(SupportEventTypeFactory.CreateBeanType(typeof(SupportBean)), SupportStatementContextFactory.MakeContext(_container), null, null);
            Assert.IsFalse(_factory.CanReuse(new FirstElementView(null), agentInstanceContext));
        }
    
        private void TryInvalidParameter(Object param)
        {
            try
            {
                UniqueByPropertyViewFactory factory = new UniqueByPropertyViewFactory();
                factory.SetViewParameters(null, TestViewSupport.ToExprListBean(new Object[] {param}));
                factory.Attach(SupportEventTypeFactory.CreateBeanType(typeof(SupportBean)), SupportStatementContextFactory.MakeContext(_container), null, null);
                Assert.Fail();
            }
            catch (ViewParameterException)
            {
                // expected
            }
        }
    
        private void TryParameter(Object param, String fieldName)
        {
            UniqueByPropertyViewFactory factory = new UniqueByPropertyViewFactory();
            factory.SetViewParameters(null, TestViewSupport.ToExprListBean(new Object[] {param}));
            factory.Attach(SupportEventTypeFactory.CreateBeanType(typeof(SupportBean)), SupportStatementContextFactory.MakeContext(_container), null, null);
            UniqueByPropertyView view = (UniqueByPropertyView) factory.MakeView(SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext(_container));
            Assert.AreEqual(fieldName, view.CriteriaExpressions[0].ToExpressionStringMinPrecedenceSafe());
        }
    }
}
