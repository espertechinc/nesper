///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.events;
using com.espertech.esper.support.view;

using NUnit.Framework;

namespace com.espertech.esper.view
{
    [TestFixture]
    public class TestZeroDepthStream 
    {
        private ZeroDepthStreamIterable _stream;
        private SupportSchemaNeutralView _testChildView;
        private EventType _eventType;
    
        private EventBean _eventBean;
    
        [SetUp]
        public void SetUp()
        {
            _eventType = SupportEventTypeFactory.CreateBeanType(typeof(SupportBean_A));
    
            _stream = new ZeroDepthStreamIterable(_eventType);
    
            _testChildView = new SupportSchemaNeutralView();
            _stream.AddView(_testChildView);
            _testChildView.Parent = _stream;
    
            _eventBean = SupportEventBeanFactory.CreateObject(new SupportBean_A("a1"));
        }
    
        [Test]
        public void TestInsert()
        {
            _testChildView.ClearLastNewData();
            _stream.Insert(_eventBean);
    
            Assert.IsTrue(_testChildView.LastNewData != null);
            Assert.AreEqual(1, _testChildView.LastNewData.Length);
            Assert.AreEqual(_eventBean, _testChildView.LastNewData[0]);
    
            // Remove view
            _testChildView.ClearLastNewData();
            _stream.RemoveView(_testChildView);
            _stream.Insert(_eventBean);
            Assert.IsTrue(_testChildView.LastNewData == null);
        }
    }
}
