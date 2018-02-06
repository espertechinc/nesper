///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.container;
using com.espertech.esper.core.support;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.epl;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.supportunit.util;
using com.espertech.esper.supportunit.view;

using NUnit.Framework;

namespace com.espertech.esper.view.window
{
    [TestFixture]
    public class TestExternallyTimedWindowView
    {
        private ExternallyTimedWindowView _myView;
        private SupportBeanClassView _childView;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
            // Set up timed window view and a test child view, set the time window size to 1 second
            ExprNode node = SupportExprNodeFactory.MakeIdentNodeBean("LongPrimitive");
            _myView = new ExternallyTimedWindowView(
                new ExternallyTimedWindowViewFactory(), 
                node, 
                node.ExprEvaluator, 
                new ExprTimePeriodEvalDeltaConstGivenDelta(1000), null, 
                SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext(_container));
            _childView = new SupportBeanClassView(typeof(SupportBean));
            _myView.AddView(_childView);
        }
    
        [Test]
        public void TestIncorrectUse() {
            try {
                _myView = new ExternallyTimedWindowView(
                    null, 
                    SupportExprNodeFactory.MakeIdentNodeBean("TheString"), null, 
                    new ExprTimePeriodEvalDeltaConstGivenDelta(0), null, 
                    SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext(_container));
            } catch (ArgumentException) {
                // Expected exception
            }
        }
    
        [Test]
        public void TestViewPush() {
            // Set up a feed for the view under test - it will have a depth of 3 trades
            SupportStreamImpl stream = new SupportStreamImpl(typeof(SupportBean), 3);
            stream.AddView(_myView);
    
            EventBean[] a = MakeBeans("a", 10000, 1);
            stream.Insert(a);
            SupportViewDataChecker.CheckOldData(_childView, null);
            SupportViewDataChecker.CheckNewData(_childView, new EventBean[]{a[0]});
            EPAssertionUtil.AssertEqualsExactOrder(new EventBean[]{a[0]}, _myView.GetEnumerator());
    
            EventBean[] b = MakeBeans("b", 10500, 2);
            stream.Insert(b);
            SupportViewDataChecker.CheckOldData(_childView, null);
            SupportViewDataChecker.CheckNewData(_childView, new EventBean[]{b[0], b[1]});
            EPAssertionUtil.AssertEqualsExactOrder(new EventBean[]{a[0], b[0], b[1]}, _myView.GetEnumerator());
    
            EventBean[] c = MakeBeans("c", 10900, 1);
            stream.Insert(c);
            SupportViewDataChecker.CheckOldData(_childView, null);
            SupportViewDataChecker.CheckNewData(_childView, new EventBean[]{c[0]});
            EPAssertionUtil.AssertEqualsExactOrder(new EventBean[]{a[0], b[0], b[1], c[0]}, _myView.GetEnumerator());
    
            EventBean[] d = MakeBeans("d", 10999, 1);
            stream.Insert(d);
            SupportViewDataChecker.CheckOldData(_childView, null);
            SupportViewDataChecker.CheckNewData(_childView, new EventBean[]{d[0]});
            EPAssertionUtil.AssertEqualsExactOrder(new EventBean[]{a[0], b[0], b[1], c[0], d[0]}, _myView.GetEnumerator());
    
            EventBean[] e = MakeBeans("e", 11000, 2);
            stream.Insert(e);
            SupportViewDataChecker.CheckOldData(_childView, new EventBean[]{a[0]});
            SupportViewDataChecker.CheckNewData(_childView, new EventBean[]{e[0], e[1]});
            EPAssertionUtil.AssertEqualsExactOrder(new EventBean[]{b[0], b[1], c[0], d[0], e[0], e[1]}, _myView.GetEnumerator());
    
            EventBean[] f = MakeBeans("f", 11500, 1);
            stream.Insert(f);
            SupportViewDataChecker.CheckOldData(_childView, new EventBean[]{b[0], b[1]});
            SupportViewDataChecker.CheckNewData(_childView, new EventBean[]{f[0]});
            EPAssertionUtil.AssertEqualsExactOrder(new EventBean[]{c[0], d[0], e[0], e[1], f[0]}, _myView.GetEnumerator());
    
            EventBean[] g = MakeBeans("g", 11899, 1);
            stream.Insert(g);
            SupportViewDataChecker.CheckOldData(_childView, null);
            SupportViewDataChecker.CheckNewData(_childView, new EventBean[]{g[0]});
            EPAssertionUtil.AssertEqualsExactOrder(new EventBean[]{c[0], d[0], e[0], e[1], f[0], g[0]}, _myView.GetEnumerator());
    
            EventBean[] h = MakeBeans("h", 11999, 3);
            stream.Insert(h);
            SupportViewDataChecker.CheckOldData(_childView, new EventBean[]{c[0], d[0]});
            SupportViewDataChecker.CheckNewData(_childView, new EventBean[]{h[0], h[1], h[2]});
            EPAssertionUtil.AssertEqualsExactOrder(new EventBean[]{e[0], e[1], f[0], g[0], h[0], h[1], h[2]}, _myView.GetEnumerator());
    
            EventBean[] i = MakeBeans("i", 13001, 1);
            stream.Insert(i);
            SupportViewDataChecker.CheckOldData(_childView, new EventBean[]{e[0], e[1], f[0], g[0], h[0], h[1], h[2]});
            SupportViewDataChecker.CheckNewData(_childView, new EventBean[]{i[0]});
            EPAssertionUtil.AssertEqualsExactOrder(new EventBean[]{i[0]}, _myView.GetEnumerator());
        }
    
        private EventBean[] MakeBeans(String id, long timestamp, int numBeans) {
            EventBean[] beans = new EventBean[numBeans];
            for (int i = 0; i < numBeans; i++) {
                SupportBean bean = new SupportBean();
                bean.LongPrimitive = timestamp;
                bean.TheString = (id + 1);
                beans[i] = SupportEventBeanFactory.CreateObject(bean);
            }
            return beans;
        }
    }
}
