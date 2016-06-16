///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.core.support;
using com.espertech.esper.support.view;

using NUnit.Framework;


namespace com.espertech.esper.view.std
{
    [TestFixture]
    public class TestLastElementViewFactory 
    {
        private LastElementViewFactory _factory;
    
        [SetUp]
        public void SetUp()
        {
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
            Assert.IsFalse(_factory.CanReuse(new FirstElementView(null)));
            Assert.IsTrue(_factory.CanReuse(new LastElementView(null)));
        }
    
        private void TryInvalidParameter(Object param)
        {
            try
            {
                LastElementViewFactory factory = new LastElementViewFactory();
                factory.SetViewParameters(SupportStatementContextFactory.MakeViewContext(), TestViewSupport.ToExprListBean(new Object[] {param}));
                Assert.Fail();
            }
            catch (ViewParameterException ex)
            {
                // expected
            }
        }
    
        private void TryParameter(Object[] param)
        {
            LastElementViewFactory factory = new LastElementViewFactory();
            factory.SetViewParameters(SupportStatementContextFactory.MakeViewContext(), TestViewSupport.ToExprListBean(param));
            Assert.IsTrue(factory.MakeView(SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext()) is LastElementView);
        }
    }
}
