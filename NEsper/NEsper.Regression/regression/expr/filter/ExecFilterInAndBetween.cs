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
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

// using static org.junit.Assert.*;

using NUnit.Framework;

namespace com.espertech.esper.regression.expr.filter
{
    public class ExecFilterInAndBetween : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionInDynamic(epService);
            RunAssertionSimpleIntAndEnumWrite(epService);
            RunAssertionInvalid(epService);
            RunAssertionInExpr(epService);
            RunAssertionNotInExpr(epService);
            RunAssertionReuse(epService);
            RunAssertionReuseNot(epService);
        }
    
        private void RunAssertionInDynamic(EPServiceProvider epService) {
            string expr = "select * from pattern [a=" + typeof(SupportBeanNumeric).Name + " -> every b=" + typeof(SupportBean).FullName
                    + "(intPrimitive in (a.intOne, a.intTwo))]";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(expr);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            SendBeanNumeric(epService, 10, 20);
            SendBeanInt(epService, 10);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            SendBeanInt(epService, 11);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
            SendBeanInt(epService, 20);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            stmt.Stop();
    
            expr = "select * from pattern [a=" + typeof(SupportBean_S0).Name + " -> every b=" + typeof(SupportBean).FullName
                    + "(theString in (a.p00, a.p01, a.p02))]";
            stmt = epService.EPAdministrator.CreateEPL(expr);
            stmt.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "a", "b", "c", "d"));
            SendBeanString(epService, "a");
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            SendBeanString(epService, "x");
            Assert.IsFalse(listener.GetAndClearIsInvoked());
            SendBeanString(epService, "b");
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            SendBeanString(epService, "c");
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            SendBeanString(epService, "d");
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            stmt.Dispose();
        }
    
        private void RunAssertionSimpleIntAndEnumWrite(EPServiceProvider epService) {
            string expr = "select * from " + typeof(SupportBean).FullName + "(intPrimitive in (1, 10))";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(expr);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            SendBeanInt(epService, 10);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            SendBeanInt(epService, 11);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
            SendBeanInt(epService, 1);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            stmt.Dispose();
    
            // try enum - ESPER-459
            var types = new HashSet<SupportEnum>();
            types.Add(SupportEnum.ENUM_VALUE_2);
            EPPreparedStatement inPstmt = epService.EPAdministrator.PrepareEPL("select * from " + typeof(SupportBean).FullName + " ev " + "where ev.enumValue in (?)");
            inPstmt.SetObject(1, types);
    
            EPStatement inStmt = epService.EPAdministrator.Create(inPstmt);
            inStmt.AddListener(listener);
    
            var theEvent = new SupportBean();
            theEvent.EnumValue = SupportEnum.ENUM_VALUE_2;
            epService.EPRuntime.SendEvent(theEvent);
    
            Assert.IsTrue(listener.IsInvoked);
    
            inStmt.Dispose();
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            // we do not coerce
            TryInvalid(epService, "select * from " + typeof(SupportBean).FullName + "(intPrimitive in (1L, 10L))");
            TryInvalid(epService, "select * from " + typeof(SupportBean).FullName + "(intPrimitive in (1, 10L))");
            TryInvalid(epService, "select * from " + typeof(SupportBean).FullName + "(intPrimitive in (1, 'x'))");
    
            string expr = "select * from pattern [a=" + typeof(SupportBean).FullName + " -> b=" + typeof(SupportBean).FullName
                    + "(intPrimitive in (a.longPrimitive, a.longBoxed))]";
            TryInvalid(epService, expr);
        }
    
        private void RunAssertionInExpr(EPServiceProvider epService) {
            TryExpr(epService, "(theString > 'b')", "theString", new string[]{"a", "b", "c", "d"}, new bool[]{false, false, true, true});
            TryExpr(epService, "(theString < 'b')", "theString", new string[]{"a", "b", "c", "d"}, new bool[]{true, false, false, false});
            TryExpr(epService, "(theString >= 'b')", "theString", new string[]{"a", "b", "c", "d"}, new bool[]{false, true, true, true});
            TryExpr(epService, "(theString <= 'b')", "theString", new string[]{"a", "b", "c", "d"}, new bool[]{true, true, false, false});
            TryExpr(epService, "(theString in ['b':'d'])", "theString", new string[]{"a", "b", "c", "d", "e"}, new bool[]{false, true, true, true, false});
            TryExpr(epService, "(theString in ('b':'d'])", "theString", new string[]{"a", "b", "c", "d", "e"}, new bool[]{false, false, true, true, false});
            TryExpr(epService, "(theString in ['b':'d'))", "theString", new string[]{"a", "b", "c", "d", "e"}, new bool[]{false, true, true, false, false});
            TryExpr(epService, "(theString in ('b':'d'))", "theString", new string[]{"a", "b", "c", "d", "e"}, new bool[]{false, false, true, false, false});
            TryExpr(epService, "(boolPrimitive in (false))", "boolPrimitive", new Object[]{true, false}, new bool[]{false, true});
            TryExpr(epService, "(boolPrimitive in (false, false, false))", "boolPrimitive", new Object[]{true, false}, new bool[]{false, true});
            TryExpr(epService, "(boolPrimitive in (false, true, false))", "boolPrimitive", new Object[]{true, false}, new bool[]{true, true});
            TryExpr(epService, "(intBoxed in (4, 6, 1))", "intBoxed", new Object[]{0, 1, 2, 3, 4, 5, 6}, new bool[]{false, true, false, false, true, false, true});
            TryExpr(epService, "(intBoxed in (3))", "intBoxed", new Object[]{0, 1, 2, 3, 4, 5, 6}, new bool[]{false, false, false, true, false, false, false});
            TryExpr(epService, "(longBoxed in (3))", "longBoxed", new Object[]{0L, 1L, 2L, 3L, 4L, 5L, 6L}, new bool[]{false, false, false, true, false, false, false});
            TryExpr(epService, "(intBoxed between 4 and 6)", "intBoxed", new Object[]{0, 1, 2, 3, 4, 5, 6}, new bool[]{false, false, false, false, true, true, true});
            TryExpr(epService, "(intBoxed between 2 and 1)", "intBoxed", new Object[]{0, 1, 2, 3, 4, 5, 6}, new bool[]{false, true, true, false, false, false, false});
            TryExpr(epService, "(intBoxed between 4 and -1)", "intBoxed", new Object[]{0, 1, 2, 3, 4, 5, 6}, new bool[]{true, true, true, true, true, false, false});
            TryExpr(epService, "(intBoxed in [2:4])", "intBoxed", new Object[]{0, 1, 2, 3, 4, 5, 6}, new bool[]{false, false, true, true, true, false, false});
            TryExpr(epService, "(intBoxed in (2:4])", "intBoxed", new Object[]{0, 1, 2, 3, 4, 5, 6}, new bool[]{false, false, false, true, true, false, false});
            TryExpr(epService, "(intBoxed in [2:4))", "intBoxed", new Object[]{0, 1, 2, 3, 4, 5, 6}, new bool[]{false, false, true, true, false, false, false});
            TryExpr(epService, "(intBoxed in (2:4))", "intBoxed", new Object[]{0, 1, 2, 3, 4, 5, 6}, new bool[]{false, false, false, true, false, false, false});
        }
    
        private void RunAssertionNotInExpr(EPServiceProvider epService) {
            TryExpr(epService, "(intBoxed not between 4 and 6)", "intBoxed", new Object[]{0, 1, 2, 3, 4, 5, 6}, new bool[]{true, true, true, true, false, false, false});
            TryExpr(epService, "(intBoxed not between 2 and 1)", "intBoxed", new Object[]{0, 1, 2, 3, 4, 5, 6}, new bool[]{true, false, false, true, true, true, true});
            TryExpr(epService, "(intBoxed not between 4 and -1)", "intBoxed", new Object[]{0, 1, 2, 3, 4, 5, 6}, new bool[]{false, false, false, false, false, true, true});
            TryExpr(epService, "(intBoxed not in [2:4])", "intBoxed", new Object[]{0, 1, 2, 3, 4, 5, 6}, new bool[]{true, true, false, false, false, true, true});
            TryExpr(epService, "(intBoxed not in (2:4])", "intBoxed", new Object[]{0, 1, 2, 3, 4, 5, 6}, new bool[]{true, true, true, false, false, true, true});
            TryExpr(epService, "(intBoxed not in [2:4))", "intBoxed", new Object[]{0, 1, 2, 3, 4, 5, 6}, new bool[]{true, true, false, false, true, true, true});
            TryExpr(epService, "(intBoxed not in (2:4))", "intBoxed", new Object[]{0, 1, 2, 3, 4, 5, 6}, new bool[]{true, true, true, false, true, true, true});
            TryExpr(epService, "(theString not in ['b':'d'])", "theString", new string[]{"a", "b", "c", "d", "e"}, new bool[]{true, false, false, false, true});
            TryExpr(epService, "(theString not in ('b':'d'])", "theString", new string[]{"a", "b", "c", "d", "e"}, new bool[]{true, true, false, false, true});
            TryExpr(epService, "(theString not in ['b':'d'))", "theString", new string[]{"a", "b", "c", "d", "e"}, new bool[]{true, false, false, true, true});
            TryExpr(epService, "(theString not in ('b':'d'))", "theString", new string[]{"a", "b", "c", "d", "e"}, new bool[]{true, true, false, true, true});
            TryExpr(epService, "(theString not in ('a', 'b'))", "theString", new string[]{"a", "x", "b", "y"}, new bool[]{false, true, false, true});
            TryExpr(epService, "(boolPrimitive not in (false))", "boolPrimitive", new Object[]{true, false}, new bool[]{true, false});
            TryExpr(epService, "(boolPrimitive not in (false, false, false))", "boolPrimitive", new Object[]{true, false}, new bool[]{true, false});
            TryExpr(epService, "(boolPrimitive not in (false, true, false))", "boolPrimitive", new Object[]{true, false}, new bool[]{false, false});
            TryExpr(epService, "(intBoxed not in (4, 6, 1))", "intBoxed", new Object[]{0, 1, 2, 3, 4, 5, 6}, new bool[]{true, false, true, true, false, true, false});
            TryExpr(epService, "(intBoxed not in (3))", "intBoxed", new Object[]{0, 1, 2, 3, 4, 5, 6}, new bool[]{true, true, true, false, true, true, true});
            TryExpr(epService, "(longBoxed not in (3))", "longBoxed", new Object[]{0L, 1L, 2L, 3L, 4L, 5L, 6L}, new bool[]{true, true, true, false, true, true, true});
        }
    
        private void RunAssertionReuse(EPServiceProvider epService) {
            string expr = "select * from " + typeof(SupportBean).FullName + "(intBoxed in [2:4])";
            TryReuse(epService, new string[]{expr, expr});
    
            expr = "select * from " + typeof(SupportBean).FullName + "(intBoxed in (1, 2, 3))";
            TryReuse(epService, new string[]{expr, expr});
    
            string exprOne = "select * from " + typeof(SupportBean).FullName + "(intBoxed in (2:3])";
            string exprTwo = "select * from " + typeof(SupportBean).FullName + "(intBoxed in (1:3])";
            TryReuse(epService, new string[]{exprOne, exprTwo});
    
            exprOne = "select * from " + typeof(SupportBean).FullName + "(intBoxed in (2, 3, 4))";
            exprTwo = "select * from " + typeof(SupportBean).FullName + "(intBoxed in (1, 3))";
            TryReuse(epService, new string[]{exprOne, exprTwo});
    
            exprOne = "select * from " + typeof(SupportBean).FullName + "(intBoxed in (2, 3, 4))";
            exprTwo = "select * from " + typeof(SupportBean).FullName + "(intBoxed in (1, 3))";
            string exprThree = "select * from " + typeof(SupportBean).FullName + "(intBoxed in (8, 3))";
            TryReuse(epService, new string[]{exprOne, exprTwo, exprThree});
    
            exprOne = "select * from " + typeof(SupportBean).FullName + "(intBoxed in (3, 1, 3))";
            exprTwo = "select * from " + typeof(SupportBean).FullName + "(intBoxed in (3, 3))";
            exprThree = "select * from " + typeof(SupportBean).FullName + "(intBoxed in (1, 3))";
            TryReuse(epService, new string[]{exprOne, exprTwo, exprThree});
    
            exprOne = "select * from " + typeof(SupportBean).FullName + "(boolPrimitive=false, intBoxed in (1, 2, 3))";
            exprTwo = "select * from " + typeof(SupportBean).FullName + "(boolPrimitive=false, intBoxed in (3, 4))";
            exprThree = "select * from " + typeof(SupportBean).FullName + "(boolPrimitive=false, intBoxed in (3))";
            TryReuse(epService, new string[]{exprOne, exprTwo, exprThree});
    
            exprOne = "select * from " + typeof(SupportBean).FullName + "(intBoxed in (1, 2, 3), longPrimitive >= 0)";
            exprTwo = "select * from " + typeof(SupportBean).FullName + "(intBoxed in (3, 4), intPrimitive >= 0)";
            exprThree = "select * from " + typeof(SupportBean).FullName + "(intBoxed in (3), bytePrimitive < 1)";
            TryReuse(epService, new string[]{exprOne, exprTwo, exprThree});
        }
    
        private void RunAssertionReuseNot(EPServiceProvider epService) {
            string expr = "select * from " + typeof(SupportBean).FullName + "(intBoxed not in [1:2])";
            TryReuse(epService, new string[]{expr, expr});
    
            string exprOne = "select * from " + typeof(SupportBean).FullName + "(intBoxed in (3, 1, 3))";
            string exprTwo = "select * from " + typeof(SupportBean).FullName + "(intBoxed not in (2, 1))";
            string exprThree = "select * from " + typeof(SupportBean).FullName + "(intBoxed not between 0 and -3)";
            TryReuse(epService, new string[]{exprOne, exprTwo, exprThree});
    
            exprOne = "select * from " + typeof(SupportBean).FullName + "(intBoxed not in (1, 4, 5))";
            exprTwo = "select * from " + typeof(SupportBean).FullName + "(intBoxed not in (1, 4, 5))";
            exprThree = "select * from " + typeof(SupportBean).FullName + "(intBoxed not in (4, 5, 1))";
            TryReuse(epService, new string[]{exprOne, exprTwo, exprThree});
    
            exprOne = "select * from " + typeof(SupportBean).FullName + "(intBoxed not in (3:4))";
            exprTwo = "select * from " + typeof(SupportBean).FullName + "(intBoxed not in [1:3))";
            exprThree = "select * from " + typeof(SupportBean).FullName + "(intBoxed not in (1,1,1,33))";
            TryReuse(epService, new string[]{exprOne, exprTwo, exprThree});
        }
    
        private void TryReuse(EPServiceProvider epService, string[] statements) {
            var testListener = new SupportUpdateListener[statements.Length];
            var stmt = new EPStatement[statements.Length];
    
            // create all statements
            for (int i = 0; i < statements.Length; i++) {
                testListener[i] = new SupportUpdateListener();
                stmt[i] = epService.EPAdministrator.CreateEPL(statements[i]);
                stmt[i].AddListener(testListener[i]);
            }
    
            // send event, all should receive the event
            SendBean(epService, "intBoxed", 3);
            for (int i = 0; i < testListener.Length; i++) {
                Assert.IsTrue(testListener[i].IsInvoked);
                testListener[i].Reset();
            }
    
            // stop first, then second, then third etc statement
            for (int toStop = 0; toStop < statements.Length; toStop++) {
                stmt[toStop].Stop();
    
                // send event, all remaining statement received it
                SendBean(epService, "intBoxed", 3);
                for (int i = 0; i <= toStop; i++) {
                    Assert.IsFalse(testListener[i].IsInvoked);
                    testListener[i].Reset();
                }
                for (int i = toStop + 1; i < testListener.Length; i++) {
                    Assert.IsTrue(testListener[i].IsInvoked);
                    testListener[i].Reset();
                }
            }
    
            // now all statements are stopped, send event and verify no listener received
            SendBean(epService, "intBoxed", 3);
            for (int i = 0; i < testListener.Length; i++) {
                Assert.IsFalse(testListener[i].IsInvoked);
            }
        }
    
        private void TryExpr(EPServiceProvider epService, string filterExpr, string fieldName, Object[] values, bool[] isInvoked) {
            string expr = "select * from " + typeof(SupportBean).FullName + filterExpr;
            EPStatement stmt = epService.EPAdministrator.CreateEPL(expr);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            for (int i = 0; i < values.Length; i++) {
                SendBean(epService, fieldName, values[i]);
                Assert.AreEqual(isInvoked[i], listener.IsInvoked, "Listener invocation unexpected for " + filterExpr + " field " + fieldName + "=" + values[i]);
                listener.Reset();
            }
    
            stmt.Stop();
        }
    
        private void SendBeanInt(EPServiceProvider epService, int intPrimitive) {
            var theEvent = new SupportBean();
            theEvent.IntPrimitive = intPrimitive;
            epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void SendBeanString(EPServiceProvider epService, string value) {
            var theEvent = new SupportBean();
            theEvent.TheString = value;
            epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void SendBeanNumeric(EPServiceProvider epService, int intOne, int intTwo) {
            var num = new SupportBeanNumeric(intOne, intTwo);
            epService.EPRuntime.SendEvent(num);
        }
    
        private void SendBean(EPServiceProvider epService, string fieldName, Object value) {
            var theEvent = new SupportBean();
            if (fieldName.Equals("theString")) {
                theEvent.TheString = (string) value;
            }
            else if (fieldName.Equals("boolPrimitive")) {
                theEvent.BoolPrimitive = (bool) value;
            }
            else if (fieldName.Equals("intBoxed")) {
                theEvent.IntBoxed = (int?) value;
            }
            else if (fieldName.Equals("longBoxed")) {
                theEvent.LongBoxed = (long) value;
            }
            epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void TryInvalid(EPServiceProvider epService, string expr) {
            try {
                epService.EPAdministrator.CreateEPL(expr);
                Assert.Fail();
            } catch (EPException ex) {
                // expected
            }
        }
    }
} // end of namespace
