///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.events;

using NUnit.Framework;

namespace com.espertech.esper.epl.core
{
    [TestFixture]
    public class TestStreamTypeServiceImpl
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            SupportEventAdapterService.Reset();

            // Prepare regualar test service
            var eventTypes = new EventType[]
            {
                SupportEventTypeFactory.CreateBeanType(typeof (SupportBean)),
                SupportEventTypeFactory.CreateBeanType(typeof (SupportBean)),
                SupportEventTypeFactory.CreateBeanType(typeof (SupportBean_A)),
                SupportEventTypeFactory.CreateBeanType(typeof (SupportMarketDataBean),
                                                       "SupportMarketDataBean")
            }
                ;
            var eventTypeName = new String[]
            {
                "SupportBean", "SupportBean", "SupportBean_A",
                "SupportMarketDataBean"
            }
                ;
            var streamNames = new String[]
            {
                "s1", null, "s3", "s4"
            }
                ;

            _serviceRegular = new StreamTypeServiceImpl(eventTypes, streamNames,
                                                       new bool[10], "default", false);

            // Prepare with stream-zero being unambigous
            var streamTypes = new LinkedHashMap<String, Pair<EventType, String>>();

            for (int i = 0; i < streamNames.Length; i++)
            {
                streamTypes.Put(streamNames[i],
                                new Pair<EventType, String>(eventTypes[i], eventTypeName[i]));
            }
            _serviceStreamZeroUnambigous = new StreamTypeServiceImpl(streamTypes,
                                                                    "default", true, false);

            // Prepare requiring stream names for non-zero streams
            _serviceRequireStreamName = new StreamTypeServiceImpl(streamTypes,
                                                                 "default", true, true);
        }

        #endregion

        private StreamTypeServiceImpl _serviceRegular;
        private StreamTypeServiceImpl _serviceStreamZeroUnambigous;
        private StreamTypeServiceImpl _serviceRequireStreamName;

        private static void TryResolveByStreamAndPropNameBoth(StreamTypeService service)
        {
            // Test lookup by stream name and prop name
            PropertyResolutionDescriptor desc = service.ResolveByStreamAndPropName(
                "s4", "Volume", false);

            Assert.AreEqual(3, desc.StreamNum);
            Assert.AreEqual(typeof (long?), desc.PropertyType);
            Assert.AreEqual("Volume", desc.PropertyName);
            Assert.AreEqual("s4", desc.StreamName);
            Assert.AreEqual(typeof (SupportMarketDataBean),
                            desc.StreamEventType.UnderlyingType);

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
            PropertyResolutionDescriptor desc = service.ResolveByPropertyName(
                "Volume", false);

            Assert.AreEqual(3, (desc.StreamNum));
            Assert.AreEqual(typeof (long?), desc.PropertyType);
            Assert.AreEqual("Volume", desc.PropertyName);
            Assert.AreEqual("s4", desc.StreamName);
            Assert.AreEqual(typeof (SupportMarketDataBean),
                            desc.StreamEventType.UnderlyingType);

            try
            {
                service.ResolveByPropertyName("BoolPrimitive", false);
                Assert.Fail();
            }
            catch (DuplicatePropertyException ex)
            {
                // Expected
            }

            try
            {
                service.ResolveByPropertyName("xxxx", false);
                Assert.Fail();
            }
            catch (PropertyNotFoundException ex)
            {
                // Expected
            }
        }

        private static void TryResolveByStreamAndPropNameInOne(StreamTypeService service)
        {
            // Test lookup by stream name and prop name
            PropertyResolutionDescriptor desc = service.ResolveByStreamAndPropName(
                "s4.Volume", false);

            Assert.AreEqual(3, desc.StreamNum);
            Assert.AreEqual(typeof (long?), desc.PropertyType);
            Assert.AreEqual("Volume", desc.PropertyName);
            Assert.AreEqual("s4", desc.StreamName);
            Assert.AreEqual(typeof (SupportMarketDataBean),
                            desc.StreamEventType.UnderlyingType);

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
            desc = service.ResolveByStreamAndPropName("SupportMarketDataBean.Volume",
                                                      false);
            Assert.AreEqual(3, desc.StreamNum);

            // resolve by engine URI plus event type alias
            desc = service.ResolveByStreamAndPropName(
                "default.SupportMarketDataBean.Volume", false);
            Assert.AreEqual(3, desc.StreamNum);
        }

        [Test]
        public void TestResolveByPropertyName()
        {
            TryResolveByPropertyName(_serviceRegular);
            _serviceStreamZeroUnambigous.ResolveByPropertyName("BoolPrimitive", false);
            _serviceRequireStreamName.ResolveByPropertyName("BoolPrimitive", false);

            try
            {
                _serviceRequireStreamName.ResolveByPropertyName("Volume", false);
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
            TryResolveByStreamAndPropNameBoth(_serviceRegular);
            TryResolveByStreamAndPropNameBoth(_serviceStreamZeroUnambigous);
            TryResolveByStreamAndPropNameBoth(_serviceRequireStreamName);
        }

        [Test]
        public void TestResolveByStreamAndPropNameInOne()
        {
            TryResolveByStreamAndPropNameInOne(_serviceRegular);
            TryResolveByStreamAndPropNameInOne(_serviceStreamZeroUnambigous);
            TryResolveByStreamAndPropNameInOne(_serviceRequireStreamName);
        }
    }
}