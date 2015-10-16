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

using NUnit.Framework;

namespace com.espertech.esper.view.window
{
    [TestFixture]
    public class TestIStreamRandomAccessImpl 
    {
        private IStreamRandomAccess _access;
        private EventBean[] _events;
    
        [SetUp]
        public void SetUp()
        {
            _access = new IStreamRandomAccess(
                new ProxyRandomAccessByIndexObserver { UpdatedFunc = randomAccessByIndex => { } });

            _events = new EventBean[100];
            for (int i = 0; i < _events.Length; i++)
            {
                _events[i] = SupportEventBeanFactory.CreateObject(new SupportBean());
            }
        }
    
        [Test]
        public void TestFlow()
        {
            Assert.IsNull(_access.GetNewData(0));
            Assert.IsNull(_access.GetOldData(0));
    
            _access.Update(new[] {_events[0]}, null);
            Assert.AreEqual(_events[0], _access.GetNewData(0));
            Assert.IsNull(_access.GetNewData(1));
            Assert.IsNull(_access.GetOldData(0));
    
            _access.Update(new[] {_events[1], _events[2]}, null);
            Assert.AreEqual(_events[2], _access.GetNewData(0));
            Assert.AreEqual(_events[1], _access.GetNewData(1));
            Assert.AreEqual(_events[0], _access.GetNewData(2));
            Assert.IsNull(_access.GetNewData(3));
            Assert.IsNull(_access.GetOldData(0));
    
            _access.Update(new[] {_events[3]}, new[] {_events[0]});
            Assert.AreEqual(_events[3], _access.GetNewData(0));
            Assert.AreEqual(_events[2], _access.GetNewData(1));
            Assert.AreEqual(_events[1], _access.GetNewData(2));
            Assert.IsNull(_access.GetNewData(3));
            Assert.IsNull(_access.GetOldData(0));
    
            _access.Update(null, new[] {_events[1], _events[2]});
            Assert.AreEqual(_events[3], _access.GetNewData(0));
            Assert.IsNull(_access.GetNewData(1));
            Assert.IsNull(_access.GetOldData(0));
    
            _access.Update(null, new[] {_events[3]});
            Assert.IsNull(_access.GetNewData(0));
            Assert.IsNull(_access.GetOldData(0));
        }
    }
}
