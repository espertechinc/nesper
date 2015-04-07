///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
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
    public class TestGroupByViewFactory
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            _factory = new GroupByViewFactory();
        }

        #endregion

        private GroupByViewFactory _factory;

        private readonly ViewFactoryContext _viewFactoryContext = new ViewFactoryContext(
            SupportStatementContextFactory.MakeContext(), 1, 0, null, null);

        private void TryInvalidParameter(Object[] parameters)
        {
            try
            {
                var factory = new GroupByViewFactory();

                factory.SetViewParameters(_viewFactoryContext, TestViewSupport.ToExprListBean(parameters));
                factory.Attach(
                    SupportEventTypeFactory.CreateBeanType(typeof (SupportBean)),
                    SupportStatementContextFactory.MakeContext(), null, null);
                Assert.Fail();
            }
            catch (ViewParameterException ex)
            {
                // expected
            }
        }

        private void TryParameter(Object[] parameters, String[] fieldNames)
        {
            var factory = new GroupByViewFactory();

            factory.SetViewParameters(_viewFactoryContext, TestViewSupport.ToExprListBean(parameters));
            factory.Attach(SupportEventTypeFactory.CreateBeanType(typeof (SupportBean)),
                           SupportStatementContextFactory.MakeContext(), null, null);
            var view = (GroupByView) factory.MakeView(
                SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext());

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
            _factory.Attach(parentType, SupportStatementContextFactory.MakeContext(), null, null);
        }

        [Test]
        public void TestCanReuse()
        {
            _factory.SetViewParameters(_viewFactoryContext, TestViewSupport.ToExprListBean(new Object[] { "TheString", "LongPrimitive" }));
            _factory.Attach(SupportEventTypeFactory.CreateBeanType(typeof (SupportBean)),
                           SupportStatementContextFactory.MakeContext(), null, null);
            Assert.IsFalse(_factory.CanReuse(new FirstElementView(null)));
            Assert.IsFalse(
                _factory.CanReuse(
                    new GroupByViewImpl(
                        SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext(),
                        SupportExprNodeFactory.MakeIdentNodesBean(
                            "TheString"),
                        null)));
            Assert.IsTrue(
                _factory.CanReuse(
                    new GroupByViewImpl(
                        SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext(),
                        SupportExprNodeFactory.MakeIdentNodesBean(
                            "TheString", "LongPrimitive"),
                        null)));

            _factory.SetViewParameters(
                _viewFactoryContext,
                TestViewSupport.ToExprListBean(
                    new Object[] { SupportExprNodeFactory.MakeIdentNodesBean("TheString", "LongPrimitive") }));
            Assert.IsFalse(
                _factory.CanReuse(
                    new GroupByViewImpl(
                        SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext(),
                        SupportExprNodeFactory.MakeIdentNodesBean(
                            "TheString"),
                        null)));
            Assert.IsTrue(
                _factory.CanReuse(
                    new GroupByViewImpl(
                        SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext(),
                        SupportExprNodeFactory.MakeIdentNodesBean(
                            "TheString", "LongPrimitive"),
                        null)));
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