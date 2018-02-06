///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using NUnit.Framework;

using System;
using System.Collections.Generic;

using Avro.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.util;

using Newtonsoft.Json.Linq;

using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestUpdateMapIndexProps
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;

        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName);
            }
            _listener = new SupportUpdateListener();
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.EndTest();
            }
            _listener = null;
        }

        [Test]
        public void TestSetMapProps()
        {
            RunAssertionSetMapPropsBean();

            EnumHelper.ForEach<EventRepresentationChoice>(rep => RunAssertionUpdateIStreamSetMapProps(rep));
            EnumHelper.ForEach<EventRepresentationChoice>(rep => RunAssertionNamedWindowSetMapProps(rep));
        }

        private void RunAssertionSetMapPropsBean()
        {
            // test update-istream with bean
            _epService.EPAdministrator.Configuration.AddEventType(typeof(MyMapPropEvent));
            _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean));

            _epService.EPAdministrator.CreateEPL("insert into MyStream select * from MyMapPropEvent");

            EPStatement stmtUpdOne = _epService.EPAdministrator.CreateEPL("update istream MyStream set Props('abc') = 1, array[2] = 10");
            stmtUpdOne.AddListener(_listener);

            _epService.EPRuntime.SendEvent(new MyMapPropEvent());
            EPAssertionUtil.AssertProps(_listener.AssertPairGetIRAndReset(), "Props('abc'),array[2]".Split(','), new Object[] { 1, 10 }, new Object[] { null, null });
        }

        private void RunAssertionUpdateIStreamSetMapProps(EventRepresentationChoice eventRepresentationEnum)
        {

            // test update-istream with map
            _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema MyInfraType(simple string, myarray int[], mymap com.espertech.esper.compat.collections.StringMap)");
            EPStatement stmtUpdTwo = _epService.EPAdministrator.CreateEPL("update istream MyInfraType set simple='A', mymap('abc') = 1, myarray[2] = 10");
            stmtUpdTwo.AddListener(_listener);

            if (eventRepresentationEnum.IsObjectArrayEvent())
            {
                _epService.EPRuntime.SendEvent(new Object[] { null, new int[10], new Dictionary<string, Object>() }, "MyInfraType");
            }
            else if (eventRepresentationEnum.IsMapEvent())
            {
                _epService.EPRuntime.SendEvent(MakeMapEvent(new Dictionary<string, object>(), new int[10]), "MyInfraType");
            }
            else if (eventRepresentationEnum.IsAvroEvent())
            {
                GenericRecord theEvent = new GenericRecord(SupportAvroUtil.GetAvroSchema(_epService, "MyInfraType").AsRecordSchema());
                theEvent.Put("myarray", Collections.List(0, 0, 0, 0, 0));
                theEvent.Put("mymap", new Dictionary<string, object>());
                _epService.EPRuntime.SendEventAvro(theEvent, "MyInfraType");
            } else {
                Assert.Fail();
            }
            EPAssertionUtil.AssertProps(_listener.AssertPairGetIRAndReset(), "simple,mymap('abc'),myarray[2]".Split(','), new Object[] {"A", 1, 10}, new Object[] {null, null, 0});
    
            _epService.Initialize();
        }
    
        private void RunAssertionNamedWindowSetMapProps(EventRepresentationChoice eventRepresentationEnum)
        {

            // test named-window update
            _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean));
            _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema MyNWInfraType(simple string, myarray int[], mymap com.espertech.esper.compat.collections.StringMap)");
            EPStatement stmtWin = _epService.EPAdministrator.CreateEPL("create window MyWindow#keepall as MyNWInfraType");
            _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " insert into MyWindow select * from MyNWInfraType");

            if (eventRepresentationEnum.IsObjectArrayEvent())
            {
                _epService.EPRuntime.SendEvent(new Object[] { null, new int[10], new Dictionary<string, Object>() }, "MyNWInfraType");
            }
            else if (eventRepresentationEnum.IsMapEvent())
            {
                _epService.EPRuntime.SendEvent(MakeMapEvent(new Dictionary<string, object>(), new int[10]), "MyNWInfraType");
            }
            else if (eventRepresentationEnum.IsAvroEvent())
            {
                GenericRecord theEvent = new GenericRecord(SupportAvroUtil.GetAvroSchema(_epService, "MyNWInfraType").AsRecordSchema());
                theEvent.Put("myarray", Collections.List(0, 0, 0, 0, 0));
                theEvent.Put("mymap", new Dictionary<string, object>());
                _epService.EPRuntime.SendEventAvro(theEvent, "MyNWInfraType");
            }
            else
            {
                Assert.Fail();
            }
            _epService.EPAdministrator.CreateEPL("on SupportBean update MyWindow set simple='A', mymap('abc') = intPrimitive, myarray[2] = intPrimitive");
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            EPAssertionUtil.AssertPropsPerRow(stmtWin.GetEnumerator(), "simple,mymap('abc'),myarray[2]".Split(','), new Object[][] { new Object[] { "A", 10, 10 } });

            // test null and array too small
            if (eventRepresentationEnum.IsObjectArrayEvent())
            {
                _epService.EPRuntime.SendEvent(new Object[] { null, new int[2], null }, "MyNWInfraType");
            }
            else if (eventRepresentationEnum.IsMapEvent())
            {
                _epService.EPRuntime.SendEvent(MakeMapEvent(null, new int[2]), "MyNWInfraType");
            }
            else if (eventRepresentationEnum.IsAvroEvent())
            {
                GenericRecord theEvent = new GenericRecord(
                    SchemaBuilder.Record(
                        "name",
                        TypeBuilder.OptionalString("simple"),
                        TypeBuilder.Field("myarray", TypeBuilder.Array(TypeBuilder.Long())),
                        TypeBuilder.Field("mymap", TypeBuilder.Map(TypeBuilder.String()))));

                theEvent.Put("myarray", Collections.List(0, 0));
                theEvent.Put("mymap", null);
                _epService.EPRuntime.SendEventAvro(theEvent, "MyNWInfraType");
            }
            else
            {
                Assert.Fail();
            }
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtWin.GetEnumerator(), "simple,mymap('abc'),myarray[2]".Split(','), new Object[][] { new Object[] { "A", 20, 20 }, new Object[] { "A", null, null } });

            _epService.Initialize();
        }

        private IDictionary<string, Object> MakeMapEvent(IDictionary<string, Object> mymap, int[] myarray)
        {
            var map = new LinkedHashMap<string, Object>();
            map.Put("mymap", mymap);
            map.Put("myarray", myarray);
            return map;
        }

        [Serializable]
        public class MyMapPropEvent
        {
            private IDictionary<string,object> _props = new Dictionary<string, object>();
            private Object[] _array = new Object[10];

            public void SetProps(string name, Object value)
            {
                _props.Put(name, value);
            }

            public void SetArray(int index, Object value)
            {
                _array[index] = value;
            }

            public IDictionary<string, object> Props
            {
                get { return _props; }
                set { this._props = value; }
            }

            public object[] Array
            {
                get { return _array; }
                set { this._array = value; }
            }

            public Object GetArray(int index)
            {
                return _array[index];
            }
        }
    }
} // end of namespace
