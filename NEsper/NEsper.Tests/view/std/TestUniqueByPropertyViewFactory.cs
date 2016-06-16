///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

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
    public class TestUniqueByPropertyViewFactory 
    {
        private UniqueByPropertyViewFactory _factory;
    
        [SetUp]
        public void SetUp()
        {
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
            _factory.SetViewParameters(null, TestViewSupport.ToExprListBean(new Object[] {"IntPrimitive"}));
            _factory.Attach(SupportEventTypeFactory.CreateBeanType(typeof(SupportBean)), SupportStatementContextFactory.MakeContext(), null, null);
            Assert.IsFalse(_factory.CanReuse(new FirstElementView(null)));
        }
    
        private void TryInvalidParameter(Object param)
        {
            try
            {
                UniqueByPropertyViewFactory factory = new UniqueByPropertyViewFactory();
                factory.SetViewParameters(null, TestViewSupport.ToExprListBean(new Object[] {param}));
                factory.Attach(SupportEventTypeFactory.CreateBeanType(typeof(SupportBean)), SupportStatementContextFactory.MakeContext(), null, null);
                Assert.Fail();
            }
            catch (ViewParameterException ex)
            {
                // expected
            }
        }
    
        private void TryParameter(Object param, String fieldName)
        {
            UniqueByPropertyViewFactory factory = new UniqueByPropertyViewFactory();
            factory.SetViewParameters(null, TestViewSupport.ToExprListBean(new Object[] {param}));
            factory.Attach(SupportEventTypeFactory.CreateBeanType(typeof(SupportBean)), SupportStatementContextFactory.MakeContext(), null, null);
            UniqueByPropertyView view = (UniqueByPropertyView) factory.MakeView(SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext());
            Assert.AreEqual(fieldName, view.CriteriaExpressions[0].ToExpressionStringMinPrecedenceSafe());
        }
    }
}
