///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;


using NUnit.Framework;

namespace com.espertech.esper.regression.expr.expr
{
    public class ExecExprInstanceOf : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionInstanceofSimple(epService);
            RunAssertionInstanceofStringAndNull_OM(epService);
            RunAssertionInstanceofStringAndNull_Compile(epService);
            RunAssertionDynamicPropertyTypes(epService);
            RunAssertionDynamicSuperTypeAndInterface(epService);
        }
    
        private void RunAssertionInstanceofSimple(EPServiceProvider epService) {
            string stmtText = "select instanceof(TheString, string) as t0, " +
                    " instanceof(IntBoxed, int) as t1, " +
                    " instanceof(FloatBoxed, System.Single) as t2, " +
                    " instanceof(TheString, System.Single, char, byte) as t3, " +
                    " instanceof(IntPrimitive, System.Int32) as t4, " +
                    " instanceof(IntPrimitive, long) as t5, " +
                    " instanceof(IntPrimitive, long, long, System.Object) as t6, " +
                    " instanceof(FloatBoxed, long, float) as t7 " +
                    " from " + typeof(SupportBean).FullName;
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            for (int i = 0; i < 7; i++) {
                Assert.AreEqual(typeof(bool?), stmt.EventType.GetPropertyType("t" + i));
            }
    
            var bean = new SupportBean("abc", 100);
            bean.FloatBoxed = 100F;
            epService.EPRuntime.SendEvent(bean);
            AssertResults(listener.AssertOneGetNewAndReset(), new bool[]{true, false, true, false, true, false, true, true});
    
            bean = new SupportBean(null, 100);
            bean.FloatBoxed = null;
            epService.EPRuntime.SendEvent(bean);
            AssertResults(listener.AssertOneGetNewAndReset(), new bool[]{false, false, false, false, true, false, true, false});
    
            stmt.Dispose();
        }
    
        private void RunAssertionInstanceofStringAndNull_OM(EPServiceProvider epService) {
            string stmtText = "select instanceof(TheString,string) as t0, " +
                    "instanceof(TheString,float,string,int) as t1 " +
                    "from " + typeof(SupportBean).FullName;
    
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create()
                    .Add(Expressions.InstanceOf("TheString", "string"), "t0")
                    .Add(Expressions.InstanceOf(Expressions.Property("TheString"), "float", "string", "int"), "t1");
            model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportBean).FullName));
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
            Assert.AreEqual(stmtText, model.ToEPL());
    
            EPStatement stmt = epService.EPAdministrator.Create(model);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("abc", 100));
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            Assert.IsTrue((bool?) theEvent.Get("t0"));
            Assert.IsTrue((bool?) theEvent.Get("t1"));
    
            epService.EPRuntime.SendEvent(new SupportBean(null, 100));
            theEvent = listener.AssertOneGetNewAndReset();
            Assert.IsFalse((bool?) theEvent.Get("t0"));
            Assert.IsFalse((bool?) theEvent.Get("t1"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionInstanceofStringAndNull_Compile(EPServiceProvider epService) {
            string stmtText = "select instanceof(TheString,string) as t0, " +
                    "instanceof(TheString,float,string,int) as t1 " +
                    "from " + typeof(SupportBean).FullName;
    
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(stmtText);
            Assert.AreEqual(stmtText, model.ToEPL());
    
            EPStatement stmt = epService.EPAdministrator.Create(model);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("abc", 100));
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            Assert.IsTrue((bool?) theEvent.Get("t0"));
            Assert.IsTrue((bool?) theEvent.Get("t1"));
    
            epService.EPRuntime.SendEvent(new SupportBean(null, 100));
            theEvent = listener.AssertOneGetNewAndReset();
            Assert.IsFalse((bool?) theEvent.Get("t0"));
            Assert.IsFalse((bool?) theEvent.Get("t1"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionDynamicPropertyTypes(EPServiceProvider epService) {
            string stmtText = "select instanceof(item?, string) as t0, " +
                    " instanceof(item?, int) as t1, " +
                    " instanceof(item?, System.Single) as t2, " +
                    " instanceof(item?, System.Single, char, byte) as t3, " +
                    " instanceof(item?, System.Int32) as t4, " +
                    " instanceof(item?, long) as t5, " +
                    " instanceof(item?, long, System.ValueType) as t6, " +
                    " instanceof(item?, long, float) as t7 " +
                    " from " + typeof(SupportMarkerInterface).FullName;
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBeanDynRoot("abc"));
            AssertResults(listener.AssertOneGetNewAndReset(), new bool[]{true, false, false, false, false, false, false, false});
    
            epService.EPRuntime.SendEvent(new SupportBeanDynRoot(100f));
            AssertResults(listener.AssertOneGetNewAndReset(), new bool[]{false, false, true, true, false, false, true, true});
    
            epService.EPRuntime.SendEvent(new SupportBeanDynRoot(null));
            AssertResults(listener.AssertOneGetNewAndReset(), new bool[]{false, false, false, false, false, false, false, false});
    
            epService.EPRuntime.SendEvent(new SupportBeanDynRoot(10));
            AssertResults(listener.AssertOneGetNewAndReset(), new bool[]{false, true, false, false, true, false, true, false});
    
            epService.EPRuntime.SendEvent(new SupportBeanDynRoot(99L));
            AssertResults(listener.AssertOneGetNewAndReset(), new bool[]{false, false, false, false, false, true, true, true});
    
            stmt.Dispose();
        }
    
        private void RunAssertionDynamicSuperTypeAndInterface(EPServiceProvider epService) {
            string stmtText = "select instanceof(item?, " + typeof(SupportMarkerInterface).FullName + ") as t0, " +
                    " instanceof(item?, " + typeof(ISupportA).FullName + ") as t1, " +
                    " instanceof(item?, " + typeof(ISupportBaseAB).FullName + ") as t2, " +
                    " instanceof(item?, " + typeof(ISupportBaseABImpl).FullName + ") as t3, " +
                    " instanceof(item?, " + typeof(ISupportA).FullName + ", " + typeof(ISupportB).FullName + ") as t4, " +
                    " instanceof(item?, " + typeof(ISupportBaseAB).FullName + ", " + typeof(ISupportB).FullName + ") as t5, " +
                    " instanceof(item?, " + typeof(ISupportAImplSuperG).FullName + ", " + typeof(ISupportB).FullName + ") as t6, " +
                    " instanceof(item?, " + typeof(ISupportAImplSuperGImplPlus).FullName + ", " + typeof(SupportBeanBase).FullName + ") as t7 " +
    
                    " from " + typeof(SupportMarkerInterface).FullName;
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBeanDynRoot(new SupportBeanDynRoot("abc")));
            AssertResults(listener.AssertOneGetNewAndReset(), new bool[]{true, false, false, false, false, false, false, false});
    
            epService.EPRuntime.SendEvent(new SupportBeanDynRoot(new ISupportAImplSuperGImplPlus()));
            AssertResults(listener.AssertOneGetNewAndReset(), new bool[]{false, true, true, false, true, true, true, true});
    
            epService.EPRuntime.SendEvent(new SupportBeanDynRoot(new ISupportAImplSuperGImpl("", "", "")));
            AssertResults(listener.AssertOneGetNewAndReset(), new bool[]{false, true, true, false, true, true, true, false});
    
            epService.EPRuntime.SendEvent(new SupportBeanDynRoot(new ISupportBaseABImpl("")));
            AssertResults(listener.AssertOneGetNewAndReset(), new bool[]{false, false, true, true, false, true, false, false});
    
            epService.EPRuntime.SendEvent(new SupportBeanDynRoot(new ISupportBImpl("", "")));
            AssertResults(listener.AssertOneGetNewAndReset(), new bool[]{false, false, true, false, true, true, true, false});
    
            epService.EPRuntime.SendEvent(new SupportBeanDynRoot(new ISupportAImpl("", "")));
            AssertResults(listener.AssertOneGetNewAndReset(), new bool[]{false, true, true, false, true, true, false, false});
    
            stmt.Dispose();
        }
    
        private void AssertResults(EventBean theEvent, bool[] result) {
            for (int i = 0; i < result.Length; i++) {
                Assert.AreEqual(result[i], theEvent.Get("t" + i), "failed for index " + i);
            }
        }
    }
} // end of namespace
