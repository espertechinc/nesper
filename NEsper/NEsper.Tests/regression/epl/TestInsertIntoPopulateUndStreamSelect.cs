///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestInsertIntoPopulateUndStreamSelect 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp() {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _listener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }

        [Test]
        public void TestNamedWindowInheritsMap()
        {
            var epl = "create objectarray schema Event();\n" +
                    "create objectarray schema ChildEvent(id string, action string) inherits Event;\n" +
                    "create objectarray schema Incident(name string, event Event);\n" +
                    "@Name('window') create window IncidentWindow.win:keepall() as Incident;\n" +
                    "\n" +
                    "on ChildEvent e\n" +
                    "    merge IncidentWindow w\n" +
                    "    where e.id = cast(w.event.id? as string)\n" +
                    "    when not matched\n" +
                    "        then insert (name, event) select 'ChildIncident', e \n" +
                    "            where e.action = 'INSERT'\n" +
                    "    when matched\n" +
                    "        then update set w.event = e \n" +
                    "            where e.action = 'INSERT'\n" +
                    "        then delete\n" +
                    "            where e.action = 'CLEAR';";
            _epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);

            _epService.EPRuntime.SendEvent(new Object[] {"ID1", "INSERT"}, "ChildEvent");
            var @event = _epService.EPAdministrator.GetStatement("window").First();
            var underlying = @event.Underlying.UnwrapIntoArray<object>();
            Assert.AreEqual("ChildIncident", underlying[0]);
            var underlyingInner = ((EventBean) underlying[1]).Underlying.UnwrapIntoArray<object>();
            EPAssertionUtil.AssertEqualsExactOrder(new Object[] {"ID1", "INSERT"}, underlyingInner);
        }

        [Test]
        public void TestNamedWindowOA() {
            RunAssertionNamedWindow(TypeTested.OA);
        }
    
        [Test]
        public void TestNamedWindowMap() {
            RunAssertionNamedWindow(TypeTested.MAP);
        }
    
        [Test]
        public void TestStreamInsertWWidenOA() {
            RunAssertionStreamInsertWWidenMap(TypeTested.OA);
        }
    
        [Test]
        public void TestStreamInsertWWidenMap() {
            RunAssertionStreamInsertWWidenMap(TypeTested.MAP);
        }
    
        [Test]
        public void TestInvalidOA() {
            RunAssertionInvalid(TypeTested.OA);
        }
    
        [Test]
        public void TestInvalidMap() {
            RunAssertionInvalid(TypeTested.MAP);
        }
    
        private void RunAssertionNamedWindow(TypeTested typeTested) {
            if (typeTested == TypeTested.MAP) {
                IDictionary<String, Object> typeinfo = new Dictionary<String, Object>();
                typeinfo.Put("myint", typeof(int));
                typeinfo.Put("mystr", typeof(String));
                _epService.EPAdministrator.Configuration.AddEventType("A", typeinfo);
                _epService.EPAdministrator.CreateEPL("create map schema C as (addprop int) inherits A");
            }
            else if (typeTested == TypeTested.OA) {
                _epService.EPAdministrator.Configuration.AddEventType("A", new String[]{"myint", "mystr"}, new Object[]{typeof(int), typeof(String)});
                _epService.EPAdministrator.CreateEPL("create objectarray schema C as (addprop int) inherits A");
            }
    
            _epService.EPAdministrator.CreateEPL("create window MyWindow.win:time(5 days) as C");
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select * from MyWindow");
            stmt.Events += _listener.Update;
    
            // select underlying
            EPStatement stmtInsert = _epService.EPAdministrator.CreateEPL("insert into MyWindow select mya.* from A as mya");
            if (typeTested == TypeTested.MAP) {
                _epService.EPRuntime.SendEvent(MakeMap(123, "abc"), "A");
            }
            else if (typeTested == TypeTested.OA) {
                _epService.EPRuntime.SendEvent(new Object[]{123, "abc"}, "A");
            }
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "myint,mystr,addprop".Split(','), new Object[]{123, "abc", null});
            stmtInsert.Dispose();
    
            // select underlying plus property
            _epService.EPAdministrator.CreateEPL("insert into MyWindow select mya.*, 1 as addprop from A as mya");
            if (typeTested == TypeTested.MAP) {
                _epService.EPRuntime.SendEvent(MakeMap(456, "def"), "A");
            }
            else if (typeTested == TypeTested.OA) {
                _epService.EPRuntime.SendEvent(new Object[] {456, "def"}, "A");
            }
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "myint,mystr,addprop".Split(','), new Object[]{456, "def", 1});
        }
    
        private void RunAssertionStreamInsertWWidenMap(TypeTested typeTested) {
    
            EPServiceProvider epService = EPServiceProviderManager.GetDefaultProvider();
            epService.EPAdministrator.CreateEPL("create " + GetText(typeTested) + " schema Src as (myint int, mystr string)");
    
            epService.EPAdministrator.CreateEPL("create " + GetText(typeTested) + " schema D1 as (myint int, mystr string, addprop long)");
            String eplOne = "insert into D1 select 1 as addprop, mysrc.* from Src as mysrc";
            RunStreamInsertAssertion(typeTested, eplOne, "myint,mystr,addprop", new Object[]{123, "abc", 1L});
    
            epService.EPAdministrator.CreateEPL("create " + GetText(typeTested) + " schema D2 as (mystr string, myint int, addprop double)");
            String eplTwo = "insert into D2 select 1 as addprop, mysrc.* from Src as mysrc";
            RunStreamInsertAssertion(typeTested, eplTwo, "myint,mystr,addprop", new Object[]{123, "abc", 1d});
    
            epService.EPAdministrator.CreateEPL("create " + GetText(typeTested) + " schema D3 as (mystr string, addprop int)");
            String eplThree = "insert into D3 select 1 as addprop, mysrc.* from Src as mysrc";
            RunStreamInsertAssertion(typeTested, eplThree, "mystr,addprop", new Object[]{"abc", 1});
    
            epService.EPAdministrator.CreateEPL("create " + GetText(typeTested) + " schema D4 as (myint int, mystr string)");
            String eplFour = "insert into D4 select mysrc.* from Src as mysrc";
            RunStreamInsertAssertion(typeTested, eplFour, "myint,mystr", new Object[]{123, "abc"});
    
            String eplFive = "insert into D4 select mysrc.*, 999 as myint, 'xxx' as mystr from Src as mysrc";
            RunStreamInsertAssertion(typeTested, eplFive, "myint,mystr", new Object[]{999, "xxx"});
            String eplSix = "insert into D4 select 999 as myint, 'xxx' as mystr, mysrc.* from Src as mysrc";
            RunStreamInsertAssertion(typeTested, eplSix, "myint,mystr", new Object[]{999, "xxx"});
        }
    
        public void RunAssertionInvalid(TypeTested typeTested) {
            _epService.EPAdministrator.CreateEPL("create " + GetText(typeTested) + " schema Src as (myint int, mystr string)");
    
            // mismatch in type
            _epService.EPAdministrator.CreateEPL("create " + GetText(typeTested) + " schema E1 as (myint long)");
            TryInvalid("insert into E1 select mysrc.* from Src as mysrc",
                    "Error starting statement: Type by name 'E1' in property 'myint' expected " + typeof(int?) + " but receives " + typeof(long?) + " [insert into E1 select mysrc.* from Src as mysrc]");
    
            // mismatch in column name
            _epService.EPAdministrator.CreateEPL("create " + GetText(typeTested) + " schema E2 as (someprop long)");
            TryInvalid("insert into E2 select mysrc.*, 1 as otherprop from Src as mysrc",
                    "Error starting statement: Failed to find column 'otherprop' in target type 'E2' [insert into E2 select mysrc.*, 1 as otherprop from Src as mysrc]");
        }
    
        private void RunStreamInsertAssertion(TypeTested typeTested, String epl, String fields, Object[] expected) {
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;
            if (TypeTested.MAP == typeTested) {
                _epService.EPRuntime.SendEvent(MakeMap(123, "abc"), "Src");
            }
            else {
                _epService.EPRuntime.SendEvent(new Object[] {123, "abc"}, "Src");
            }
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields.Split(','), expected);
            stmt.Dispose();
        }
    
        private void TryInvalid(String epl, String message) {
            try {
                _epService.EPAdministrator.CreateEPL(epl);
                Assert.Fail();
            }
            catch (EPStatementException ex) {
                Assert.AreEqual(message, ex.Message);
            }
        }
    
        private IDictionary<String, Object> MakeMap(int myint, String mystr) {
            IDictionary<String, Object> @event = new Dictionary<String, Object>();
            @event.Put("myint", myint);
            @event.Put("mystr", mystr);
            return @event;
        }

        internal static string GetText(TypeTested type)
        {
            switch (type)
            {
                case TypeTested.MAP:
                    return "map";
                case TypeTested.OA:
                    return "objectarray";
            }

            throw new ArgumentException();
        }

        public enum TypeTested
        {
            MAP,
            OA
        }
    }
}
