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
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.named;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestNamedWindowInsertFrom
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener[] _listeners;
    
        [SetUp]
        public void SetUp() {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("SupportBean", typeof(SupportBean));
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);
            }
    
            _listeners = new SupportUpdateListener[10];
            for (int i = 0; i < _listeners.Length; i++) {
                _listeners[i] = new SupportUpdateListener();
            }
        }

        [TearDown]    
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.EndTest();
            }
            _listeners = null;
        }
    
        [Test]
        public void TestCreateNamedAfterNamed() {
            // create window
            string stmtTextCreateOne = "create window MyWindow#keepall as SupportBean";
            EPStatement stmtCreateOne = _epService.EPAdministrator.CreateEPL(stmtTextCreateOne);
            stmtCreateOne.AddListener(_listeners[0]);
    
            // create window
            string stmtTextCreateTwo = "create window MyWindowTwo#keepall as MyWindow";
            EPStatement stmtCreateTwo = _epService.EPAdministrator.CreateEPL(stmtTextCreateTwo);
            stmtCreateTwo.AddListener(_listeners[1]);
    
            // create insert into
            string stmtTextInsertOne = "insert into MyWindow select * from SupportBean";
            _epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // create consumer
            string stmtTextSelectOne = "select theString from MyWindow";
            EPStatement stmtSelectOne = _epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            stmtSelectOne.AddListener(_listeners[2]);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            var fields = new string[] {"theString"};
            EPAssertionUtil.AssertProps(_listeners[0].AssertOneGetNewAndReset(), fields, new Object[] {"E1"});
            EPAssertionUtil.AssertProps(_listeners[2].AssertOneGetNewAndReset(), fields, new Object[] {"E1"});
        }
    
        public void TestInsertWhereTypeAndFilter() {
            var fields = new string[] {"theString"};
    
            // create window
            string stmtTextCreateOne = "create window MyWindow#keepall as SupportBean";
            EPStatement stmtCreateOne = _epService.EPAdministrator.CreateEPL(stmtTextCreateOne, "name1");
            stmtCreateOne.AddListener(_listeners[0]);
            EventType eventTypeOne = stmtCreateOne.EventType;
    
            // create insert into
            string stmtTextInsertOne = "insert into MyWindow select * from SupportBean(intPrimitive > 0)";
            _epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // populate some data
            Assert.AreEqual(0, GetCount("MyWindow"));
            _epService.EPRuntime.SendEvent(new SupportBean("A1", 1));
            Assert.AreEqual(1, GetCount("MyWindow"));
            _epService.EPRuntime.SendEvent(new SupportBean("B2", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("C3", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("A4", 4));
            _epService.EPRuntime.SendEvent(new SupportBean("C5", 4));
            Assert.AreEqual(5, GetCount("MyWindow"));
            Assert.AreEqual("name1", GetStatementName("MyWindow"));
            Assert.AreEqual(stmtTextCreateOne, GetEPL("MyWindow"));
            _listeners[0].Reset();
    
            // create window with keep-all
            string stmtTextCreateTwo = "create window MyWindowTwo#keepall as MyWindow insert";
            EPStatement stmtCreateTwo = _epService.EPAdministrator.CreateEPL(stmtTextCreateTwo);
            stmtCreateTwo.AddListener(_listeners[2]);
            EPAssertionUtil.AssertPropsPerRow(stmtCreateTwo.GetEnumerator(), fields, new Object[][] { new Object[] { "A1"}, new Object[] { "B2"}, new Object[] { "C3"}, new Object[] { "A4"}, new Object[] { "C5"}});
            EventType eventTypeTwo = stmtCreateTwo.First().EventType;
            Assert.IsFalse(_listeners[2].IsInvoked);
            Assert.AreEqual(5, GetCount("MyWindowTwo"));
            Assert.AreEqual(StatementType.CREATE_WINDOW, ((EPStatementSPI) stmtCreateTwo).StatementMetadata.StatementType);
    
            // create window with keep-all and filter
            string stmtTextCreateThree = "create window MyWindowThree#keepall as MyWindow insert where theString like 'A%'";
            EPStatement stmtCreateThree = _epService.EPAdministrator.CreateEPL(stmtTextCreateThree);
            stmtCreateThree.AddListener(_listeners[3]);
            EPAssertionUtil.AssertPropsPerRow(stmtCreateThree.GetEnumerator(), fields, new Object[][] { new Object[] { "A1"}, new Object[] { "A4"}});
            EventType eventTypeThree = stmtCreateThree.First().EventType;
            Assert.IsFalse(_listeners[3].IsInvoked);
            Assert.AreEqual(2, GetCount("MyWindowThree"));
    
            // create window with last-per-id
            string stmtTextCreateFour = "create window MyWindowFour#Unique(intPrimitive) as MyWindow insert";
            EPStatement stmtCreateFour = _epService.EPAdministrator.CreateEPL(stmtTextCreateFour);
            stmtCreateFour.AddListener(_listeners[4]);
            EPAssertionUtil.AssertPropsPerRow(stmtCreateFour.GetEnumerator(), fields, new Object[][] { new Object[] { "C3"}, new Object[] { "C5"}});
            EventType eventTypeFour = stmtCreateFour.First().EventType;
            Assert.IsFalse(_listeners[4].IsInvoked);
            Assert.AreEqual(2, GetCount("MyWindowFour"));
    
            _epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean(theString like 'A%')");
            _epService.EPAdministrator.CreateEPL("insert into MyWindowTwo select * from SupportBean(theString like 'B%')");
            _epService.EPAdministrator.CreateEPL("insert into MyWindowThree select * from SupportBean(theString like 'C%')");
            _epService.EPAdministrator.CreateEPL("insert into MyWindowFour select * from SupportBean(theString like 'D%')");
            Assert.IsFalse(_listeners[0].IsInvoked || _listeners[2].IsInvoked || _listeners[3].IsInvoked || _listeners[4].IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("B9", -9));
            EventBean received = _listeners[2].AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(received, fields, new Object[] {"B9"});
            Assert.AreSame(eventTypeTwo, received.EventType);
            Assert.IsFalse(_listeners[0].IsInvoked || _listeners[3].IsInvoked || _listeners[4].IsInvoked);
            Assert.AreEqual(6, GetCount("MyWindowTwo"));
    
            _epService.EPRuntime.SendEvent(new SupportBean("A8", -8));
            received = _listeners[0].AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(received, fields, new Object[] {"A8"});
            Assert.AreSame(eventTypeOne, received.EventType);
            Assert.IsFalse(_listeners[2].IsInvoked || _listeners[3].IsInvoked || _listeners[4].IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("C7", -7));
            received = _listeners[3].AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(received, fields, new Object[] {"C7"});
            Assert.AreSame(eventTypeThree, received.EventType);
            Assert.IsFalse(_listeners[2].IsInvoked || _listeners[0].IsInvoked || _listeners[4].IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("D6", -6));
            received = _listeners[4].AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(received, fields, new Object[] {"D6"});
            Assert.AreSame(eventTypeFour, received.EventType);
            Assert.IsFalse(_listeners[2].IsInvoked || _listeners[0].IsInvoked || _listeners[3].IsInvoked);
        }

        [Test]
        public void TestInsertWhereOMStaggered()
        {
            EnumHelper.ForEach<EventRepresentationChoice>(RunAssertionInsertWhereOMStaggered);
        }
    
        private void RunAssertionInsertWhereOMStaggered(EventRepresentationChoice eventRepresentationEnum) {
    
            IDictionary<string, Object> dataType = MakeMap(new Object[][] { new Object[] { "a", typeof(string)}, new Object[] { "b", typeof(int)}});
            _epService.EPAdministrator.Configuration.AddEventType("MyMap", dataType);
    
            string stmtTextCreateOne = eventRepresentationEnum.GetAnnotationText() + " create window MyWindow#keepall as select a, b from MyMap";
            EPStatement stmtCreateOne = _epService.EPAdministrator.CreateEPL(stmtTextCreateOne);
            Assert.IsTrue(eventRepresentationEnum.MatchesClass(stmtCreateOne.EventType.UnderlyingType));
            stmtCreateOne.AddListener(_listeners[0]);
    
            // create insert into
            string stmtTextInsertOne = "insert into MyWindow select a, b from MyMap";
            _epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // populate some data
            _epService.EPRuntime.SendEvent(MakeMap(new Object[][] { new Object[] { "a", "E1"}, new Object[] { "b", 2}}), "MyMap");
            _epService.EPRuntime.SendEvent(MakeMap(new Object[][] { new Object[] { "a", "E2"}, new Object[] { "b", 10}}), "MyMap");
            _epService.EPRuntime.SendEvent(MakeMap(new Object[][] { new Object[] { "a", "E3"}, new Object[] { "b", 10}}), "MyMap");
    
            // create window with keep-all using OM
            var model = new EPStatementObjectModel();
            eventRepresentationEnum.AddAnnotationForNonMap(model);
            Expression where = Expressions.Eq("b", 10);
            model.CreateWindow = CreateWindowClause.Create("MyWindowTwo", View.Create("keepall")).SetIsInsert(true).SetInsertWhereClause(where);
            model.SelectClause = SelectClause.CreateWildcard();
            model.FromClause = FromClause.Create(FilterStream.Create("MyWindow"));
            string text = eventRepresentationEnum.GetAnnotationTextForNonMap() + " create window MyWindowTwo#keepall as select * from MyWindow insert where b=10";
            Assert.AreEqual(text.Trim(), model.ToEPL());
    
            EPStatementObjectModel modelTwo = _epService.EPAdministrator.CompileEPL(text);
            Assert.AreEqual(text.Trim(), modelTwo.ToEPL());
    
            EPStatement stmt = _epService.EPAdministrator.Create(modelTwo);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), "a,b".SplitCsv(), new Object[][] { new Object[] { "E2", 10}, new Object[] { "E3", 10}});
    
            // test select individual fields and from an insert-from named window
            stmt = _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create window MyWindowThree#keepall as select a from MyWindowTwo insert where a = 'E2'");
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), "a".SplitCsv(), new Object[][] { new Object[] { "E2"}});
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("MyWindow", true);
            _epService.EPAdministrator.Configuration.RemoveEventType("MyWindowTwo", true);
            _epService.EPAdministrator.Configuration.RemoveEventType("MyWindowThree", true);
        }

        [Test]
        public void TestVariantStream() {
            _epService.EPAdministrator.Configuration.AddEventType("SupportBean_A", typeof(SupportBean_A));
            _epService.EPAdministrator.Configuration.AddEventType("SupportBean_B", typeof(SupportBean_B));
    
            var config = new ConfigurationVariantStream();
            //config.TypeVariance = ConfigurationVariantStream.TypeVariance.ANY;
            config.AddEventTypeName("SupportBean_A");
            config.AddEventTypeName("SupportBean_B");
            _epService.EPAdministrator.Configuration.AddVariantStream("VarStream", config);
            _epService.EPAdministrator.CreateEPL("create window MyWindow#keepall as select * from VarStream");
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("create window MyWindowTwo#keepall as MyWindow");
    
            _epService.EPAdministrator.CreateEPL("insert into VarStream select * from SupportBean_A");
            _epService.EPAdministrator.CreateEPL("insert into VarStream select * from SupportBean_B");
            _epService.EPAdministrator.CreateEPL("insert into MyWindowTwo select * from VarStream");
            _epService.EPRuntime.SendEvent(new SupportBean_A("A1"));
            _epService.EPRuntime.SendEvent(new SupportBean_B("B1"));
            EventBean[] events = EPAssertionUtil.EnumeratorToArray(stmt.GetEnumerator());
            Assert.AreEqual("A1", events[0].Get("id?"));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), "id?".SplitCsv(), new Object[][] { new Object[] { "A1"}, new Object[] { "B1"}});
        }

        [Test]
        public void TestInvalid() {
            string stmtTextCreateOne = "create window MyWindow#keepall as SupportBean";
            _epService.EPAdministrator.CreateEPL(stmtTextCreateOne);
    
            try {
                _epService.EPAdministrator.CreateEPL("create window testWindow3#keepall as SupportBean insert");
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual("A named window by name 'SupportBean' could not be located, use the insert-keyword with an existing named window [create window testWindow3#keepall as SupportBean insert]", ex.Message);
            }
    
            try {
                _epService.EPAdministrator.CreateEPL("create window testWindow3#keepall as select * from " + typeof(SupportBean).FullName + " insert where (intPrimitive = 10)");
                Assert.Fail();
            } catch (EPStatementException ex) {
                SupportMessageAssertUtil.AssertMessage(ex, "A named window by name '" + typeof(SupportBean).FullName + "' could not be located, use the insert-keyword with an existing named window [");
            }
    
            try {
                _epService.EPAdministrator.CreateEPL("create window MyWindowTwo#keepall as MyWindow insert where (select intPrimitive from SupportBean#lastevent)");
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual("Create window where-clause may not have a subselect [create window MyWindowTwo#keepall as MyWindow insert where (select intPrimitive from SupportBean#lastevent)]", ex.Message);
            }
    
            try {
                _epService.EPAdministrator.CreateEPL("create window MyWindowTwo#keepall as MyWindow insert where Sum(intPrimitive) > 2");
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual("Create window where-clause may not have an aggregation function [create window MyWindowTwo#keepall as MyWindow insert where Sum(intPrimitive) > 2]", ex.Message);
            }
    
            try {
                _epService.EPAdministrator.CreateEPL("create window MyWindowTwo#keepall as MyWindow insert where Prev(1, intPrimitive) = 1");
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual("Create window where-clause may not have a function that requires view resources (prior, prev) [create window MyWindowTwo#keepall as MyWindow insert where Prev(1, intPrimitive) = 1]", ex.Message);
            }
        }
    
        private IDictionary<string, Object> MakeMap(Object[][] entries) {
            var result = new Dictionary<string, Object>();
            if (entries == null) {
                return result;
            }
            for (int i = 0; i < entries.Length; i++) {
                result.Put((string) entries[i][0], entries[i][1]);
            }
            return result;
        }
    
        private long GetCount(string windowName) {
            NamedWindowProcessor processor = ((EPServiceProviderSPI)_epService).NamedWindowMgmtService.GetProcessor(windowName);
            return processor.GetProcessorInstance(null).CountDataWindow;
        }
    
        private string GetStatementName(string windowName) {
            NamedWindowProcessor processor = ((EPServiceProviderSPI)_epService).NamedWindowMgmtService.GetProcessor(windowName);
            return processor.StatementName;
        }
    
        private string GetEPL(string windowName) {
            NamedWindowProcessor processor = ((EPServiceProviderSPI)_epService).NamedWindowMgmtService.GetProcessor(windowName);
            return processor.EplExpression;
        }
    }
} // end of namespace
