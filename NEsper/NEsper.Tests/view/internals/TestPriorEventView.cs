///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

using com.espertech.esper.client;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.events;
using com.espertech.esper.support.view;

using NUnit.Framework;

namespace com.espertech.esper.view.internals
{
    [TestFixture]
    public class TestPriorEventView 
    {
        private PriorEventBufferSingle buffer;
        private PriorEventView view;
        private SupportBeanClassView childView;
    
        [SetUp]
        public void SetUp()
        {
            buffer = new PriorEventBufferSingle(1);
            view = new PriorEventView(buffer);
            childView = new SupportBeanClassView(typeof(SupportMarketDataBean));
            view.AddView(childView);
        }
    
        [Test]
        public void TestUpdate()
        {
            // Send some data
            EventBean[] newEventsOne = MakeBeans("a", 2);
            view.Update(newEventsOne, null);
    
            // make sure received
            Assert.AreSame(newEventsOne, childView.LastNewData);
            Assert.IsNull(childView.LastOldData);
            childView.Reset();
    
            // Assert random access
            Assert.AreSame(newEventsOne[0], buffer.GetRelativeToEvent(newEventsOne[1], 0));
    
            EventBean[] newEventsTwo = MakeBeans("b", 3);
            view.Update(newEventsTwo, null);
    
            Assert.AreSame(newEventsTwo[1], buffer.GetRelativeToEvent(newEventsTwo[2], 0));
            Assert.AreSame(newEventsTwo[0], buffer.GetRelativeToEvent(newEventsTwo[1], 0));
            Assert.AreSame(newEventsOne[1], buffer.GetRelativeToEvent(newEventsTwo[0], 0));
        }
    
        private EventBean[] MakeBeans(String id, int numTrades)
        {
            EventBean[] trades = new EventBean[numTrades];
            for (int i = 0; i < numTrades; i++)
            {
                SupportBean_A bean = new SupportBean_A(id + i);
                trades[i] = SupportEventBeanFactory.CreateObject(bean);
            }
            return trades;
        }
    }
}
