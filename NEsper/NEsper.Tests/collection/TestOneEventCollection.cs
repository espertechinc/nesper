///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.supportunit.events;

using NUnit.Framework;

namespace com.espertech.esper.collection
{
    [TestFixture]
    public class TestOneEventCollection 
    {
        private OneEventCollection _list;
        private EventBean[] _events;
    
        [SetUp]
        public void SetUp()
        {
            _list = new OneEventCollection();
            _events = SupportEventBeanFactory.MakeEvents(new [] {"1", "2", "3", "4"});
        }
    
        [Test]
        public void TestFlow()
        {
            Assert.IsTrue(_list.IsEmpty());
            EPAssertionUtil.AssertEqualsExactOrder(new EventBean[0], _list.ToArray());
    
            _list.Add(_events[0]);
            Assert.IsFalse(_list.IsEmpty());
            EPAssertionUtil.AssertEqualsExactOrder(new[] { _events[0] }, _list.ToArray());
    
            _list.Add(_events[1]);
            Assert.IsFalse(_list.IsEmpty());
            EPAssertionUtil.AssertEqualsExactOrder(new[] { _events[0], _events[1] }, _list.ToArray());
    
            _list.Add(_events[2]);
            Assert.IsFalse(_list.IsEmpty());
            EPAssertionUtil.AssertEqualsExactOrder(new[] { _events[0], _events[1], _events[2] }, _list.ToArray());
        }
    }
}
