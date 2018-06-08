///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Avro.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;

using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl.other
{
    using Map = IDictionary<string, object>;

    public class ExecEPLUpdateMapIndexProps : RegressionExecution
    {
        public override void Run(EPServiceProvider epService)
        {
            RunAssertionSetMapPropsBean(epService);

            foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>())
            {
                RunAssertionUpdateIStreamSetMapProps(epService, rep);
            }

            foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>())
            {
                RunAssertionNamedWindowSetMapProps(epService, rep);
            }
        }

        private void RunAssertionSetMapPropsBean(EPServiceProvider epService)
        {
            // test update-istream with bean
            epService.EPAdministrator.Configuration.AddEventType(typeof(MyMapPropEvent));
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();

            epService.EPAdministrator.CreateEPL("insert into MyStream select * from MyMapPropEvent");

            var stmtUpdOne =
                epService.EPAdministrator.CreateEPL("update istream MyStream set Props('abc') = 1, array[2] = 10");
            var listener = new SupportUpdateListener();
            stmtUpdOne.Events += listener.Update;

            epService.EPRuntime.SendEvent(new MyMapPropEvent());
            EPAssertionUtil.AssertProps(
                listener.AssertPairGetIRAndReset(), "Props('abc'),array[2]".Split(','), new object[] {1, 10},
                new object[] {null, null});
        }

        private void RunAssertionUpdateIStreamSetMapProps(
            EPServiceProvider epService, EventRepresentationChoice eventRepresentationEnum)
        {

            // test update-istream with map
            epService.EPAdministrator.CreateEPL(
                eventRepresentationEnum.GetAnnotationText() +
                " create schema MyInfraType(simple string, myarray int[], mymap Map)");
            var stmtUpdTwo = epService.EPAdministrator.CreateEPL(
                "update istream MyInfraType set simple='A', mymap('abc') = 1, myarray[2] = 10");
            var listener = new SupportUpdateListener();
            stmtUpdTwo.Events += listener.Update;

            if (eventRepresentationEnum.IsObjectArrayEvent())
            {
                epService.EPRuntime.SendEvent(
                    new object[] {null, new int[10], new Dictionary<string, object>()}, "MyInfraType");
            }
            else if (eventRepresentationEnum.IsMapEvent())
            {
                epService.EPRuntime.SendEvent(
                    MakeMapEvent(new Dictionary<string, object>(), new int[10]), "MyInfraType");
            }
            else if (eventRepresentationEnum.IsAvroEvent())
            {
                var @event = new GenericRecord(
                    SupportAvroUtil.GetAvroSchema(epService, "MyInfraType").AsRecordSchema());
                @event.Put("myarray", Collections.List(0, 0, 0, 0, 0));
                @event.Put("mymap", new Dictionary<string, object>());
                epService.EPRuntime.SendEventAvro(@event, "MyInfraType");
            }
            else
            {
                Assert.Fail();
            }

            EPAssertionUtil.AssertProps(
                listener.AssertPairGetIRAndReset(), "simple,mymap('abc'),myarray[2]".Split(','),
                new object[] {"A", 1, 10}, new object[] {null, null, 0});

            epService.EPAdministrator.DestroyAllStatements();
            foreach (var name in "MyInfraType".Split(','))
            {
                epService.EPAdministrator.Configuration.RemoveEventType(name, true);
            }
        }

        private void RunAssertionNamedWindowSetMapProps(
            EPServiceProvider epService, EventRepresentationChoice eventRepresentationEnum)
        {

            // test named-window update
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.CreateEPL(
                eventRepresentationEnum.GetAnnotationText() +
                " create schema MyNWInfraType(simple string, myarray int[], mymap Map)");
            var stmtWin =
                epService.EPAdministrator.CreateEPL("create window MyWindow#keepall as MyNWInfraType");
            epService.EPAdministrator.CreateEPL(
                eventRepresentationEnum.GetAnnotationText() + " insert into MyWindow select * from MyNWInfraType");

            if (eventRepresentationEnum.IsObjectArrayEvent())
            {
                epService.EPRuntime.SendEvent(
                    new object[] {null, new int[10], new Dictionary<string, object>()}, "MyNWInfraType");
            }
            else if (eventRepresentationEnum.IsMapEvent())
            {
                epService.EPRuntime.SendEvent(
                    MakeMapEvent(new Dictionary<string, object>(), new int[10]), "MyNWInfraType");
            }
            else if (eventRepresentationEnum.IsAvroEvent())
            {
                var @event = new GenericRecord(
                    SupportAvroUtil.GetAvroSchema(epService, "MyNWInfraType").AsRecordSchema());
                @event.Put("myarray", Collections.List(0, 0, 0, 0, 0));
                @event.Put("mymap", new Dictionary<string, object>());
                epService.EPRuntime.SendEventAvro(@event, "MyNWInfraType");
            }
            else
            {
                Assert.Fail();
            }

            epService.EPAdministrator.CreateEPL(
                "on SupportBean update MyWindow set simple='A', mymap('abc') = IntPrimitive, myarray[2] = IntPrimitive");
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            EPAssertionUtil.AssertPropsPerRow(
                stmtWin.GetEnumerator(), "simple,mymap('abc'),myarray[2]".Split(','),
                new[] {new object[] {"A", 10, 10}});

            // test null and array too small
            if (eventRepresentationEnum.IsObjectArrayEvent())
            {
                epService.EPRuntime.SendEvent(new object[] {null, new int[2], null}, "MyNWInfraType");
            }
            else if (eventRepresentationEnum.IsMapEvent())
            {
                epService.EPRuntime.SendEvent(MakeMapEvent(null, new int[2]), "MyNWInfraType");
            }
            else if (eventRepresentationEnum.IsAvroEvent())
            {
                var @event = new GenericRecord(
                    SchemaBuilder.Record("name",
                        TypeBuilder.OptionalString("simple"),
                        TypeBuilder.Field("myarray", TypeBuilder.Array(TypeBuilder.LongType())),
                        TypeBuilder.Field("mymap", TypeBuilder.Map(TypeBuilder.StringType()))));
                @event.Put("myarray", Collections.List(0, 0));
                @event.Put("mymap", null);
                epService.EPRuntime.SendEventAvro(@event, "MyNWInfraType");
            }
            else
            {
                Assert.Fail();
            }

            epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmtWin.GetEnumerator(), "simple,mymap('abc'),myarray[2]".Split(','),
                new[] {new object[] {"A", 20, 20}, new object[] {"A", null, null}});

            epService.EPAdministrator.DestroyAllStatements();
            foreach (var name in "MyNWInfraType,MyWindow".Split(','))
            {
                epService.EPAdministrator.Configuration.RemoveEventType(name, true);
            }
        }

        private IDictionary<string, object> MakeMapEvent(IDictionary<string, object> mymap, int[] myarray)
        {
            var map = new LinkedHashMap<string, object>();
            map.Put("mymap", mymap);
            map.Put("myarray", myarray);
            return map;
        }

        [Serializable]
        public class MyMapPropEvent
        {
            private Map _props = new Dictionary<string, object>();
            private object[] _array = new object[10];

            public Map Props
            {
                get => _props;
                set => _props = value;
            }

            public object[] Array
            {
                get => _array;
                set => _array = value;
            }

            public void SetProps(string name, object value)
            {
                _props.Put(name, value);
            }

            public void SetArray(int index, object value)
            {
                _array[index] = value;
            }

            public Map GetProps()
            {
                return _props;
            }

            public void SetProps(Map props)
            {
                _props = props;
            }

            public object[] GetArray()
            {
                return _array;
            }

            public void SetArray(object[] array)
            {
                _array = array;
            }

            public object GetArray(int index)
            {
                return _array[index];
            }
        }
    }
} // end of namespace
