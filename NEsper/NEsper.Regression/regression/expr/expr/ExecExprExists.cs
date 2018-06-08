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
    public class ExecExprExists : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionExistsSimple(epService);
            RunAssertionExistsInner(epService);
            RunAssertionCastDoubleAndNull_OM(epService);
            RunAssertionCastStringAndNull_Compile(epService);
        }
    
        private void RunAssertionExistsSimple(EPServiceProvider epService) {
            string stmtText = "select exists(TheString) as t0, " +
                    " exists(IntBoxed?) as t1, " +
                    " exists(dummy?) as t2, " +
                    " exists(IntPrimitive?) as t3, " +
                    " exists(IntPrimitive) as t4 " +
                    " from " + typeof(SupportBean).FullName;
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            for (int i = 0; i < 5; i++) {
                Assert.AreEqual(typeof(bool?), stmt.EventType.GetPropertyType("t" + i));
            }
    
            var bean = new SupportBean("abc", 100);
            bean.FloatBoxed = 9.5f;
            bean.IntBoxed = 3;
            epService.EPRuntime.SendEvent(bean);
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            AssertResults(theEvent, new bool[]{true, true, false, true, true});
    
            stmt.Dispose();
        }
    
        private void RunAssertionExistsInner(EPServiceProvider epService) {
            string stmtText = "select exists(item?.id) as t0, " +
                    " exists(item?.id?) as t1, " +
                    " exists(item?.item.IntBoxed) as t2, " +
                    " exists(item?.indexed[0]?) as t3, " +
                    " exists(item?.Mapped('keyOne')?) as t4, " +
                    " exists(item?.nested?) as t5, " +
                    " exists(item?.nested.nestedValue?) as t6, " +
                    " exists(item?.nested.nestedNested?) as t7, " +
                    " exists(item?.nested.nestedNested.nestedNestedValue?) as t8, " +
                    " exists(item?.nested.nestedNested.nestedNestedValue.dummy?) as t9, " +
                    " exists(item?.nested.nestedNested.dummy?) as t10 " +
                    " from " + typeof(SupportMarkerInterface).FullName;
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            for (int i = 0; i < 11; i++) {
                Assert.AreEqual(typeof(bool?), stmt.EventType.GetPropertyType("t" + i));
            }
    
            // cannot Exists if the inner is null
            epService.EPRuntime.SendEvent(new SupportBeanDynRoot(null));
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            AssertResults(theEvent, new bool[]{false, false, false, false, false, false, false, false, false, false, false});
    
            // try nested, indexed and mapped
            epService.EPRuntime.SendEvent(new SupportBeanDynRoot(SupportBeanComplexProps.MakeDefaultBean()));
            theEvent = listener.AssertOneGetNewAndReset();
            AssertResults(theEvent, new bool[]{false, false, false, true, true, true, true, true, true, false, false});
    
            // try nested, indexed and mapped
            epService.EPRuntime.SendEvent(new SupportBeanDynRoot(SupportBeanComplexProps.MakeDefaultBean()));
            theEvent = listener.AssertOneGetNewAndReset();
            AssertResults(theEvent, new bool[]{false, false, false, true, true, true, true, true, true, false, false});
    
            // try a boxed that returns null but does Exists
            epService.EPRuntime.SendEvent(new SupportBeanDynRoot(new SupportBeanDynRoot(new SupportBean())));
            theEvent = listener.AssertOneGetNewAndReset();
            AssertResults(theEvent, new bool[]{false, false, true, false, false, false, false, false, false, false, false});
    
            epService.EPRuntime.SendEvent(new SupportBeanDynRoot(new SupportBean_A("10")));
            theEvent = listener.AssertOneGetNewAndReset();
            AssertResults(theEvent, new bool[]{true, true, false, false, false, false, false, false, false, false, false});
    
            stmt.Dispose();
        }
    
        private void RunAssertionCastDoubleAndNull_OM(EPServiceProvider epService) {
            string stmtText = "select exists(item?.IntBoxed) as t0 " +
                    "from " + typeof(SupportMarkerInterface).FullName;
    
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create().Add(Expressions.ExistsProperty("item?.IntBoxed"), "t0");
            model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportMarkerInterface).FullName));
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
            Assert.AreEqual(stmtText, model.ToEPL());
    
            EPStatement stmt = epService.EPAdministrator.Create(model);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            Assert.AreEqual(typeof(bool?), stmt.EventType.GetPropertyType("t0"));
    
            epService.EPRuntime.SendEvent(new SupportBeanDynRoot(new SupportBean()));
            Assert.AreEqual(true, listener.AssertOneGetNewAndReset().Get("t0"));
    
            epService.EPRuntime.SendEvent(new SupportBeanDynRoot(null));
            Assert.AreEqual(false, listener.AssertOneGetNewAndReset().Get("t0"));
    
            epService.EPRuntime.SendEvent(new SupportBeanDynRoot("abc"));
            Assert.AreEqual(false, listener.AssertOneGetNewAndReset().Get("t0"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionCastStringAndNull_Compile(EPServiceProvider epService) {
            string stmtText = "select exists(item?.IntBoxed) as t0 " +
                    "from " + typeof(SupportMarkerInterface).FullName;
    
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(stmtText);
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
            Assert.AreEqual(stmtText, model.ToEPL());
    
            EPStatement stmt = epService.EPAdministrator.Create(model);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            Assert.AreEqual(typeof(bool?), stmt.EventType.GetPropertyType("t0"));
    
            epService.EPRuntime.SendEvent(new SupportBeanDynRoot(new SupportBean()));
            Assert.AreEqual(true, listener.AssertOneGetNewAndReset().Get("t0"));
    
            epService.EPRuntime.SendEvent(new SupportBeanDynRoot(null));
            Assert.AreEqual(false, listener.AssertOneGetNewAndReset().Get("t0"));
    
            epService.EPRuntime.SendEvent(new SupportBeanDynRoot("abc"));
            Assert.AreEqual(false, listener.AssertOneGetNewAndReset().Get("t0"));
    
            stmt.Dispose();
        }
    
        private void AssertResults(EventBean theEvent, bool[] result) {
            for (int i = 0; i < result.Length; i++) {
                Assert.AreEqual(result[i], theEvent.Get("t" + i), "failed for index " + i);
            }
        }
    }
} // end of namespace
