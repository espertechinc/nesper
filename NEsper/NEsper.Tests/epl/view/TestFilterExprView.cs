///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.support.epl;
using com.espertech.esper.support.events;
using com.espertech.esper.support.view;

using NUnit.Framework;

namespace com.espertech.esper.epl.view
{
    [TestFixture]
    public class TestFilterExprView 
    {
        private FilterExprView _filterExprViewAdapter;
        private SupportMapView _childView;
    
        [SetUp]
        public void SetUp()
        {
            _filterExprViewAdapter = new FilterExprView(new SupportExprNode(null), new SupportExprEvaluator(), null);
            _childView = new SupportMapView();
            _filterExprViewAdapter.AddView(_childView);
        }
    
        [Test]
        public void TestUpdate()
        {
            // Test all evaluate to true (ie. all pass the filter)
            EventBean[] oldEvents = SupportEventBeanFactory.MakeEvents(new [] {true, true});
            EventBean[] newEvents = SupportEventBeanFactory.MakeEvents(new [] {true, true});
            _filterExprViewAdapter.Update(newEvents, oldEvents);
    
            Assert.AreEqual(newEvents, _childView.LastNewData);
            Assert.AreEqual(oldEvents, _childView.LastOldData);
            _childView.Reset();
    
            // Test all evaluate to false (ie. none pass the filter)
            oldEvents = SupportEventBeanFactory.MakeEvents(new [] {false, false});
            newEvents = SupportEventBeanFactory.MakeEvents(new [] {false, false});
            _filterExprViewAdapter.Update(newEvents, oldEvents);
    
            Assert.IsFalse(_childView.GetAndClearIsInvoked());  // Should not be invoked if no events
            Assert.IsNull(_childView.LastNewData);
            Assert.IsNull(_childView.LastOldData);
    
            // Test some pass through the filter
            oldEvents = SupportEventBeanFactory.MakeEvents(new [] {false, true, false});
            newEvents = SupportEventBeanFactory.MakeEvents(new [] {true, false, true});
            _filterExprViewAdapter.Update(newEvents, oldEvents);

            Assert.NotNull(_childView.LastNewData);
            Assert.NotNull(_childView.LastOldData);

            // ReSharper disable PossibleNullReferenceException
            Assert.AreEqual(2, _childView.LastNewData.Length);
            Assert.AreSame(newEvents[0], _childView.LastNewData[0]);
            Assert.AreSame(newEvents[2], _childView.LastNewData[1]);
            Assert.AreEqual(1, _childView.LastOldData.Length);
            Assert.AreSame(oldEvents[1], _childView.LastOldData[0]);
            // ReSharper restore PossibleNullReferenceException
        }
    }
}
