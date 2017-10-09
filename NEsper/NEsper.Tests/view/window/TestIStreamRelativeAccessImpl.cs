///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using com.espertech.esper.client;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.events;

using NUnit.Framework;

namespace com.espertech.esper.view.window
{
    [TestFixture]
    public class TestIStreamRelativeAccessImpl 
    {
        private IStreamRelativeAccess _access;
        private EventBean[] _events;
    
        [SetUp]
        public void SetUp()
        {
            _access = new IStreamRelativeAccess(new ProxyIStreamRelativeAccessUpdateObserver
            {
                ProcUpdated = (access, newData) => { }
            });
    
            _events = new EventBean[100];
            for (int i = 0; i < _events.Length; i++)
            {
                _events[i] = SupportEventBeanFactory.CreateObject(new SupportBean());
            }
        }
    
        [Test]
        public void TestGet()
        {
            _access.Update(new EventBean[] {_events[0]}, null);
            Assert.AreEqual(_events[0], _access.GetRelativeToEvent(_events[0], 0));
            Assert.IsNull(_access.GetRelativeToEvent(_events[0], 1));
    
            // sends the newest event last (i.e. 1 older 2 as 1 is sent first)
            _access.Update(new EventBean[] {_events[1], _events[2]}, null);
            Assert.AreEqual(_events[1], _access.GetRelativeToEvent(_events[1], 0));
            Assert.IsNull(_access.GetRelativeToEvent(_events[1], 1));
            Assert.AreEqual(_events[2], _access.GetRelativeToEvent(_events[2], 0));
            Assert.AreEqual(_events[1], _access.GetRelativeToEvent(_events[2], 1));
            Assert.IsNull(_access.GetRelativeToEvent(_events[2], 2));
    
            // sends the newest event last (i.e. 1 older 2 as 1 is sent first)
            _access.Update(new EventBean[] {_events[3], _events[4], _events[5]}, null);
            Assert.AreEqual(_events[3], _access.GetRelativeToEvent(_events[3], 0));
            Assert.IsNull(_access.GetRelativeToEvent(_events[3], 1));
            Assert.AreEqual(_events[4], _access.GetRelativeToEvent(_events[4], 0));
            Assert.AreEqual(_events[3], _access.GetRelativeToEvent(_events[4], 1));
            Assert.IsNull(_access.GetRelativeToEvent(_events[4], 2));
            Assert.AreEqual(_events[5], _access.GetRelativeToEvent(_events[5], 0));
            Assert.AreEqual(_events[4], _access.GetRelativeToEvent(_events[5], 1));
            Assert.AreEqual(_events[3], _access.GetRelativeToEvent(_events[5], 2));
            Assert.IsNull(_access.GetRelativeToEvent(_events[5], 3));
        }
    }
}
