///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.core.support;
using com.espertech.esper.support.view;

using NUnit.Framework;

namespace com.espertech.esper.view.std
{
    [TestFixture]
    public class TestSizeViewFactory 
    {
        private SizeViewFactory _factory;
    
        [SetUp]
        public void SetUp()
        {
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
            Assert.IsFalse(_factory.CanReuse(new LastElementView(null)));
            EventType type = SizeView.CreateEventType(SupportStatementContextFactory.MakeContext(), null, 1);
            Assert.IsTrue(_factory.CanReuse(new SizeView(SupportStatementContextFactory.MakeAgentInstanceContext(), type, null)));
        }
    
        private void TryParameter(Object[] param)
        {
            SizeViewFactory factory = new SizeViewFactory();
            factory.SetViewParameters(SupportStatementContextFactory.MakeViewContext(), TestViewSupport.ToExprListBean(param));
            Assert.IsTrue(factory.MakeView(SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext()) is SizeView);
        }
    }
}
