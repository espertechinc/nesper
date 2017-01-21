///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.pattern;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.events;

using NUnit.Framework;


namespace com.espertech.esper.filter
{
    [TestFixture]
    public class TestRangeValueEventProp 
    {
        private FilterSpecParamRangeValue[] @params = new FilterSpecParamRangeValue[5];
    
        [SetUp]
        public void SetUp()
        {
            @params[0] = new RangeValueEventProp("a", "b");
            @params[1] = new RangeValueEventProp("asName", "b");
            @params[2] = new RangeValueEventProp("asName", "BoolPrimitive");
            @params[3] = new RangeValueEventProp("asName", "IntPrimitive");
            @params[4] = new RangeValueEventProp("asName", "IntPrimitive");
        }
    
        [Test]
        public void TestGetFilterValue()
        {
            SupportBean eventBean = new SupportBean();
            eventBean.IntPrimitive = 1000;
            EventBean theEvent = SupportEventBeanFactory.CreateObject(eventBean);
            MatchedEventMap matchedEvents = new MatchedEventMapImpl(new MatchedEventMapMeta(new String[] { "asName" }, false));
            matchedEvents.Add(0, theEvent);
    
            TryInvalidGetFilterValue(matchedEvents, @params[0]);
            TryInvalidGetFilterValue(matchedEvents, @params[1]);
            Assert.AreEqual(1000.0, @params[3].GetFilterValue(matchedEvents, null));
        }
    
        [Test]
        public void TestEquals()
        {
            Assert.IsFalse(@params[0].Equals(@params[1]));
            Assert.IsFalse(@params[2].Equals(@params[3]));
            Assert.IsTrue(@params[3].Equals(@params[4]));
        }
    
        private void TryInvalidGetFilterValue(MatchedEventMap matchedEvents, FilterSpecParamRangeValue value)
        {
            try
            {
                value.GetFilterValue(matchedEvents, null);
                Assert.Fail();
            }
            catch (IllegalStateException ex)
            {
                // expected
            }
            catch (PropertyAccessException ex)
            {
                // expected
            }
        }
    }
}
