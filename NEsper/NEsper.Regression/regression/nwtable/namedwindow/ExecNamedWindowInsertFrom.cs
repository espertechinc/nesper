///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.named;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.namedwindow
{
    public class ExecNamedWindowInsertFrom : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            var listeners = new SupportUpdateListener[10];
            for (int i = 0; i < listeners.Length; i++) {
                listeners[i] = new SupportUpdateListener();
            }
    
            RunAssertionCreateNamedAfterNamed(epService, listeners);
            RunAssertionInsertWhereTypeAndFilter(epService, listeners);
            RunAssertionInsertWhereOMStaggered(epService);
            RunAssertionVariantStream(epService);
            RunAssertionInvalid(epService);
        }
    
        private void RunAssertionCreateNamedAfterNamed(EPServiceProvider epService, SupportUpdateListener[] listeners) {
            // create window
            string stmtTextCreateOne = "create window MyWindow#keepall as SupportBean";
            EPStatement stmtCreateOne = epService.EPAdministrator.CreateEPL(stmtTextCreateOne);
            stmtCreateOne.Events += listeners[0].Update;
    
            // create window
            string stmtTextCreateTwo = "create window MyWindowTwo#keepall as MyWindow";
            EPStatement stmtCreateTwo = epService.EPAdministrator.CreateEPL(stmtTextCreateTwo);
            stmtCreateTwo.Events += listeners[1].Update;
    
            // create insert into
            string stmtTextInsertOne = "insert into MyWindow select * from SupportBean";
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // create consumer
            string stmtTextSelectOne = "select TheString from MyWindow";
            EPStatement stmtSelectOne = epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            stmtSelectOne.Events += listeners[2].Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            var fields = new[]{"TheString"};
            EPAssertionUtil.AssertProps(listeners[0].AssertOneGetNewAndReset(), fields, new object[]{"E1"});
            EPAssertionUtil.AssertProps(listeners[2].AssertOneGetNewAndReset(), fields, new object[]{"E1"});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionInsertWhereTypeAndFilter(EPServiceProvider epService, SupportUpdateListener[] listeners) {
            var fields = new[]{"TheString"};
    
            // create window
            string stmtTextCreateOne = "create window MyWindowIWT#keepall as SupportBean";
            EPStatement stmtCreateOne = epService.EPAdministrator.CreateEPL(stmtTextCreateOne, "name1");
            stmtCreateOne.Events += listeners[0].Update;
            EventType eventTypeOne = stmtCreateOne.EventType;
    
            // create insert into
            string stmtTextInsertOne = "insert into MyWindowIWT select * from SupportBean(IntPrimitive > 0)";
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // populate some data
            Assert.AreEqual(0, GetCount(epService, "MyWindowIWT"));
            epService.EPRuntime.SendEvent(new SupportBean("A1", 1));
            Assert.AreEqual(1, GetCount(epService, "MyWindowIWT"));
            epService.EPRuntime.SendEvent(new SupportBean("B2", 1));
            epService.EPRuntime.SendEvent(new SupportBean("C3", 1));
            epService.EPRuntime.SendEvent(new SupportBean("A4", 4));
            epService.EPRuntime.SendEvent(new SupportBean("C5", 4));
            Assert.AreEqual(5, GetCount(epService, "MyWindowIWT"));
            Assert.AreEqual("name1", GetStatementName(epService, "MyWindowIWT"));
            Assert.AreEqual(stmtTextCreateOne, GetEPL(epService, "MyWindowIWT"));
            listeners[0].Reset();
    
            // create window with keep-all
            string stmtTextCreateTwo = "create window MyWindowTwo#keepall as MyWindowIWT insert";
            EPStatement stmtCreateTwo = epService.EPAdministrator.CreateEPL(stmtTextCreateTwo);
            stmtCreateTwo.Events += listeners[2].Update;
            EPAssertionUtil.AssertPropsPerRow(stmtCreateTwo.GetEnumerator(), fields, new[] {new object[] {"A1"}, new object[] {"B2"}, new object[] {"C3"}, new object[] {"A4"}, new object[] {"C5"}});
            EventType eventTypeTwo = stmtCreateTwo.First().EventType;
            Assert.IsFalse(listeners[2].IsInvoked);
            Assert.AreEqual(5, GetCount(epService, "MyWindowTwo"));
            Assert.AreEqual(StatementType.CREATE_WINDOW, ((EPStatementSPI) stmtCreateTwo).StatementMetadata.StatementType);
    
            // create window with keep-all and filter
            string stmtTextCreateThree = "create window MyWindowThree#keepall as MyWindowIWT insert where TheString like 'A%'";
            EPStatement stmtCreateThree = epService.EPAdministrator.CreateEPL(stmtTextCreateThree);
            stmtCreateThree.Events += listeners[3].Update;
            EPAssertionUtil.AssertPropsPerRow(stmtCreateThree.GetEnumerator(), fields, new[] {new object[] {"A1"}, new object[] {"A4"}});
            EventType eventTypeThree = stmtCreateThree.First().EventType;
            Assert.IsFalse(listeners[3].IsInvoked);
            Assert.AreEqual(2, GetCount(epService, "MyWindowThree"));
    
            // create window with last-per-id
            string stmtTextCreateFour = "create window MyWindowFour#unique(IntPrimitive) as MyWindowIWT insert";
            EPStatement stmtCreateFour = epService.EPAdministrator.CreateEPL(stmtTextCreateFour);
            stmtCreateFour.Events += listeners[4].Update;
            EPAssertionUtil.AssertPropsPerRow(stmtCreateFour.GetEnumerator(), fields, new[] {new object[] {"C3"}, new object[] {"C5"}});
            EventType eventTypeFour = stmtCreateFour.First().EventType;
            Assert.IsFalse(listeners[4].IsInvoked);
            Assert.AreEqual(2, GetCount(epService, "MyWindowFour"));
    
            epService.EPAdministrator.CreateEPL("insert into MyWindowIWT select * from SupportBean(TheString like 'A%')");
            epService.EPAdministrator.CreateEPL("insert into MyWindowTwo select * from SupportBean(TheString like 'B%')");
            epService.EPAdministrator.CreateEPL("insert into MyWindowThree select * from SupportBean(TheString like 'C%')");
            epService.EPAdministrator.CreateEPL("insert into MyWindowFour select * from SupportBean(TheString like 'D%')");
            Assert.IsFalse(listeners[0].IsInvoked || listeners[2].IsInvoked || listeners[3].IsInvoked || listeners[4].IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("B9", -9));
            EventBean received = listeners[2].AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(received, fields, new object[]{"B9"});
            Assert.AreSame(eventTypeTwo, received.EventType);
            Assert.IsFalse(listeners[0].IsInvoked || listeners[3].IsInvoked || listeners[4].IsInvoked);
            Assert.AreEqual(6, GetCount(epService, "MyWindowTwo"));
    
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
    
        private void RunAssertionInsertWhereOMStaggered(EPServiceProvider epService) {
            foreach (EventRepresentationChoice rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                TryAssertionInsertWhereOMStaggered(epService, rep);
            }
        }
    
        private void TryAssertionInsertWhereOMStaggered(EPServiceProvider epService, EventRepresentationChoice eventRepresentationEnum) {
    
            IDictionary<string, object> dataType = MakeMap(new[] {new object[] {"a", typeof(string)}, new object[] {"b", typeof(int)}});
            epService.EPAdministrator.Configuration.AddEventType("MyMap", dataType);
    
            string stmtTextCreateOne = eventRepresentationEnum.GetAnnotationText() + " create window MyWindowIWOM#keepall as select a, b from MyMap";
            EPStatement stmtCreateOne = epService.EPAdministrator.CreateEPL(stmtTextCreateOne);
            Assert.IsTrue(eventRepresentationEnum.MatchesClass(stmtCreateOne.EventType.UnderlyingType));
            var listener = new SupportUpdateListener();
            stmtCreateOne.Events += listener.Update;
    
            // create insert into
            string stmtTextInsertOne = "insert into MyWindowIWOM select a, b from MyMap";
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // populate some data
            epService.EPRuntime.SendEvent(MakeMap(new[] {new object[] {"a", "E1"}, new object[] {"b", 2}}), "MyMap");
            epService.EPRuntime.SendEvent(MakeMap(new[] {new object[] {"a", "E2"}, new object[] {"b", 10}}), "MyMap");
            epService.EPRuntime.SendEvent(MakeMap(new[] {new object[] {"a", "E3"}, new object[] {"b", 10}}), "MyMap");
    
            // create window with keep-all using OM
            var model = new EPStatementObjectModel();
            eventRepresentationEnum.AddAnnotationForNonMap(model);
            Expression where = Expressions.Eq("b", 10);
            model.CreateWindow = CreateWindowClause.Create("MyWindowIWOMTwo", View.Create("keepall"))
                .SetIsInsert(true)
                .SetInsertWhereClause(where);
            model.SelectClause = SelectClause.CreateWildcard();
            model.FromClause = FromClause.Create(FilterStream.Create("MyWindowIWOM"));
            string text = eventRepresentationEnum.GetAnnotationTextForNonMap() + " create window MyWindowIWOMTwo#keepall as select * from MyWindowIWOM insert where b=10";
            Assert.AreEqual(text.Trim(), model.ToEPL());
    
            EPStatementObjectModel modelTwo = epService.EPAdministrator.CompileEPL(text);
            Assert.AreEqual(text.Trim(), modelTwo.ToEPL());
    
            EPStatement stmt = epService.EPAdministrator.Create(modelTwo);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), "a,b".Split(','), new[] {new object[] {"E2", 10}, new object[] {"E3", 10}});
    
            // test select individual fields and from an insert-from named window
            stmt = epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create window MyWindowIWOMThree#keepall as select a from MyWindowIWOMTwo insert where a = 'E2'");
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), "a".Split(','), new[] {new object[] {"E2"}});
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyWindowIWOM", true);
            epService.EPAdministrator.Configuration.RemoveEventType("MyWindowIWOMTwo", true);
            epService.EPAdministrator.Configuration.RemoveEventType("MyWindowIWOMThree", true);
        }
    
        private void RunAssertionVariantStream(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_A", typeof(SupportBean_A));
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_B", typeof(SupportBean_B));
    
            var config = new ConfigurationVariantStream();
            //config.TypeVariance = TypeVarianceEnum.ANY;
            config.AddEventTypeName("SupportBean_A");
            config.AddEventTypeName("SupportBean_B");
            epService.EPAdministrator.Configuration.AddVariantStream("VarStream", config);
            epService.EPAdministrator.CreateEPL("create window MyWindowVS#keepall as select * from VarStream");
            EPStatement stmt = epService.EPAdministrator.CreateEPL("create window MyWindowVSTwo#keepall as MyWindowVS");
    
            epService.EPAdministrator.CreateEPL("insert into VarStream select * from SupportBean_A");
            epService.EPAdministrator.CreateEPL("insert into VarStream select * from SupportBean_B");
            epService.EPAdministrator.CreateEPL("insert into MyWindowVSTwo select * from VarStream");
            epService.EPRuntime.SendEvent(new SupportBean_A("A1"));
            epService.EPRuntime.SendEvent(new SupportBean_B("B1"));
            EventBean[] events = EPAssertionUtil.EnumeratorToArray(stmt.GetEnumerator());
            Assert.AreEqual("A1", events[0].Get("id?"));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), "id?".Split(','), new[] {new object[] {"A1"}, new object[] {"B1"}});
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            string stmtTextCreateOne = "create window MyWindowINV#keepall as SupportBean";
            epService.EPAdministrator.CreateEPL(stmtTextCreateOne);
    
            TryInvalid(epService, "create window testWindow3#keepall as SupportBean insert",
                    "A named window by name 'SupportBean' could not be located, use the insert-keyword with an existing named window [create window testWindow3#keepall as SupportBean insert]");
            TryInvalid(epService, "create window testWindow3#keepall as select * from " + typeof(SupportBean).FullName + " insert where (IntPrimitive = 10)",
                    "A named window by name '" + typeof(SupportBean).FullName + "' could not be located, use the insert-keyword with an existing named window [");
            TryInvalid(epService, "create window MyWindowTwo#keepall as MyWindowINV insert where (select IntPrimitive from SupportBean#lastevent)",
                    "Create window where-clause may not have a subselect [create window MyWindowTwo#keepall as MyWindowINV insert where (select IntPrimitive from SupportBean#lastevent)]");
            TryInvalid(epService, "create window MyWindowTwo#keepall as MyWindowINV insert where sum(IntPrimitive) > 2",
                    "Create window where-clause may not have an aggregation function [create window MyWindowTwo#keepall as MyWindowINV insert where sum(IntPrimitive) > 2]");
            TryInvalid(epService, "create window MyWindowTwo#keepall as MyWindowINV insert where prev(1, IntPrimitive) = 1",
                    "Create window where-clause may not have a function that requires view resources (prior, prev) [create window MyWindowTwo#keepall as MyWindowINV insert where prev(1, IntPrimitive) = 1]");
        }
    
        private IDictionary<string, object> MakeMap(object[][] entries) {
            var result = new Dictionary<string, object>();
            if (entries == null) {
                return result;
            }
            for (int i = 0; i < entries.Length; i++) {
                result.Put((string) entries[i][0], entries[i][1]);
            }
            return result;
        }
    
        private long GetCount(EPServiceProvider epService, string windowName) {
            NamedWindowProcessor processor = ((EPServiceProviderSPI) epService).NamedWindowMgmtService.GetProcessor(windowName);
            return processor.GetProcessorInstance(null).CountDataWindow;
        }
    
        private string GetStatementName(EPServiceProvider epService, string windowName) {
            NamedWindowProcessor processor = ((EPServiceProviderSPI) epService).NamedWindowMgmtService.GetProcessor(windowName);
            return processor.StatementName;
        }
    
        private string GetEPL(EPServiceProvider epService, string windowName) {
            NamedWindowProcessor processor = ((EPServiceProviderSPI) epService).NamedWindowMgmtService.GetProcessor(windowName);
            return processor.EplExpression;
        }
    }
} // end of namespace
