///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.collection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.supportunit.bean;
using com.espertech.esper.common.@internal.supportunit.@event;
using com.espertech.esper.common.@internal.supportunit.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.container;
using NUnit.Framework;

namespace com.espertech.esper.common.@internal.epl.streamtype
{
    [TestFixture]
    public class TestStreamTypeServiceImpl : AbstractTestBase
    {
        private StreamTypeServiceImpl serviceRegular;
        private StreamTypeServiceImpl serviceStreamZeroUnambigous;
        private StreamTypeServiceImpl serviceRequireStreamName;

        [SetUp]
        public void SetUp()
        {
            // Prepare regualar test service
            var eventTypes = new EventType[]{
                supportEventTypeFactory.CreateBeanType(typeof(SupportBean)),
                supportEventTypeFactory.CreateBeanType(typeof(SupportBean)),
                supportEventTypeFactory.CreateBeanType(typeof(SupportBean_A)),
                supportEventTypeFactory.CreateBeanType(typeof(SupportMarketDataBean))
            };
            var eventTypeName = new string[] { "SupportBean", "SupportBean", "SupportBean_A", "SupportMarketDataBean" };
            var streamNames = new string[] { "s1", null, "s3", "s4" };
            serviceRegular = new StreamTypeServiceImpl(eventTypes, streamNames, new bool[10], false, false);

            // Prepare with stream-zero being unambigous
            var streamTypes = new LinkedHashMap<string, Pair<EventType, string>>();
            for (var i = 0; i < streamNames.Length; i++)
            {
                streamTypes.Put(streamNames[i], new Pair<EventType, string>(eventTypes[i], eventTypeName[i]));
            }
            serviceStreamZeroUnambigous = new StreamTypeServiceImpl(streamTypes, true, false);

            // Prepare requiring stream names for non-zero streams
            serviceRequireStreamName = new StreamTypeServiceImpl(streamTypes, true, true);
        }

        [Test]
        public void TestResolveByStreamAndPropNameInOne()
        {
            TryResolveByStreamAndPropNameInOne(serviceRegular);
            TryResolveByStreamAndPropNameInOne(serviceStreamZeroUnambigous);
            TryResolveByStreamAndPropNameInOne(serviceRequireStreamName);
        }

        [Test]
        public void TestResolveByPropertyName()
        {
            TryResolveByPropertyName(serviceRegular);
            serviceStreamZeroUnambigous.ResolveByPropertyName("BoolPrimitive", false);
            serviceRequireStreamName.ResolveByPropertyName("BoolPrimitive", false);

            try
            {
                serviceRequireStreamName.ResolveByPropertyName("Volume", false);
                Assert.Fail();
            }
            catch (PropertyNotFoundException ex)
            {
                // expected
            }
        }

        [Test]
        public void TestResolveByStreamAndPropNameBoth()
        {
            TryResolveByStreamAndPropNameBoth(serviceRegular);
            TryResolveByStreamAndPropNameBoth(serviceStreamZeroUnambigous);
            TryResolveByStreamAndPropNameBoth(serviceRequireStreamName);
        }

        private static void TryResolveByStreamAndPropNameBoth(StreamTypeService service)
        {
            // Test lookup by stream name and prop name
            var desc = service.ResolveByStreamAndPropName("s4", "Volume", false);
            Assert.AreEqual(3, desc.StreamNum);
            Assert.AreEqual(typeof(long?), desc.PropertyType);
            Assert.AreEqual("Volume", desc.PropertyName);
            Assert.AreEqual("s4", desc.StreamName);
            Assert.AreEqual(typeof(SupportMarketDataBean), desc.StreamEventType.UnderlyingType);

            try
            {
                service.ResolveByStreamAndPropName("xxx", "Volume", false);
                Assert.Fail();
            }
            catch (StreamNotFoundException ex)
            {
                // Expected
            }

            try
            {
                service.ResolveByStreamAndPropName("s4", "xxxx", false);
                Assert.Fail();
            }
            catch (PropertyNotFoundException ex)
            {
                // Expected
            }
        }

        private static void TryResolveByPropertyName(StreamTypeService service)
        {
            // Test lookup by property name only
            var desc = service.ResolveByPropertyName("Volume", false);
            Assert.AreEqual(3, desc.StreamNum);
            Assert.AreEqual(typeof(long?), desc.PropertyType);
            Assert.AreEqual("Volume", desc.PropertyName);
            Assert.AreEqual("s4", desc.StreamName);
            Assert.AreEqual(typeof(SupportMarketDataBean), desc.StreamEventType.UnderlyingType);

            try
            {
                service.ResolveByPropertyName("BoolPrimitive", false);
                Assert.Fail();
            }
            catch (DuplicatePropertyException)
            {
                // Expected
            }

            try
            {
                service.ResolveByPropertyName("xxxx", false);
                Assert.Fail();
            }
            catch (PropertyNotFoundException)
            {
                // Expected
            }
        }

        private static void TryResolveByStreamAndPropNameInOne(StreamTypeService service)
        {
            // Test lookup by stream name and prop name
            var desc = service.ResolveByStreamAndPropName("s4.Volume", false);
            Assert.AreEqual(3, desc.StreamNum);
            Assert.AreEqual(typeof(long?), desc.PropertyType);
            Assert.AreEqual("Volume", desc.PropertyName);
            Assert.AreEqual("s4", desc.StreamName);
            Assert.AreEqual(typeof(SupportMarketDataBean), desc.StreamEventType.UnderlyingType);

            try
            {
                service.ResolveByStreamAndPropName("xxx.Volume", false);
                Assert.Fail();
            }
            catch (PropertyNotFoundException ex)
            {
                // Expected
            }

            try
            {
                service.ResolveByStreamAndPropName("s4.xxxx", false);
                Assert.Fail();
            }
            catch (PropertyNotFoundException ex)
            {
                // Expected
            }

            // resolve by event type alias (table name)
            desc = service.ResolveByStreamAndPropName("SupportMarketDataBean.Volume", false);
            Assert.AreEqual(3, desc.StreamNum);

            // resolve by engine URI plus event type alias
            desc = service.ResolveByStreamAndPropName("default.SupportMarketDataBean.Volume", false);
            Assert.AreEqual(3, desc.StreamNum);
        }
    }
} // end of namespace
