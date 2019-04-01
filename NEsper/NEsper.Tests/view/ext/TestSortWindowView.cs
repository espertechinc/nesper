///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.epl;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.supportunit.view;

using NUnit.Framework;

namespace com.espertech.esper.view.ext
{
    [TestFixture]
    public class TestSortWindowView
    {
        private SortWindowView _myView;
        private SupportBeanClassView _childView;
    
        [SetUp]
        public void SetUp() {
            // Set up length window view and a test child view
            ExprNode[] expressions = SupportExprNodeFactory.MakeIdentNodesMD("Volume");
            _myView = new SortWindowView(new SortWindowViewFactory(), expressions, ExprNodeUtility.GetEvaluators(expressions), new bool[] { false }, 5, null, false, null);
            _childView = new SupportBeanClassView(typeof(SupportMarketDataBean));
            _myView.AddView(_childView);
        }
    
        [Test]
        public void TestViewOneProperty() {
            // Set up a feed for the view under test - the depth is 10 events so bean[10] will cause bean[0] to go old
            SupportStreamImpl stream = new SupportStreamImpl(typeof(SupportMarketDataBean), 10);
            stream.AddView(_myView);
    
            EventBean[] bean = new EventBean[12];
    
            bean[0] = MakeBean(1000);
            stream.Insert(bean[0]);
            SupportViewDataChecker.CheckOldData(_childView, null);
            SupportViewDataChecker.CheckNewData(_childView, new EventBean[]{bean[0]});
            EPAssertionUtil.AssertEqualsExactOrder(new EventBean[]{bean[0]}, _myView.GetEnumerator());
    
            bean[1] = MakeBean(800);
            bean[2] = MakeBean(1200);
            stream.Insert(new EventBean[]{bean[1], bean[2]});
            SupportViewDataChecker.CheckOldData(_childView, null);
            SupportViewDataChecker.CheckNewData(_childView, new EventBean[]{bean[1], bean[2]});
            EPAssertionUtil.AssertEqualsExactOrder(new EventBean[]{bean[1], bean[0], bean[2]}, _myView.GetEnumerator());
    
            bean[3] = MakeBean(1200);
            bean[4] = MakeBean(1000);
            bean[5] = MakeBean(1400);
            bean[6] = MakeBean(1100);
            stream.Insert(new EventBean[]{bean[3], bean[4], bean[5], bean[6]});
            SupportViewDataChecker.CheckOldData(_childView, new EventBean[]{bean[5], bean[2]});
            SupportViewDataChecker.CheckNewData(_childView, new EventBean[]{bean[3], bean[4], bean[5], bean[6]});
            EPAssertionUtil.AssertEqualsExactOrder(new EventBean[]{bean[1], bean[4], bean[0], bean[6], bean[3]}, _myView.GetEnumerator());
    
            bean[7] = MakeBean(800);
            bean[8] = MakeBean(700);
            bean[9] = MakeBean(1200);
            stream.Insert(new EventBean[]{bean[7], bean[8], bean[9]});
            SupportViewDataChecker.CheckOldData(_childView, new EventBean[]{bean[3], bean[9], bean[6]});
            SupportViewDataChecker.CheckNewData(_childView, new EventBean[]{bean[7], bean[8], bean[9]});
            EPAssertionUtil.AssertEqualsExactOrder(new EventBean[]{bean[8], bean[7], bean[1], bean[4], bean[0]}, _myView.GetEnumerator());
    
            bean[10] = MakeBean(1050);
            stream.Insert(new EventBean[]{bean[10]});       // Thus bean[0] will be old data !
            SupportViewDataChecker.CheckOldData(_childView, new EventBean[]{bean[0]});
            SupportViewDataChecker.CheckNewData(_childView, new EventBean[]{bean[10]});
            EPAssertionUtil.AssertEqualsExactOrder(new EventBean[]{bean[8], bean[7], bean[1], bean[4], bean[10]}, _myView.GetEnumerator());
    
            bean[11] = MakeBean(2000);
            stream.Insert(new EventBean[]{bean[11]});       // Thus bean[1] will be old data !
            SupportViewDataChecker.CheckOldData(_childView, new EventBean[]{bean[1]});
            SupportViewDataChecker.CheckNewData(_childView, new EventBean[]{bean[11]});
            EPAssertionUtil.AssertEqualsExactOrder(new EventBean[]{bean[8], bean[7], bean[4], bean[10], bean[11]}, _myView.GetEnumerator());
        }
    
        [Test]
        public void TestViewTwoProperties()
        {
            // Set up a sort windows that sorts on two properties
            ExprNode[] expressions = SupportExprNodeFactory.MakeIdentNodesMD("Volume", "Price");
            _myView = new SortWindowView(new SortWindowViewFactory(), expressions, ExprNodeUtility.GetEvaluators(expressions), new bool[] { false, true }, 5, null, false, null);
            _childView = new SupportBeanClassView(typeof(SupportMarketDataBean));
            _myView.AddView(_childView);
    
            // Set up a feed for the view under test - the depth is 10 events so bean[10] will cause bean[0] to go old
            SupportStreamImpl stream = new SupportStreamImpl(typeof(SupportMarketDataBean), 10);
            stream.AddView(_myView);
    
            EventBean[] bean = new EventBean[12];
    
            bean[0] = MakeBean(20d, 1000);
            stream.Insert(bean[0]);
            SupportViewDataChecker.CheckOldData(_childView, null);
            SupportViewDataChecker.CheckNewData(_childView, new EventBean[]{bean[0]});
            EPAssertionUtil.AssertEqualsExactOrder(new EventBean[]{bean[0]}, _myView.GetEnumerator());
    
            bean[1] = MakeBean(19d, 800);
            bean[2] = MakeBean(18d, 1200);
            stream.Insert(new EventBean[]{bean[1], bean[2]});
            SupportViewDataChecker.CheckOldData(_childView, null);
            SupportViewDataChecker.CheckNewData(_childView, new EventBean[]{bean[1], bean[2]});
            EPAssertionUtil.AssertEqualsExactOrder(new EventBean[]{bean[1], bean[0], bean[2]}, _myView.GetEnumerator());
    
            bean[3] = MakeBean(17d, 1200);
            bean[4] = MakeBean(16d, 1000);
            bean[5] = MakeBean(15d, 1400);
            bean[6] = MakeBean(14d, 1100);
            stream.Insert(new EventBean[]{bean[3], bean[4], bean[5], bean[6]});
            SupportViewDataChecker.CheckOldData(_childView, new EventBean[]{bean[5], bean[3]});
            SupportViewDataChecker.CheckNewData(_childView, new EventBean[]{bean[3], bean[4], bean[5], bean[6]});
            EPAssertionUtil.AssertEqualsExactOrder(new EventBean[]{bean[1], bean[0], bean[4], bean[6], bean[2]}, _myView.GetEnumerator());
    
            bean[7] = MakeBean(13d, 800);
            bean[8] = MakeBean(12d, 700);
            bean[9] = MakeBean(11d, 1200);
            stream.Insert(new EventBean[]{bean[7], bean[8], bean[9]});
            SupportViewDataChecker.CheckOldData(_childView, new EventBean[]{bean[9], bean[2], bean[6]});
            SupportViewDataChecker.CheckNewData(_childView, new EventBean[]{bean[7], bean[8], bean[9]});
            EPAssertionUtil.AssertEqualsExactOrder(new EventBean[]{bean[8], bean[1], bean[7], bean[0], bean[4]}, _myView.GetEnumerator());
    
            bean[10] = MakeBean(10d, 1050);
            stream.Insert(new EventBean[]{bean[10]});       // Thus bean[0] will be old data !
            SupportViewDataChecker.CheckOldData(_childView, new EventBean[]{bean[0]});
            SupportViewDataChecker.CheckNewData(_childView, new EventBean[]{bean[10]});
            EPAssertionUtil.AssertEqualsExactOrder(new EventBean[]{bean[8], bean[1], bean[7], bean[4], bean[10]}, _myView.GetEnumerator());
    
            bean[11] = MakeBean(2000);
            stream.Insert(new EventBean[]{bean[11]});       // Thus bean[1] will be old data !
            SupportViewDataChecker.CheckOldData(_childView, new EventBean[]{bean[1]});
            SupportViewDataChecker.CheckNewData(_childView, new EventBean[]{bean[11]});
            EPAssertionUtil.AssertEqualsExactOrder(new EventBean[]{bean[8], bean[7], bean[4], bean[10], bean[11]}, _myView.GetEnumerator());
        }
    
        private EventBean MakeBean(long volume) {
            SupportMarketDataBean bean = new SupportMarketDataBean("CSCO.O", 0, volume, "");
            return SupportEventBeanFactory.CreateObject(bean);
        }
    
        private EventBean MakeBean(double price, long volume) {
            SupportMarketDataBean bean = new SupportMarketDataBean("CSCO.O", price, volume, "");
            return SupportEventBeanFactory.CreateObject(bean);
        }
    }
}
