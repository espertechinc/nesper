///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.pattern;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.filter
{
    [TestFixture]
    public class TestFilterSpecParamEventProp
    {
        private static FilterSpecParamEventProp MakeParam(String eventAsName, String property)
        {
            Coercer numberCoercer = CoercerFactory.GetCoercer(typeof (int), typeof (int));
            return new FilterSpecParamEventProp(MakeLookupable("IntPrimitive"), FilterOperator.EQUAL, eventAsName,
                                                property, false, numberCoercer, typeof (int), "Test");
        }

        private static FilterSpecLookupable MakeLookupable(String fieldName)
        {
            return new FilterSpecLookupable(fieldName, null, null, false);
        }

        [Test]
        public void TestEquals()
        {
            var @params = new FilterSpecParamEventProp[5];
            @params[0] = MakeParam("a", "IntBoxed");
            @params[1] = MakeParam("b", "IntBoxed");
            @params[2] = MakeParam("a", "IntPrimitive");
            @params[3] = MakeParam("c", "IntPrimitive");
            @params[4] = MakeParam("a", "IntBoxed");

            Assert.AreEqual(@params[0], @params[4]);
            Assert.AreEqual(@params[4], @params[0]);
            Assert.IsFalse(@params[0].Equals(@params[1]));
            Assert.IsFalse(@params[0].Equals(@params[2]));
            Assert.IsFalse(@params[0].Equals(@params[3]));
        }

        [Test]
        public void TestGetFilterValue()
        {
            FilterSpecParamEventProp @params = MakeParam("asName", "IntBoxed");

            var eventBean = new SupportBean();
            eventBean.IntBoxed = 1000;
            EventBean theEvent = SupportEventBeanFactory.CreateObject(eventBean);

            MatchedEventMap matchedEvents =
                new MatchedEventMapImpl(new MatchedEventMapMeta(new String[] {"asName"}, false));
            matchedEvents.Add(0, theEvent);

            Assert.AreEqual(1000, @params.GetFilterValue(matchedEvents, null));
        }
    }
}