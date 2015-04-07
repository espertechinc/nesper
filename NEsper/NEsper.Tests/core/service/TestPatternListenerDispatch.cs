///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.support.events;

using NUnit.Framework;

namespace com.espertech.esper.core.service
{
    [TestFixture]
    public class TestPatternListenerDispatch 
    {
        private PatternListenerDispatch _dispatch;
    
        private readonly EventBean _eventOne = SupportEventBeanFactory.CreateObject("a");
        private readonly EventBean _eventTwo = SupportEventBeanFactory.CreateObject("b");
    
        private readonly SupportUpdateListener _listener = new SupportUpdateListener();
    
        [SetUp]
        public void SetUp()
        {
            ISet<UpdateEventHandler> listeners = new HashSet<UpdateEventHandler>();
            listeners.Add(_listener.Update);
            _dispatch = new PatternListenerDispatch(null, null, listeners);
        }
    
        [Test]
        public void TestSingle()
        {
            _listener.Reset();
    
            Assert.IsFalse(_dispatch.HasData);
            _dispatch.Add(_eventOne);
            Assert.IsTrue(_dispatch.HasData);
    
            _dispatch.Execute();
    
            Assert.IsFalse(_dispatch.HasData);
            Assert.AreEqual(1, _listener.LastNewData.Length);
            Assert.AreEqual(_eventOne, _listener.LastNewData[0]);
        }
    
        [Test]
        public void TestTwo()
        {
            _listener.Reset();
            Assert.IsFalse(_dispatch.HasData);
    
            _dispatch.Add(_eventOne);
            _dispatch.Add(_eventTwo);
            Assert.IsTrue(_dispatch.HasData);
    
            _dispatch.Execute();
    
            Assert.IsFalse(_dispatch.HasData);
            Assert.AreEqual(2, _listener.LastNewData.Length);
            Assert.AreEqual(_eventOne, _listener.LastNewData[0]);
            Assert.AreEqual(_eventTwo, _listener.LastNewData[1]);
        }
    }
}
