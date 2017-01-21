///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.core.support;
using com.espertech.esper.support.view;
using com.espertech.esper.view.std;

using NUnit.Framework;

namespace com.espertech.esper.view.window
{
    [TestFixture]
    public class TestLengthBatchViewFactory 
    {
        private LengthBatchViewFactory _factory;
    
        [SetUp]
        public void SetUp()
        {
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
            _factory.SetViewParameters(SupportStatementContextFactory.MakeViewContext(), TestViewSupport.ToExprListBean(new Object[] {1000}));
            Assert.IsFalse(_factory.CanReuse(new FirstElementView(null)));
            Assert.IsFalse(_factory.CanReuse(new LengthBatchView(null, _factory, 1, null)));
            Assert.IsTrue(_factory.CanReuse(new LengthBatchView(null, _factory, 1000, null)));
        }
    
        private void TryInvalidParameter(Object param)
        {
            try
            {
                _factory.SetViewParameters(SupportStatementContextFactory.MakeViewContext(), TestViewSupport.ToExprListBean(new Object[] {param}));
                Assert.Fail();
            }
            catch (ViewParameterException ex)
            {
                // expected
            }
        }
    
        private void TryParameter(Object[] param, int size)
        {
            var factory = new LengthBatchViewFactory();
            factory.SetViewParameters(SupportStatementContextFactory.MakeViewContext(), TestViewSupport.ToExprListBean(param));
            var view = (LengthBatchView) factory.MakeView(SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext());
            Assert.AreEqual(size, view.Size);
        }
    }
}
