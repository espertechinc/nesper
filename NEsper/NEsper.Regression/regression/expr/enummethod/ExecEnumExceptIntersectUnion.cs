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
using com.espertech.esper.client.deploy;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.bean.lambda;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;
// using static org.junit.Assert.assertEquals;

using NUnit.Framework;

namespace com.espertech.esper.regression.expr.enummethod
{
    public class ExecEnumExceptIntersectUnion : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("SupportBean_ST0_Container", typeof(SupportBean_ST0_Container));
            configuration.AddEventType("SupportBean_ST0", typeof(SupportBean_ST0));
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType("SupportCollection", typeof(SupportCollection));
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionStringArrayIntersection(epService);
            RunAssertionSetLogicWithContained(epService);
            RunAssertionSetLogicWithEvents(epService);
            RunAssertionSetLogicWithScalar(epService);
            RunAssertionUnionWhere(epService);
            TryAssertionInheritance(epService);
            RunAssertionInvalid(epService);
        }
    
        private void RunAssertionStringArrayIntersection(EPServiceProvider epService) {
            string epl = "create objectarray schema Event(meta1 string[], meta2 string[]);\n" +
                    "@Name('Out') select * from Event(meta1.Intersect(meta2).CountOf() > 0);\n";
            DeploymentResult result = epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.GetStatement("Out").AddListener(listener);
    
            SendAndAssert(epService, listener, "a,b", "a,b", true);
            SendAndAssert(epService, listener, "c,d", "a,b", false);
            SendAndAssert(epService, listener, "c,d", "a,d", true);
            SendAndAssert(epService, listener, "a,d,a,a", "b,c", false);
            SendAndAssert(epService, listener, "a,d,a,a", "b,d", true);
    
            epService.EPAdministrator.DeploymentAdmin.UndeployRemove(result.DeploymentId);
        }
    
        private void RunAssertionSetLogicWithContained(EPServiceProvider epService) {
            string epl = "select " +
                    "contained.Except(containedTwo) as val0," +
                    "contained.Intersect(containedTwo) as val1, " +
                    "contained.Union(containedTwo) as val2 " +
                    " from SupportBean_ST0_Container";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
            LambdaAssertionUtil.AssertTypes(stmt.EventType, "val0".Split(','), new Type[]{typeof(Collection)});
    
            List<SupportBean_ST0> first = SupportBean_ST0_Container.Make2ValueList("E1,1", "E2,10", "E3,1", "E4,10", "E5,11");
            List<SupportBean_ST0> second = SupportBean_ST0_Container.Make2ValueList("E1,1", "E3,1", "E4,10");
            epService.EPRuntime.SendEvent(new SupportBean_ST0_Container(first, second));
            LambdaAssertionUtil.AssertST0Id(listener, "val0", "E2,E5");
            LambdaAssertionUtil.AssertST0Id(listener, "val1", "E1,E3,E4");
            LambdaAssertionUtil.AssertST0Id(listener, "val2", "E1,E2,E3,E4,E5,E1,E3,E4");
            listener.Reset();
    
            stmt.Dispose();
        }
    
        private void RunAssertionSetLogicWithEvents(EPServiceProvider epService) {
    
            string epl =
                    "expression last10A {" +
                            " (select * from SupportBean_ST0(key0 like 'A%')#length(2)) " +
                            "}" +
                            "expression last10NonZero {" +
                            " (select * from SupportBean_ST0(p00 > 0)#length(2)) " +
                            "}" +
                            "select " +
                            "Last10A().Except(Last10NonZero()) as val0," +
                            "Last10A().Intersect(Last10NonZero()) as val1, " +
                            "Last10A().Union(Last10NonZero()) as val2 " +
                            "from SupportBean";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
            LambdaAssertionUtil.AssertTypes(stmt.EventType, "val0".Split(','), new Type[]{typeof(Collection)});
    
            epService.EPRuntime.SendEvent(new SupportBean_ST0("E1", "A1", 10));    // in both
            epService.EPRuntime.SendEvent(new SupportBean());
            LambdaAssertionUtil.AssertST0Id(listener, "val0", "");
            LambdaAssertionUtil.AssertST0Id(listener, "val1", "E1");
            LambdaAssertionUtil.AssertST0Id(listener, "val2", "E1,E1");
            listener.Reset();
    
            epService.EPRuntime.SendEvent(new SupportBean_ST0("E2", "A1", 0));
            epService.EPRuntime.SendEvent(new SupportBean());
            LambdaAssertionUtil.AssertST0Id(listener, "val0", "E2");
            LambdaAssertionUtil.AssertST0Id(listener, "val1", "E1");
            LambdaAssertionUtil.AssertST0Id(listener, "val2", "E1,E2,E1");
            listener.Reset();
    
            epService.EPRuntime.SendEvent(new SupportBean_ST0("E3", "B1", 0));
            epService.EPRuntime.SendEvent(new SupportBean());
            LambdaAssertionUtil.AssertST0Id(listener, "val0", "E2");
            LambdaAssertionUtil.AssertST0Id(listener, "val1", "E1");
            LambdaAssertionUtil.AssertST0Id(listener, "val2", "E1,E2,E1");
            listener.Reset();
    
            epService.EPRuntime.SendEvent(new SupportBean_ST0("E4", "A2", -1));
            epService.EPRuntime.SendEvent(new SupportBean());
            LambdaAssertionUtil.AssertST0Id(listener, "val0", "E2,E4");
            LambdaAssertionUtil.AssertST0Id(listener, "val1", "");
            LambdaAssertionUtil.AssertST0Id(listener, "val2", "E2,E4,E1");
            listener.Reset();
    
            epService.EPRuntime.SendEvent(new SupportBean_ST0("E5", "A3", -2));
            epService.EPRuntime.SendEvent(new SupportBean());
            LambdaAssertionUtil.AssertST0Id(listener, "val0", "E4,E5");
            LambdaAssertionUtil.AssertST0Id(listener, "val1", "");
            LambdaAssertionUtil.AssertST0Id(listener, "val2", "E4,E5,E1");
            listener.Reset();
    
            epService.EPRuntime.SendEvent(new SupportBean_ST0("E6", "A6", 11));    // in both
            epService.EPRuntime.SendEvent(new SupportBean_ST0("E7", "A7", 12));    // in both
            epService.EPRuntime.SendEvent(new SupportBean());
            LambdaAssertionUtil.AssertST0Id(listener, "val0", "");
            LambdaAssertionUtil.AssertST0Id(listener, "val1", "E6,E7");
            LambdaAssertionUtil.AssertST0Id(listener, "val2", "E6,E7,E6,E7");
            listener.Reset();
    
            stmt.Dispose();
        }
    
        private void RunAssertionSetLogicWithScalar(EPServiceProvider epService) {
            string epl = "select " +
                    "strvals.Except(strvalstwo) as val0," +
                    "strvals.Intersect(strvalstwo) as val1, " +
                    "strvals.Union(strvalstwo) as val2 " +
                    " from SupportCollection as bean";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
            LambdaAssertionUtil.AssertTypes(stmt.EventType, "val0".Split(','), new Type[]{typeof(Collection)});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1,E2", "E3,E4"));
            LambdaAssertionUtil.AssertValuesArrayScalar(listener, "val0", "E1", "E2");
            LambdaAssertionUtil.AssertValuesArrayScalar(listener, "val1");
            LambdaAssertionUtil.AssertValuesArrayScalar(listener, "val2", "E1", "E2", "E3", "E4");
            listener.Reset();
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString(null, "E3,E4"));
            LambdaAssertionUtil.AssertValuesArrayScalar(listener, "val0", (Object[]) null);
            LambdaAssertionUtil.AssertValuesArrayScalar(listener, "val1", (Object[]) null);
            LambdaAssertionUtil.AssertValuesArrayScalar(listener, "val2", (Object[]) null);
            listener.Reset();
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString("", "E3,E4"));
            LambdaAssertionUtil.AssertValuesArrayScalar(listener, "val0");
            LambdaAssertionUtil.AssertValuesArrayScalar(listener, "val1");
            LambdaAssertionUtil.AssertValuesArrayScalar(listener, "val2", "E3", "E4");
            listener.Reset();
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1,E3,E5", "E3,E4"));
            LambdaAssertionUtil.AssertValuesArrayScalar(listener, "val0", "E1", "E5");
            LambdaAssertionUtil.AssertValuesArrayScalar(listener, "val1", "E3");
            LambdaAssertionUtil.AssertValuesArrayScalar(listener, "val2", "E1", "E3", "E5", "E3", "E4");
            listener.Reset();
    
            stmt.Dispose();
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            string epl;
    
            epl = "select Contained.Union(true) from SupportBean_ST0_Container";
            TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'contained.Union(true)': Enumeration method 'union' requires an expression yielding an event-collection as input paramater [select Contained.Union(true) from SupportBean_ST0_Container]");
    
            epl = "select Contained.Union(Prevwindow(s1)) from SupportBean_ST0_Container#lastevent, SupportBean#keepall s1";
            TryInvalid(epService, epl, "Error starting statement: Failed to validate select-clause expression 'contained.Union(Prevwindow(s1))': Enumeration method 'union' expects event type 'SupportBean_ST0' but receives event type 'SupportBean' [select Contained.Union(Prevwindow(s1)) from SupportBean_ST0_Container#lastevent, SupportBean#keepall s1]");
        }
    
        private void RunAssertionUnionWhere(EPServiceProvider epService) {
    
            string epl = "expression one {" +
                    "  x => x.contained.Where(y => p00 = 10)" +
                    "} " +
                    "" +
                    "expression two {" +
                    "  x => x.contained.Where(y => p00 = 11)" +
                    "} " +
                    "" +
                    "select One(bean).Union(Two(bean)) as val0 from SupportBean_ST0_Container as bean";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
            LambdaAssertionUtil.AssertTypes(stmt.EventType, "val0".Split(','), new Type[]{typeof(Collection)});
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,1", "E2,10", "E3,1", "E4,10", "E5,11"));
            LambdaAssertionUtil.AssertST0Id(listener, "val0", "E2,E4,E5");
            listener.Reset();
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,10", "E2,1", "E3,1"));
            LambdaAssertionUtil.AssertST0Id(listener, "val0", "E1");
            listener.Reset();
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,1", "E2,1", "E3,10", "E4,11"));
            LambdaAssertionUtil.AssertST0Id(listener, "val0", "E3,E4");
            listener.Reset();
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value((string[]) null));
            LambdaAssertionUtil.AssertST0Id(listener, "val0", null);
            listener.Reset();
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value());
            LambdaAssertionUtil.AssertST0Id(listener, "val0", "");
            listener.Reset();
    
            stmt.Dispose();
        }
    
        private void TryAssertionInheritance(EPServiceProvider epService) {
            foreach (EventRepresentationChoice rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                if (rep.IsMapEvent() || rep.IsObjectArrayEvent()) {
                    TryAssertionInheritance(epService, rep);
                }
            }
        }
    
        private void TryAssertionInheritance(EPServiceProvider epService, EventRepresentationChoice eventRepresentationEnum) {
    
            epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema BaseEvent as (b1 string)");
            epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema SubEvent as (s1 string) inherits BaseEvent");
            epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema OuterEvent as (bases BaseEvent[], subs SubEvent[])");
            EPStatement stmt = epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " select Bases.Union(subs) as val from OuterEvent");
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                epService.EPRuntime.SendEvent(new Object[]{new Object[][]{new object[] {"b10"}}, new Object[][] {new object[] {"b10", "s10"}}}, "OuterEvent");
            } else {
                IDictionary<string, Object> baseEvent = MakeMap("b1", "b10");
                IDictionary<string, Object> subEvent = MakeMap("s1", "s10");
                IDictionary<string, Object> outerEvent = MakeMap("bases", new Map[]{baseEvent}, "subs", new Map[]{subEvent});
                epService.EPRuntime.SendEvent(outerEvent, "OuterEvent");
            }
    
            Collection result = (Collection) listener.AssertOneGetNewAndReset().Get("val");
            Assert.AreEqual(2, result.Count);
    
            epService.EPAdministrator.DestroyAllStatements();
            foreach (string name in "BaseEvent,SubEvent,OuterEvent".Split(',')) {
                epService.EPAdministrator.Configuration.RemoveEventType(name, true);
            }
        }
    
        private IDictionary<string, Object> MakeMap(string key, Object value) {
            var map = new LinkedHashMap<string, object>();
            map.Put(key, value);
            return map;
        }
    
        private IDictionary<string, Object> MakeMap(string key, Object value, string key2, Object value2) {
            IDictionary<string, Object> map = MakeMap(key, value);
            map.Put(key2, value2);
            return map;
        }
    
        private void SendAndAssert(EPServiceProvider epService, SupportUpdateListener listener, string metaOne, string metaTwo, bool expected) {
            epService.EPRuntime.SendEvent(new Object[]{metaOne.Split(','), metaTwo.Split(',')}, "Event");
            Assert.AreEqual(expected, listener.IsInvokedAndReset());
        }
    }
} // end of namespace
