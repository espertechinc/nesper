///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.support;
using com.espertech.esper.events;
using com.espertech.esper.pattern;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.supportunit.filter;
using com.espertech.esper.supportunit.util;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.filter
{
    [TestFixture]
    public class TestFilterSpecCompiled 
    {
        private EventType _eventType;
        private String _eventTypeName;
    
        [SetUp]
        public void SetUp()
        {
            _eventTypeName = typeof(SupportBean).FullName;
            _eventType = SupportContainer.Instance.Resolve<EventAdapterService>()
                .AddBeanType(_eventTypeName, typeof(SupportBean), true, true, true);
        }
    
        [Test]
        public void TestEquals()
        {
            Object[][] paramList =
            {
                new Object[] { "IntPrimitive", FilterOperator.EQUAL, 2, "IntBoxed", FilterOperator.EQUAL, 3 },
                new Object[] { "IntPrimitive", FilterOperator.EQUAL, 3, "IntBoxed", FilterOperator.EQUAL, 3 },
                new Object[] { "IntPrimitive", FilterOperator.EQUAL, 2 },
                new Object[] { "IntPrimitive", FilterOperator.RANGE_CLOSED, 1, 10 },
                new Object[] { "IntPrimitive", FilterOperator.EQUAL, 2, "IntBoxed", FilterOperator.EQUAL, 3 },
                new Object[] { },
                new Object[] { },
            };
    
            var specVec = new List<FilterSpecCompiled>();
            foreach (Object[] param in paramList) {
                FilterSpecCompiled spec = SupportFilterSpecBuilder.Build(_eventType, param);
                specVec.Add(spec);
            }
    
            Assert.IsFalse(specVec[0].Equals(specVec[1]));
            Assert.IsFalse(specVec[0].Equals(specVec[2]));
            Assert.IsFalse(specVec[0].Equals(specVec[3]));
            Assert.AreEqual(specVec[0], specVec[4]);
            Assert.IsFalse(specVec[0].Equals(specVec[5]));
            Assert.AreEqual(specVec[5], specVec[6]);
    
            Assert.IsFalse(specVec[2].Equals(specVec[4]));
        }
    
        [Test]
        public void TestGetValueSet()
        {
            IList<FilterSpecParam> parameters = SupportFilterSpecBuilder.BuildList(_eventType, new Object[]
                                        { "IntPrimitive", FilterOperator.EQUAL, 2 });
            var numberCoercer = CoercerFactory.GetCoercer(typeof(int), typeof(double));
            parameters.Add(new FilterSpecParamEventProp(MakeLookupable("DoubleBoxed"), FilterOperator.EQUAL, "asName", "DoublePrimitive", false, numberCoercer, typeof(double?), "Test"));
            FilterSpecCompiled filterSpec = new FilterSpecCompiled(_eventType, "SupportBean", new IList<FilterSpecParam>[] { parameters }, null);
    
            SupportBean eventBean = new SupportBean();
            eventBean.DoublePrimitive = 999.999;
            EventBean theEvent = SupportEventBeanFactory.CreateObject(eventBean);
            MatchedEventMap matchedEvents = new MatchedEventMapImpl(new MatchedEventMapMeta(new String[] {"asName"}, false));
            matchedEvents.Add(0, theEvent);
            FilterValueSet valueSet = filterSpec.GetValueSet(matchedEvents, null, null);
    
            // Assert the generated filter value container
            Assert.AreSame(_eventType, valueSet.EventType);
            Assert.AreEqual(2, valueSet.Parameters[0].Length);
    
            // Assert the first param
            var param = valueSet.Parameters[0][0];
            Assert.AreEqual("IntPrimitive", param.Lookupable.Expression);
            Assert.AreEqual(FilterOperator.EQUAL, param.FilterOperator);
            Assert.AreEqual(2, param.FilterForValue);
    
            // Assert the second param
            param = (FilterValueSetParam) valueSet.Parameters[0][1];
            Assert.AreEqual("DoubleBoxed", param.Lookupable.Expression);
            Assert.AreEqual(FilterOperator.EQUAL, param.FilterOperator);
            Assert.AreEqual(999.999, param.FilterForValue);
        }
    
        private FilterSpecLookupable MakeLookupable(String fieldName) {
            return new FilterSpecLookupable(fieldName, _eventType.GetGetter(fieldName), _eventType.GetPropertyType(fieldName), false);
        }
    
        [Test]
        public void TestPresortParameters()
        {
            FilterSpecCompiled spec = MakeFilterValues(
                    "DoublePrimitive", FilterOperator.LESS, 1.1,
                    "DoubleBoxed", FilterOperator.LESS, 1.1,
                    "IntPrimitive", FilterOperator.EQUAL, 1,
                    "string", FilterOperator.EQUAL, "jack",
                    "IntBoxed", FilterOperator.EQUAL, 2,
                    "FloatBoxed", FilterOperator.RANGE_CLOSED, 1.1d, 2.2d);
    
            LinkedList<FilterSpecParam> copy = new LinkedList<FilterSpecParam>(spec.Parameters[0]);

            Assert.AreEqual("IntPrimitive", copy.PopFront().Lookupable.Expression);
            Assert.AreEqual("string", copy.PopFront().Lookupable.Expression);
            Assert.AreEqual("IntBoxed", copy.PopFront().Lookupable.Expression);
            Assert.AreEqual("FloatBoxed", copy.PopFront().Lookupable.Expression);
            Assert.AreEqual("DoublePrimitive", copy.PopFront().Lookupable.Expression);
            Assert.AreEqual("DoubleBoxed", copy.PopFront().Lookupable.Expression);
        }
    
        private FilterSpecCompiled MakeFilterValues(params object[] filterSpecArgs)
        {
            return SupportFilterSpecBuilder.Build(_eventType, filterSpecArgs);
        }
    }
}
