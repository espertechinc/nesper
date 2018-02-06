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
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.events;

using NUnit.Framework;


namespace com.espertech.esper.filter
{
    [TestFixture]
    public class TestRangeValueEventProp 
    {
        private readonly FilterSpecParamFilterForEval[] _params = new FilterSpecParamFilterForEval[5];
    
        [SetUp]
        public void SetUp()
        {
            _params[0] = new FilterForEvalEventPropDouble("a", "b");
            _params[1] = new FilterForEvalEventPropDouble("asName", "b");
            _params[2] = new FilterForEvalEventPropDouble("asName", "BoolPrimitive");
            _params[3] = new FilterForEvalEventPropDouble("asName", "IntPrimitive");
            _params[4] = new FilterForEvalEventPropDouble("asName", "IntPrimitive");
        }
    
        [Test]
        public void TestGetFilterValue()
        {
            SupportBean eventBean = new SupportBean();
            eventBean.IntPrimitive = 1000;
            EventBean theEvent = SupportEventBeanFactory.CreateObject(eventBean);
            MatchedEventMap matchedEvents = new MatchedEventMapImpl(new MatchedEventMapMeta(new String[] { "asName" }, false));
            matchedEvents.Add(0, theEvent);
    
            TryInvalidGetFilterValue(matchedEvents, _params[0]);
            TryInvalidGetFilterValue(matchedEvents, _params[1]);
            Assert.AreEqual(1000.0, _params[3].GetFilterValue(matchedEvents, null));
        }
    
        [Test]
        public void TestEquals()
        {
            Assert.IsFalse(_params[0].Equals(_params[1]));
            Assert.IsFalse(_params[2].Equals(_params[3]));
            Assert.IsTrue(_params[3].Equals(_params[4]));
        }
    
        private void TryInvalidGetFilterValue(MatchedEventMap matchedEvents, FilterSpecParamFilterForEval value)
        {
            try
            {
                value.GetFilterValue(matchedEvents, null);
                Assert.Fail();
            }
            catch (IllegalStateException)
            {
                // expected
            }
            catch (PropertyAccessException)
            {
                // expected
            }
        }
    }
}
