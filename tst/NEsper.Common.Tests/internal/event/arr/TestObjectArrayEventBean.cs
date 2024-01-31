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
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.@event.arr
{
    [TestFixture]
    public class TestObjectArrayEventBean : AbstractCommonTest
    {
        private string[] testProps;
        private object[] testTypes;
        private object[] testValues;

        private EventType eventType;
        private ObjectArrayEventBean eventBean;

        private readonly SupportBeanComplexProps supportBean = SupportBeanComplexProps.MakeDefaultBean();

        [SetUp]
        public void SetUp()
        {
            testProps = new string[] { "aString", "anInt", "MyComplexBean" };
            testTypes = new object[] { typeof(string), typeof(int?), typeof(SupportBeanComplexProps) };
            IDictionary<string, object> typeRep = new LinkedHashMap<string, object>();
            for (int i = 0; i < testProps.Length; i++)
            {
                typeRep.Put(testProps[i], testTypes[i]);
            }

            testValues = new object[] { "test", 10, supportBean };

            EventTypeMetadata metadata = new EventTypeMetadata("MyType", null, EventTypeTypeClass.STREAM, EventTypeApplicationType.OBJECTARR, NameAccessModifier.INTERNAL, EventTypeBusModifier.NONBUS, false, EventTypeIdPair.Unassigned());
            eventType = new ObjectArrayEventType(metadata, typeRep, null, null, null, null,
                SupportEventTypeFactory.GetInstance(container).BEAN_EVENT_TYPE_FACTORY);
            eventBean = new ObjectArrayEventBean(testValues, eventType);
        }

        [Test]
        public void TestGet()
        {
            ClassicAssert.AreEqual(eventType, eventBean.EventType);
            ClassicAssert.AreEqual(testValues, eventBean.Underlying);

            ClassicAssert.AreEqual("test", eventBean.Get("aString"));
            ClassicAssert.AreEqual(10, eventBean.Get("anInt"));

            ClassicAssert.AreEqual("NestedValue", eventBean.Get("MyComplexBean.Nested.NestedValue"));

            // test wrong property name
            try
            {
                eventBean.Get("dummy");
                ClassicAssert.IsTrue(false);
            }
            catch (PropertyAccessException ex)
            {
                // Expected
                Log.Debug(".testGetter Expected exception, msg=" + ex.Message);
            }
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
