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
using com.espertech.esper.core.service;
using com.espertech.esper.epl.named;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestNamedWindowInsertFrom 
    {
        private EPServiceProvider epService;
        private SupportUpdateListener[] listeners;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportBean>();
            epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, this.GetType(), GetType().FullName);}
    
            listeners = new SupportUpdateListener[10];
            for (int i = 0; i < listeners.Length; i++)
            {
                listeners[i] = new SupportUpdateListener();
            }
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
            listeners = null;
        }
    
        [Test]
        public void TestCreateNamedAfterNamed()
        {
            // create window
            string stmtTextCreateOne = "create window MyWindow.win:keepall() as SupportBean";
            EPStatement stmtCreateOne = epService.EPAdministrator.CreateEPL(stmtTextCreateOne);
            stmtCreateOne.AddListener(listeners[0]);
    
            // create window
            string stmtTextCreateTwo = "create window MyWindowTwo.win:keepall() as MyWindow";
            EPStatement stmtCreateTwo = epService.EPAdministrator.CreateEPL(stmtTextCreateTwo);
            stmtCreateTwo.AddListener(listeners[1]);
    
            // create insert into
            string stmtTextInsertOne = "insert into MyWindow select * from SupportBean";
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // create consumer
            string stmtTextSelectOne = "select TheString from MyWindow";
            EPStatement stmtSelectOne = epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            stmtSelectOne.AddListener(listeners[2]);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            string[] fields = new string[] {"TheString"};
            EPAssertionUtil.AssertProps(listeners[0].AssertOneGetNewAndReset(), fields, new object[]{"E1"});
            EPAssertionUtil.AssertProps(listeners[2].AssertOneGetNewAndReset(), fields, new object[]{"E1"});
        }
    
        [Test]
        public void TestInsertWhereTypeAndFilter() 
        {
            string[] fields = new string[] {"TheString"};
    
            // create window
            string stmtTextCreateOne = "create window MyWindow.win:keepall() as SupportBean";
            EPStatement stmtCreateOne = epService.EPAdministrator.CreateEPL(stmtTextCreateOne, "name1");
            stmtCreateOne.AddListener(listeners[0]);
            EventType eventTypeOne = stmtCreateOne.EventType;
    
            // create insert into
            string stmtTextInsertOne = "insert into MyWindow select * from SupportBean(IntPrimitive > 0)";
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // populate some data
            Assert.AreEqual(0, GetCount("MyWindow"));
            epService.EPRuntime.SendEvent(new SupportBean("A1", 1));
            Assert.AreEqual(1, GetCount("MyWindow"));
            epService.EPRuntime.SendEvent(new SupportBean("B2", 1));
            epService.EPRuntime.SendEvent(new SupportBean("C3", 1));
            epService.EPRuntime.SendEvent(new SupportBean("A4", 4));
            epService.EPRuntime.SendEvent(new SupportBean("C5", 4));
            Assert.AreEqual(5, GetCount("MyWindow"));
            Assert.AreEqual("name1", GetStatementName("MyWindow"));
            Assert.AreEqual(stmtTextCreateOne, GetEPL("MyWindow"));
            listeners[0].Reset();
            
            // create window with keep-all
            string stmtTextCreateTwo = "create window MyWindowTwo.win:keepall() as MyWindow insert";
            EPStatement stmtCreateTwo = epService.EPAdministrator.CreateEPL(stmtTextCreateTwo);
            stmtCreateTwo.AddListener(listeners[2]);
            EPAssertionUtil.AssertPropsPerRow(stmtCreateTwo.GetEnumerator(), fields, new object[][] { new object[] { "A1" }, new object[] { "B2" }, new object[] { "C3" }, new object[] { "A4" }, new object[] { "C5" } });
            EventType eventTypeTwo = stmtCreateTwo.First().EventType;
            Assert.IsFalse(listeners[2].IsInvoked);
            Assert.AreEqual(5, GetCount("MyWindowTwo"));
            Assert.AreEqual(StatementType.CREATE_WINDOW, ((EPStatementSPI) stmtCreateTwo).StatementMetadata.StatementType);
    
            // create window with keep-all and filter
            string stmtTextCreateThree = "create window MyWindowThree.win:keepall() as MyWindow insert where TheString like 'A%'";
            EPStatement stmtCreateThree = epService.EPAdministrator.CreateEPL(stmtTextCreateThree);
            stmtCreateThree.AddListener(listeners[3]);
            EPAssertionUtil.AssertPropsPerRow(stmtCreateThree.GetEnumerator(), fields, new object[][] { new object[] { "A1" }, new object[] { "A4" } });
            EventType eventTypeThree = stmtCreateThree.First().EventType;
            Assert.IsFalse(listeners[3].IsInvoked);
            Assert.AreEqual(2, GetCount("MyWindowThree"));
    
            // create window with last-per-id
            string stmtTextCreateFour = "create window MyWindowFour.std:unique(IntPrimitive) as MyWindow insert";
            EPStatement stmtCreateFour = epService.EPAdministrator.CreateEPL(stmtTextCreateFour);
            stmtCreateFour.AddListener(listeners[4]);
            EPAssertionUtil.AssertPropsPerRow(stmtCreateFour.GetEnumerator(), fields, new object[][] { new object[] { "C3" }, new object[] { "C5" } });
            EventType eventTypeFour = stmtCreateFour.First().EventType;
            Assert.IsFalse(listeners[4].IsInvoked);
            Assert.AreEqual(2, GetCount("MyWindowFour"));
    
            epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean(TheString like 'A%')");
            epService.EPAdministrator.CreateEPL("insert into MyWindowTwo select * from SupportBean(TheString like 'B%')");
            epService.EPAdministrator.CreateEPL("insert into MyWindowThree select * from SupportBean(TheString like 'C%')");
            epService.EPAdministrator.CreateEPL("insert into MyWindowFour select * from SupportBean(TheString like 'D%')");
            Assert.IsFalse(listeners[0].IsInvoked || listeners[2].IsInvoked || listeners[3].IsInvoked || listeners[4].IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("B9", -9));
            EventBean received = listeners[2].AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(received, fields, new object[]{"B9"});
            Assert.AreSame(eventTypeTwo, received.EventType);
            Assert.IsFalse(listeners[0].IsInvoked || listeners[3].IsInvoked || listeners[4].IsInvoked);
            Assert.AreEqual(6, GetCount("MyWindowTwo"));
    
            epService.EPRuntime.SendEvent(new SupportBean("A8", -8));
            received = listeners[0].AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(received, fields, new object[]{"A8"});
            Assert.AreSame(eventTypeOne, received.EventType);
            Assert.IsFalse(listeners[2].IsInvoked || listeners[3].IsInvoked || listeners[4].IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("C7", -7));
            received = listeners[3].AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(received, fields, new object[]{"C7"});
            Assert.AreSame(eventTypeThree, received.EventType);
            Assert.IsFalse(listeners[2].IsInvoked || listeners[0].IsInvoked || listeners[4].IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("D6", -6));
            received = listeners[4].AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(received, fields, new object[]{"D6"});
            Assert.AreSame(eventTypeFour, received.EventType);
            Assert.IsFalse(listeners[2].IsInvoked || listeners[0].IsInvoked || listeners[3].IsInvoked);
        }
    
        [Test]
        public void TestInsertWhereOMStaggered()
        {
            RunAssertionInsertWhereOMStaggered(EventRepresentationEnum.OBJECTARRAY);
            RunAssertionInsertWhereOMStaggered(EventRepresentationEnum.DEFAULT);
            RunAssertionInsertWhereOMStaggered(EventRepresentationEnum.MAP);
        }
    
        private void RunAssertionInsertWhereOMStaggered(EventRepresentationEnum eventRepresentationEnum)
        {
            IDictionary<String, object> dataType = MakeMap(new object[][] { new object[] { "a", typeof(string) }, new object[] { "b", typeof(int) } });
            epService.EPAdministrator.Configuration.AddEventType("MyMap", dataType);
    
            string stmtTextCreateOne = eventRepresentationEnum.GetAnnotationText() + " create window MyWindow.win:keepall() as select a, b from MyMap";
            EPStatement stmtCreateOne = epService.EPAdministrator.CreateEPL(stmtTextCreateOne);
            Assert.AreEqual(eventRepresentationEnum.GetOutputClass(), stmtCreateOne.EventType.UnderlyingType);
            stmtCreateOne.AddListener(listeners[0]);
    
            // create insert into
            string stmtTextInsertOne = "insert into MyWindow select a, b from MyMap";
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // populate some data
            epService.EPRuntime.SendEvent(MakeMap(new object[][] { new object[] { "a", "E1" }, new object[] { "b", 2 } }), "MyMap");
            epService.EPRuntime.SendEvent(MakeMap(new object[][] { new object[] { "a", "E2" }, new object[] { "b", 10 } }), "MyMap");
            epService.EPRuntime.SendEvent(MakeMap(new object[][] { new object[] { "a", "E3" }, new object[] { "b", 10 } }), "MyMap");
    
            // create window with keep-all using OM
            EPStatementObjectModel model = new EPStatementObjectModel();
            eventRepresentationEnum.AddAnnotation(model);
            Expression where = Expressions.Eq("b", 10);
            model.CreateWindow = CreateWindowClause.Create("MyWindowTwo", View.Create("win", "keepall")).SetInsert(true).SetInsertWhereClause(where);
            model.SelectClause = SelectClause.CreateWildcard();
            model.FromClause = FromClause.Create(FilterStream.Create("MyWindow"));
            string text = eventRepresentationEnum.GetAnnotationText() + " create window MyWindowTwo.win:keepall() as select * from MyWindow insert where b=10";
            Assert.AreEqual(text.Trim(), model.ToEPL());
    
            EPStatementObjectModel modelTwo = epService.EPAdministrator.CompileEPL(text);
            Assert.AreEqual(text.Trim(), modelTwo.ToEPL());
            
            EPStatement stmt = epService.EPAdministrator.Create(modelTwo);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), "a,b".Split(','), new object[][] { new object[] { "E2", 10 }, new object[] { "E3", 10 } });
    
            // test select individual fields and from an insert-from named window
            stmt = epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create window MyWindowThree.win:keepall() as select a from MyWindowTwo insert where a = 'E2'");
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), "a".Split(','), new object[][] { new object[] { "E2" } });
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyWindow", true);
            epService.EPAdministrator.Configuration.RemoveEventType("MyWindowTwo", true);
            epService.EPAdministrator.Configuration.RemoveEventType("MyWindowThree", true);
        }
    
        [Test]
        public void TestVariantStream()
        {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean_A>();
            epService.EPAdministrator.Configuration.AddEventType<SupportBean_B>();
    
            ConfigurationVariantStream config = new ConfigurationVariantStream();
            //config.setTypeVariance(ConfigurationVariantStream.TypeVariance.ANY);
            config.AddEventTypeName("SupportBean_A");
            config.AddEventTypeName("SupportBean_B");
            epService.EPAdministrator.Configuration.AddVariantStream("VarStream", config);
            epService.EPAdministrator.CreateEPL("create window MyWindow.win:keepall() as select * from VarStream");
            EPStatement stmt = epService.EPAdministrator.CreateEPL("create window MyWindowTwo.win:keepall() as MyWindow");
    
            epService.EPAdministrator.CreateEPL("insert into VarStream select * from SupportBean_A");
            epService.EPAdministrator.CreateEPL("insert into VarStream select * from SupportBean_B");
            epService.EPAdministrator.CreateEPL("insert into MyWindowTwo select * from VarStream");
            epService.EPRuntime.SendEvent(new SupportBean_A("A1"));
            epService.EPRuntime.SendEvent(new SupportBean_B("B1"));
            EventBean[] events = EPAssertionUtil.EnumeratorToArray(stmt.GetEnumerator());
            Assert.AreEqual("A1", events[0].Get("id?"));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), "id?".Split(','), new object[][] { new object[] { "A1" }, new object[] { "B1" } });
        }
    
        [Test]
        public void TestInvalid()
        {
            string stmtTextCreateOne = "create window MyWindow.win:keepall() as SupportBean";
            epService.EPAdministrator.CreateEPL(stmtTextCreateOne);
    
            try
            {
                epService.EPAdministrator.CreateEPL("create window testWindow3.win:keepall() as SupportBean insert");
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual("A named window by name 'SupportBean' could not be located, use the insert-keyword with an existing named window [create window testWindow3.win:keepall() as SupportBean insert]", ex.Message);
            }
    
            try
            {
                epService.EPAdministrator.CreateEPL("create window testWindow3.win:keepall() as select * from " + typeof(SupportBean).FullName + " insert where (IntPrimitive = 10)");
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual("A named window by name 'com.espertech.esper.support.bean.SupportBean' could not be located, use the insert-keyword with an existing named window [create window testWindow3.win:keepall() as select * from com.espertech.esper.support.bean.SupportBean insert where (IntPrimitive = 10)]", ex.Message);
            }
    
            try
            {
                epService.EPAdministrator.CreateEPL("create window MyWindowTwo.win:keepall() as MyWindow insert where (select IntPrimitive from SupportBean.std:lastevent())");
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual("Create window where-clause may not have a subselect [create window MyWindowTwo.win:keepall() as MyWindow insert where (select IntPrimitive from SupportBean.std:lastevent())]", ex.Message);
            }
    
            try
            {
                epService.EPAdministrator.CreateEPL("create window MyWindowTwo.win:keepall() as MyWindow insert where sum(IntPrimitive) > 2");
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual("Create window where-clause may not have an aggregation function [create window MyWindowTwo.win:keepall() as MyWindow insert where sum(IntPrimitive) > 2]", ex.Message);
            }
    
            try
            {
                epService.EPAdministrator.CreateEPL("create window MyWindowTwo.win:keepall() as MyWindow insert where prev(1, IntPrimitive) = 1");
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual("Create window where-clause may not have a function that requires view resources (prior, prev) [create window MyWindowTwo.win:keepall() as MyWindow insert where prev(1, IntPrimitive) = 1]", ex.Message);
            }
        }
    
        private IDictionary<String, object> MakeMap(object[][] entries)
        {
            var result = new Dictionary<string, object>();
            if (entries == null)
            {
                return result;
            }
            for (int i = 0; i < entries.Length; i++)
            {
                result.Put((string) entries[i][0], entries[i][1]);
            }
            return result;
        }
    
        private long GetCount(string windowName) 
        {
            NamedWindowProcessor processor = ((EPServiceProviderSPI)epService).NamedWindowMgmtService.GetProcessor(windowName);
            return processor.GetProcessorInstance(null).CountDataWindow;
        }    
    
        private string GetStatementName(string windowName) 
        {
            NamedWindowProcessor processor = ((EPServiceProviderSPI)epService).NamedWindowMgmtService.GetProcessor(windowName);
            return processor.StatementName;
        }
    
        private string GetEPL(string windowName) 
        {
            NamedWindowProcessor processor = ((EPServiceProviderSPI)epService).NamedWindowMgmtService.GetProcessor(windowName);
            return processor.EplExpression;
        }
    }
}
