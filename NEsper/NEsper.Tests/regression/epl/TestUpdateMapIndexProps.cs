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
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    using Map = IDictionary<string, object>;

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
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _listener = new SupportUpdateListener();
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }

        [Test]
        public void TestSetMapProps()
        {
            RunAssertionSetMapProps(EventRepresentationEnum.OBJECTARRAY);
            RunAssertionSetMapProps(EventRepresentationEnum.MAP);
            RunAssertionSetMapProps(EventRepresentationEnum.DEFAULT);
        }

        private void RunAssertionSetMapProps(EventRepresentationEnum eventRepresentationEnum)
        {
            // test Update-istream with bean
            _epService.EPAdministrator.Configuration.AddEventType(
                typeof (MyMapPropEvent));
            _epService.EPAdministrator.Configuration.AddEventType(
                typeof (SupportBean));

            _epService.EPAdministrator.CreateEPL(
                "insert into MyStream select * from MyMapPropEvent");

            EPStatement stmtUpdOne = _epService.EPAdministrator.CreateEPL(
                "update istream MyStream set Props('abc') = 1, Array[2] = 10");

            stmtUpdOne.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new MyMapPropEvent());
            EPAssertionUtil.AssertProps(
                _listener.AssertPairGetIRAndReset(),
                "Props('abc'),Array[2]".Split(','),
                new Object[]
                {
                    1, 10
                },
                new Object[]
                {
                    null, null
                });

            // test Update-istream with map
            _epService.EPAdministrator.CreateEPL(
                eventRepresentationEnum.GetAnnotationText()
                + " create schema MyMapType(Simple String, MyArray System.Object[], MyMap com.espertech.esper.support.util.QuickMap)");
            EPStatement stmtUpdTwo = _epService.EPAdministrator.CreateEPL(
                "Update istream MyMapType set Simple='A', MyMap('abc') = 1, MyArray[2] = 10");

            stmtUpdTwo.Events += _listener.Update;

            if (eventRepresentationEnum.IsObjectArrayEvent())
            {
                _epService.EPRuntime.SendEvent(
                    new Object[]
                    {
                        null, new Object[10], new Dictionary<String, Object>()
                    },
                    "MyMapType");
            }
            else
            {
                _epService.EPRuntime.SendEvent(
                    MakeMapEvent(new Dictionary<String, Object>(), new Object[10]),
                    "MyMapType");
            }
            EPAssertionUtil.AssertProps(
                _listener.AssertPairGetIRAndReset(),
                "Simple,MyMap('abc'),MyArray[2]".Split(','), 
                new Object[]
                {
                    "A", 1, 10
                },
                new Object[]
                {
                    null, null, null
                });

            // test named-window Update
            _epService.EPAdministrator.CreateEPL(
                eventRepresentationEnum.GetAnnotationText()
                + " create schema MyNWMapType(Simple String, MyArray System.Object[], MyMap com.espertech.esper.support.util.QuickMap)");
            EPStatement stmtWin = _epService.EPAdministrator.CreateEPL(
                "create window MyWindow.win:keepall() as MyNWMapType");

            _epService.EPAdministrator.CreateEPL(
                eventRepresentationEnum.GetAnnotationText()
                + " insert into MyWindow select * from MyNWMapType");

            if (eventRepresentationEnum.IsObjectArrayEvent())
            {
                _epService.EPRuntime.SendEvent(
                    new Object[]
                    {
                        null, new Object[10], new Dictionary<String, Object>()
                    }, 
                    "MyNWMapType");
            }
            else
            {
                _epService.EPRuntime.SendEvent(
                    MakeMapEvent(new Dictionary<String, Object>(), new Object[10]),
                    "MyNWMapType");
            }
            _epService.EPAdministrator.CreateEPL(
                "on SupportBean Update MyWindow set Simple='A', MyMap('abc') = IntPrimitive, MyArray[2] = IntPrimitive");
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            EPAssertionUtil.AssertPropsPerRow(
                stmtWin.GetEnumerator(),
                "Simple,MyMap('abc'),MyArray[2]".Split(','), 
                new Object[][]
                {
                    new Object[]
                    {
                        "A", 10, 10
                    }
                });

            // test null and array too small
            if (eventRepresentationEnum.IsObjectArrayEvent())
            {
                _epService.EPRuntime.SendEvent(
                    new Object[]
                    {
                        null, new Object[2], null
                    },
                    "MyNWMapType");
            }
            else
            {
                _epService.EPRuntime.SendEvent(MakeMapEvent(null, new Object[2]), "MyNWMapType");
            }

            _epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmtWin.GetEnumerator(),
                "Simple,MyMap('abc'),MyArray[2]".Split(','), new Object[][]
                {
                    new Object[]
                    {
                        "A", 20, 20
                    },
                    new Object[]
                    {
                        "A", null, null
                    }
                });

            _epService.Initialize();
        }

        private IDictionary<String, Object> MakeMapEvent(IDictionary<String, Object> mymap, Object[] myarray)
        {
            IDictionary<String, Object> map = new LinkedHashMap<String, Object>();

            map["MyMap"] = mymap;
            map["MyArray"] = myarray;
            return map;
        }

        [Serializable]
        public class MyMapPropEvent
        {
            public MyMapPropEvent()
            {
                Props = new NullableDictionary<string, object>();
                Array = new Object[10];
            }

            public void SetProps(String name, Object value)
            {
                Props.Put(name, value);
            }

            public void SetArray(int index, Object value)
            {
                Array[index] = value;
            }

            public Map Props { get; set; }

            public object[] Array { get; set; }

            public object GetArray(int index)
            {
                return Array[index];
            }
        }
    }
}
