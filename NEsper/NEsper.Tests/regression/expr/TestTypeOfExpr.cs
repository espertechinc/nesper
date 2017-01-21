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
using com.espertech.esper.client.soda;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.expr
{
    [TestFixture]
    public class TestTypeOfExpr
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            _listener = new SupportUpdateListener();
            _epService = EPServiceProviderManager.GetDefaultProvider(
                SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }

        #endregion

        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;

        private void TryInvalid(String epl, String message)
        {
            try
            {
                _epService.EPAdministrator.CreateEPL(epl);
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual(message, ex.Message);
            }
        }

        private void RunAssertion()
        {
            var fields = new String[]
            {
                "typeof(prop?)", "typeof(key)"
            };

            SendSchemaEvent(1, "E1");
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[] {"Int32", "String"}
                );

            SendSchemaEvent("test", "E2");
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[] {"String", "String"}
                );

            SendSchemaEvent(null, "E3");
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[] {null, "String"}
                );
        }

        private void SendSchemaEvent(Object prop, String key)
        {
            IDictionary<String, Object> theEvent = new Dictionary<String, Object>();

            theEvent["prop"] = prop;
            theEvent["key"] = key;

            if (EventRepresentationEnumExtensions.GetEngineDefault(_epService).IsObjectArrayEvent())
            {
                _epService.EPRuntime.SendEvent(theEvent, "MySchema");
            }
            else
            {
                _epService.EPRuntime.SendEvent(theEvent, "MySchema");
            }
        }

        private void RunAssertionVariantStream(EventRepresentationEnum eventRepresentationEnum)
        {
            _epService.EPAdministrator.Configuration.AddEventType(
                "SupportBean", typeof (SupportBean));

            _epService.EPAdministrator.CreateEPL(
                eventRepresentationEnum.GetAnnotationText()
                + " create schema EventOne as (key string)");
            _epService.EPAdministrator.CreateEPL(
                eventRepresentationEnum.GetAnnotationText()
                + " create schema EventTwo as (key string)");
            _epService.EPAdministrator.CreateEPL(
                eventRepresentationEnum.GetAnnotationText()
                + " create schema S0 as "
                + typeof (SupportBean_S0).FullName);
            _epService.EPAdministrator.CreateEPL(
                eventRepresentationEnum.GetAnnotationText()
                + " create variant schema VarSchema as *");

            _epService.EPAdministrator.CreateEPL(
                "insert into VarSchema select * from EventOne");
            _epService.EPAdministrator.CreateEPL(
                "insert into VarSchema select * from EventTwo");
            _epService.EPAdministrator.CreateEPL(
                "insert into VarSchema select * from S0");
            _epService.EPAdministrator.CreateEPL(
                "insert into VarSchema select * from SupportBean");

            String stmtText = "select Typeof(A) as t0 from VarSchema as A";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);

            stmt.Events += _listener.Update;

            if (eventRepresentationEnum.IsObjectArrayEvent())
            {
                _epService.EPRuntime.SendEvent(new Object[]{ "value" }, "EventOne");
            }
            else
            {
                _epService.EPRuntime.SendEvent(
                    Collections.SingletonMap<string, object>("key", "value"), "EventOne");
            }
            Assert.AreEqual("EventOne", _listener.AssertOneGetNewAndReset().Get("t0"));

            if (eventRepresentationEnum.IsObjectArrayEvent())
            {
                _epService.EPRuntime.SendEvent(new Object[]{ "value" }, "EventTwo");
            }
            else
            {
                _epService.EPRuntime.SendEvent(
                    Collections.SingletonMap<string, object>("key", "value"), "EventTwo");
            }
            Assert.AreEqual("EventTwo", _listener.AssertOneGetNewAndReset().Get("t0"));

            _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            Assert.AreEqual("S0", _listener.AssertOneGetNewAndReset().Get("t0"));

            _epService.EPRuntime.SendEvent(new SupportBean());
            Assert.AreEqual("SupportBean", _listener.AssertOneGetNewAndReset().Get("t0"));

            stmt.Dispose();
            _listener.Reset();
            stmt = _epService.EPAdministrator.CreateEPL(
                "select * from VarSchema Match_recognize(\n"
                + "  measures A as a, B as b\n" + "  pattern (A B)\n"
                + "  define A as Typeof(A) = \"EventOne\",\n"
                + "         B as Typeof(B) = \"EventTwo\"\n" + "  )");
            stmt.Events += _listener.Update;

            if (eventRepresentationEnum.IsObjectArrayEvent())
            {
                _epService.EPRuntime.SendEvent(new Object[]
                {
                    "value"
                }
                                              , "EventOne");
                _epService.EPRuntime.SendEvent(new Object[]
                {
                    "value"
                }
                                              , "EventTwo");
            }
            else
            {
                _epService.EPRuntime.SendEvent(
                    Collections.SingletonMap<string, object>("key", "value"), "EventOne");
                _epService.EPRuntime.SendEvent(
                    Collections.SingletonMap<string, object>("key", "value"), "EventTwo");
            }
            Assert.IsTrue(_listener.GetAndClearIsInvoked());

            _listener.Reset();
            _epService.Initialize();
        }

        public void RunAssertionFragment(EventRepresentationEnum eventRepresentationEnum)
        {
            _epService.EPAdministrator.CreateEPL(
                eventRepresentationEnum.GetAnnotationText()
                + " create schema InnerSchema as (key string)");
            _epService.EPAdministrator.CreateEPL(
                eventRepresentationEnum.GetAnnotationText()
                + " create schema MySchema as (inside InnerSchema, insidearr InnerSchema[])");

            var fields = new String[] { "t0", "t1" };
            String stmtText = eventRepresentationEnum.GetAnnotationText()
                              + " select Typeof(s0.inside) as t0, Typeof(s0.insidearr) as t1 from MySchema as s0";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);

            stmt.Events += _listener.Update;

            if (eventRepresentationEnum.IsObjectArrayEvent())
            {
                _epService.EPRuntime.SendEvent(new Object[2], "MySchema");
            }
            else
            {
                _epService.EPRuntime.SendEvent(new Dictionary<String, Object>(),
                                              "MySchema");
            }
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { null, null });

            if (eventRepresentationEnum.IsObjectArrayEvent())
            {
                _epService.EPRuntime.SendEvent(new Object[] { new Object[2], null }, "MySchema");
            }
            else
            {
                var theEvent = new Dictionary<String, Object>();
                theEvent["inside"] = new Dictionary<String, Object>();
                _epService.EPRuntime.SendEvent(theEvent, "MySchema");
            }
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { "InnerSchema", null });

            if (eventRepresentationEnum.IsObjectArrayEvent())
            {
                _epService.EPRuntime.SendEvent(new Object[] { null, new Object[2][] }, "MySchema");
            }
            else
            {
                var theEvent = new Dictionary<String, Object>();
                theEvent["insidearr"] = new IDictionary<string, object>[0];
                _epService.EPRuntime.SendEvent(theEvent, "MySchema");
            }

            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { null, "InnerSchema[]" });

            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("InnerSchema", true);
            _epService.EPAdministrator.Configuration.RemoveEventType("MySchema", true);
        }

        [Test]
        public void TestDynamicProps()
        {
            _epService.EPAdministrator.CreateEPL(
                EventRepresentationEnum.MAP.GetAnnotationText()
                + " create schema MySchema as (key string)");

            String stmtText = "select typeof(prop?), typeof(key) from MySchema as s0";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);

            stmt.Events += _listener.Update;

            RunAssertion();

            stmt.Dispose();
            EPStatementObjectModel model = _epService.EPAdministrator.CompileEPL(
                stmtText);

            Assert.AreEqual(stmtText, model.ToEPL());
            stmt = _epService.EPAdministrator.Create(model);
            stmt.Events += _listener.Update;

            RunAssertion();
        }

        [Test]
        public void TestFragment()
        {
            RunAssertionFragment(EventRepresentationEnum.DEFAULT);
            RunAssertionFragment(EventRepresentationEnum.OBJECTARRAY);
            RunAssertionFragment(EventRepresentationEnum.MAP);
        }

        [Test]
        public void TestInvalid()
        {
            TryInvalid("select typeof(xx) from System.Object",
                       "Error starting statement: Failed to validate select-clause expression 'typeof(xx)': Property named 'xx' is not valid in any stream [select typeof(xx) from System.Object]");
        }

        [Test]
        public void TestNamedUnnamedPONO()
        {
            // test name-provided or no-name-provided
            _epService.EPAdministrator.Configuration.AddEventType(
                "ISupportA", typeof (ISupportA));
            _epService.EPAdministrator.Configuration.AddEventType(
                "ISupportABCImpl", typeof (ISupportABCImpl));

            String stmtText = "select Typeof(A) as t0 from ISupportA as A";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);

            stmt.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new ISupportAImpl(null, null));
            Assert.AreEqual(typeof (ISupportAImpl).FullName,
                            _listener.AssertOneGetNewAndReset().Get("t0"));

            _epService.EPRuntime.SendEvent(
                new ISupportABCImpl(null, null, null, null));
            Assert.AreEqual("ISupportABCImpl",
                            _listener.AssertOneGetNewAndReset().Get("t0"));
        }

        [Test]
        public void TestVariantStream()
        {
            RunAssertionVariantStream(EventRepresentationEnum.OBJECTARRAY);
            RunAssertionVariantStream(EventRepresentationEnum.MAP);
            RunAssertionVariantStream(EventRepresentationEnum.DEFAULT);
        }
    }
}
