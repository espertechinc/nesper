///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.supportunit.@event;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.@event.map
{
    [TestFixture]
    public class TestMapEventBean : AbstractCommonTest
    {
        private IDictionary<string, object> testTypesMap;
        private IDictionary<string, object> testValuesMap;

        private EventType eventType;
        private MapEventBean eventBean;

        private readonly SupportBeanComplexProps supportBean = SupportBeanComplexProps.MakeDefaultBean();

        [SetUp]
        public void SetUp()
        {
            testTypesMap = new Dictionary<string, object>();
            testTypesMap.Put("aString", typeof(string));
            testTypesMap.Put("anInt", typeof(int?));
            testTypesMap.Put("MyComplexBean", typeof(SupportBeanComplexProps));

            testValuesMap = new Dictionary<string, object>();
            testValuesMap.Put("aString", "test");
            testValuesMap.Put("anInt", 10);
            testValuesMap.Put("MyComplexBean", supportBean);

            EventTypeMetadata metadata = new EventTypeMetadata("MyType", null, EventTypeTypeClass.STREAM, EventTypeApplicationType.MAP, NameAccessModifier.INTERNAL, EventTypeBusModifier.NONBUS, false, EventTypeIdPair.Unassigned());
            eventType = new MapEventType(metadata, testTypesMap, null, null, null, null,
                SupportEventTypeFactory.GetInstance(container).BEAN_EVENT_TYPE_FACTORY);
            eventBean = new MapEventBean(testValuesMap, eventType);
        }

        [Test, RunInApplicationDomain]
        public void TestGet()
        {
            Assert.AreEqual(eventType, eventBean.EventType);
            Assert.AreEqual(testValuesMap, eventBean.Underlying);

            Assert.AreEqual("test", eventBean.Get("aString"));
            Assert.AreEqual(10, eventBean.Get("anInt"));

            Assert.AreEqual("NestedValue", eventBean.Get("MyComplexBean.Nested.NestedValue"));

            // test wrong property name
            try
            {
                eventBean.Get("dummy");
                Assert.IsTrue(false);
            }
            catch (PropertyAccessException ex)
            {
                // Expected
                log.Debug(".testGetter Expected exception, msg=" + ex.Message);
            }
        }

        [Test, RunInApplicationDomain]
        public void TestCreateUnderlying()
        {
            SupportBean beanOne = new SupportBean();
            SupportBean_A beanTwo = new SupportBean_A("a");

            // Set up event type
            testTypesMap.Clear();
            testTypesMap.Put("a", typeof(SupportBean));
            testTypesMap.Put("b", typeof(SupportBean_A));
            EventTypeMetadata metadata = new EventTypeMetadata("MyType", null, EventTypeTypeClass.STREAM, EventTypeApplicationType.MAP, NameAccessModifier.INTERNAL, EventTypeBusModifier.NONBUS, false, EventTypeIdPair.Unassigned());
            EventType eventType = new MapEventType(metadata, testTypesMap, null, null, null, null,
                SupportEventTypeFactory.GetInstance(container).BEAN_EVENT_TYPE_FACTORY);

            IDictionary<string, object> events = new Dictionary<string, object>();
            events.Put("a", beanOne);
            events.Put("b", beanTwo);

            MapEventBean theEvent = new MapEventBean(events, eventType);
            Assert.AreSame(theEvent.Get("a"), beanOne);
            Assert.AreSame(theEvent.Get("b"), beanTwo);
        }

        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
