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
            string expr = "select * from pattern [a=" + typeof(SupportBeanNumeric).FullName + " -> every b=" + typeof(SupportBean).FullName
                    + "(IntPrimitive in (a.intOne, a.intTwo))]";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(expr);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendBeanNumeric(epService, 10, 20);
            SendBeanInt(epService, 10);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            SendBeanInt(epService, 11);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
            SendBeanInt(epService, 20);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            stmt.Stop();
    
            expr = "select * from pattern [a=" + typeof(SupportBean_S0).FullName + " -> every b=" + typeof(SupportBean).FullName
                    + "(TheString in (a.p00, a.p01, a.p02))]";
            stmt = epService.EPAdministrator.CreateEPL(expr);
            stmt.Events += listener.Update;
    
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
            string expr = "select * from " + typeof(SupportBean).FullName + "(IntPrimitive in (1, 10))";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(expr);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
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
            inStmt.Events += listener.Update;
    
            var theEvent = new SupportBean();
            theEvent.EnumValue = SupportEnum.ENUM_VALUE_2;
            epService.EPRuntime.SendEvent(theEvent);
    
            Assert.IsTrue(listener.IsInvoked);
    
            inStmt.Dispose();
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            // we do not coerce
            TryInvalid(epService, "select * from " + typeof(SupportBean).FullName + "(IntPrimitive in (1L, 10L))");
            TryInvalid(epService, "select * from " + typeof(SupportBean).FullName + "(IntPrimitive in (1, 10L))");
            TryInvalid(epService, "select * from " + typeof(SupportBean).FullName + "(IntPrimitive in (1, 'x'))");
    
            string expr = "select * from pattern [a=" + typeof(SupportBean).FullName + " -> b=" + typeof(SupportBean).FullName
                    + "(IntPrimitive in (a.LongPrimitive, a.LongBoxed))]";
            TryInvalid(epService, expr);
        }
    
        private void RunAssertionInExpr(EPServiceProvider epService) {
            TryExpr(epService, "(TheString > 'b')", "TheString", new object[]{"a", "b", "c", "d"}, new[]{false, false, true, true});
            TryExpr(epService, "(TheString < 'b')", "TheString", new object[]{"a", "b", "c", "d"}, new[]{true, false, false, false});
            TryExpr(epService, "(TheString >= 'b')", "TheString", new object[]{"a", "b", "c", "d"}, new[]{false, true, true, true});
            TryExpr(epService, "(TheString <= 'b')", "TheString", new object[]{"a", "b", "c", "d"}, new[]{true, true, false, false});
            TryExpr(epService, "(TheString in ['b':'d'])", "TheString", new object[]{"a", "b", "c", "d", "e"}, new[]{false, true, true, true, false});
            TryExpr(epService, "(TheString in ('b':'d'])", "TheString", new object[]{"a", "b", "c", "d", "e"}, new[]{false, false, true, true, false});
            TryExpr(epService, "(TheString in ['b':'d'))", "TheString", new object[]{"a", "b", "c", "d", "e"}, new[]{false, true, true, false, false});
            TryExpr(epService, "(TheString in ('b':'d'))", "TheString", new object[]{"a", "b", "c", "d", "e"}, new[]{false, false, true, false, false});
            TryExpr(epService, "(BoolPrimitive in (false))", "BoolPrimitive", new object[]{true, false}, new[]{false, true});
            TryExpr(epService, "(BoolPrimitive in (false, false, false))", "BoolPrimitive", new object[]{true, false}, new[]{false, true});
            TryExpr(epService, "(BoolPrimitive in (false, true, false))", "BoolPrimitive", new object[]{true, false}, new[]{true, true});
            TryExpr(epService, "(IntBoxed in (4, 6, 1))", "IntBoxed", new object[]{0, 1, 2, 3, 4, 5, 6}, new[]{false, true, false, false, true, false, true});
            TryExpr(epService, "(IntBoxed in (3))", "IntBoxed", new object[]{0, 1, 2, 3, 4, 5, 6}, new[]{false, false, false, true, false, false, false});
            TryExpr(epService, "(LongBoxed in (3))", "LongBoxed", new object[]{0L, 1L, 2L, 3L, 4L, 5L, 6L}, new[]{false, false, false, true, false, false, false});
            TryExpr(epService, "(IntBoxed between 4 and 6)", "IntBoxed", new object[]{0, 1, 2, 3, 4, 5, 6}, new[]{false, false, false, false, true, true, true});
            TryExpr(epService, "(IntBoxed between 2 and 1)", "IntBoxed", new object[]{0, 1, 2, 3, 4, 5, 6}, new[]{false, true, true, false, false, false, false});
            TryExpr(epService, "(IntBoxed between 4 and -1)", "IntBoxed", new object[]{0, 1, 2, 3, 4, 5, 6}, new[]{true, true, true, true, true, false, false});
            TryExpr(epService, "(IntBoxed in [2:4])", "IntBoxed", new object[]{0, 1, 2, 3, 4, 5, 6}, new[]{false, false, true, true, true, false, false});
            TryExpr(epService, "(IntBoxed in (2:4])", "IntBoxed", new object[]{0, 1, 2, 3, 4, 5, 6}, new[]{false, false, false, true, true, false, false});
            TryExpr(epService, "(IntBoxed in [2:4))", "IntBoxed", new object[]{0, 1, 2, 3, 4, 5, 6}, new[]{false, false, true, true, false, false, false});
            TryExpr(epService, "(IntBoxed in (2:4))", "IntBoxed", new object[]{0, 1, 2, 3, 4, 5, 6}, new[]{false, false, false, true, false, false, false});
        }
    
        private void RunAssertionNotInExpr(EPServiceProvider epService) {
            TryExpr(epService, "(IntBoxed not between 4 and 6)", "IntBoxed", new object[]{0, 1, 2, 3, 4, 5, 6}, new[]{true, true, true, true, false, false, false});
            TryExpr(epService, "(IntBoxed not between 2 and 1)", "IntBoxed", new object[]{0, 1, 2, 3, 4, 5, 6}, new[]{true, false, false, true, true, true, true});
            TryExpr(epService, "(IntBoxed not between 4 and -1)", "IntBoxed", new object[]{0, 1, 2, 3, 4, 5, 6}, new[]{false, false, false, false, false, true, true});
            TryExpr(epService, "(IntBoxed not in [2:4])", "IntBoxed", new object[]{0, 1, 2, 3, 4, 5, 6}, new[]{true, true, false, false, false, true, true});
            TryExpr(epService, "(IntBoxed not in (2:4])", "IntBoxed", new object[]{0, 1, 2, 3, 4, 5, 6}, new[]{true, true, true, false, false, true, true});
            TryExpr(epService, "(IntBoxed not in [2:4))", "IntBoxed", new object[]{0, 1, 2, 3, 4, 5, 6}, new[]{true, true, false, false, true, true, true});
            TryExpr(epService, "(IntBoxed not in (2:4))", "IntBoxed", new object[]{0, 1, 2, 3, 4, 5, 6}, new[]{true, true, true, false, true, true, true});
            TryExpr(epService, "(TheString not in ['b':'d'])", "TheString", new object[]{"a", "b", "c", "d", "e"}, new[]{true, false, false, false, true});
            TryExpr(epService, "(TheString not in ('b':'d'])", "TheString", new object[]{"a", "b", "c", "d", "e"}, new[]{true, true, false, false, true});
            TryExpr(epService, "(TheString not in ['b':'d'))", "TheString", new object[]{"a", "b", "c", "d", "e"}, new[]{true, false, false, true, true});
            TryExpr(epService, "(TheString not in ('b':'d'))", "TheString", new object[]{"a", "b", "c", "d", "e"}, new[]{true, true, false, true, true});
            TryExpr(epService, "(TheString not in ('a', 'b'))", "TheString", new object[]{"a", "x", "b", "y"}, new[]{false, true, false, true});
            TryExpr(epService, "(BoolPrimitive not in (false))", "BoolPrimitive", new object[]{true, false}, new[]{true, false});
            TryExpr(epService, "(BoolPrimitive not in (false, false, false))", "BoolPrimitive", new object[]{true, false}, new[]{true, false});
            TryExpr(epService, "(BoolPrimitive not in (false, true, false))", "BoolPrimitive", new object[]{true, false}, new[]{false, false});
            TryExpr(epService, "(IntBoxed not in (4, 6, 1))", "IntBoxed", new object[]{0, 1, 2, 3, 4, 5, 6}, new[]{true, false, true, true, false, true, false});
            TryExpr(epService, "(IntBoxed not in (3))", "IntBoxed", new object[]{0, 1, 2, 3, 4, 5, 6}, new[]{true, true, true, false, true, true, true});
            TryExpr(epService, "(LongBoxed not in (3))", "LongBoxed", new object[]{0L, 1L, 2L, 3L, 4L, 5L, 6L}, new[]{true, true, true, false, true, true, true});
        }
    
        private void RunAssertionReuse(EPServiceProvider epService) {
            string expr = "select * from " + typeof(SupportBean).FullName + "(IntBoxed in [2:4])";
            TryReuse(epService, new[]{expr, expr});
    
            expr = "select * from " + typeof(SupportBean).FullName + "(IntBoxed in (1, 2, 3))";
            TryReuse(epService, new[]{expr, expr});
    
            string exprOne = "select * from " + typeof(SupportBean).FullName + "(IntBoxed in (2:3])";
            string exprTwo = "select * from " + typeof(SupportBean).FullName + "(IntBoxed in (1:3])";
            TryReuse(epService, new[]{exprOne, exprTwo});
    
            exprOne = "select * from " + typeof(SupportBean).FullName + "(IntBoxed in (2, 3, 4))";
            exprTwo = "select * from " + typeof(SupportBean).FullName + "(IntBoxed in (1, 3))";
            TryReuse(epService, new[]{exprOne, exprTwo});
    
            exprOne = "select * from " + typeof(SupportBean).FullName + "(IntBoxed in (2, 3, 4))";
            exprTwo = "select * from " + typeof(SupportBean).FullName + "(IntBoxed in (1, 3))";
            string exprThree = "select * from " + typeof(SupportBean).FullName + "(IntBoxed in (8, 3))";
            TryReuse(epService, new[]{exprOne, exprTwo, exprThree});
    
            exprOne = "select * from " + typeof(SupportBean).FullName + "(IntBoxed in (3, 1, 3))";
            exprTwo = "select * from " + typeof(SupportBean).FullName + "(IntBoxed in (3, 3))";
            exprThree = "select * from " + typeof(SupportBean).FullName + "(IntBoxed in (1, 3))";
            TryReuse(epService, new[]{exprOne, exprTwo, exprThree});
    
            exprOne = "select * from " + typeof(SupportBean).FullName + "(BoolPrimitive=false, IntBoxed in (1, 2, 3))";
            exprTwo = "select * from " + typeof(SupportBean).FullName + "(BoolPrimitive=false, IntBoxed in (3, 4))";
            exprThree = "select * from " + typeof(SupportBean).FullName + "(BoolPrimitive=false, IntBoxed in (3))";
            TryReuse(epService, new[]{exprOne, exprTwo, exprThree});
    
            exprOne = "select * from " + typeof(SupportBean).FullName + "(IntBoxed in (1, 2, 3), LongPrimitive >= 0)";
            exprTwo = "select * from " + typeof(SupportBean).FullName + "(IntBoxed in (3, 4), IntPrimitive >= 0)";
            exprThree = "select * from " + typeof(SupportBean).FullName + "(IntBoxed in (3), bytePrimitive < 1)";
            TryReuse(epService, new[]{exprOne, exprTwo, exprThree});
        }
    
        private void RunAssertionReuseNot(EPServiceProvider epService) {
            string expr = "select * from " + typeof(SupportBean).FullName + "(IntBoxed not in [1:2])";
            TryReuse(epService, new[]{expr, expr});
    
            string exprOne = "select * from " + typeof(SupportBean).FullName + "(IntBoxed in (3, 1, 3))";
            string exprTwo = "select * from " + typeof(SupportBean).FullName + "(IntBoxed not in (2, 1))";
            string exprThree = "select * from " + typeof(SupportBean).FullName + "(IntBoxed not between 0 and -3)";
            TryReuse(epService, new[]{exprOne, exprTwo, exprThree});
    
            exprOne = "select * from " + typeof(SupportBean).FullName + "(IntBoxed not in (1, 4, 5))";
            exprTwo = "select * from " + typeof(SupportBean).FullName + "(IntBoxed not in (1, 4, 5))";
            exprThree = "select * from " + typeof(SupportBean).FullName + "(IntBoxed not in (4, 5, 1))";
            TryReuse(epService, new[]{exprOne, exprTwo, exprThree});
    
            exprOne = "select * from " + typeof(SupportBean).FullName + "(IntBoxed not in (3:4))";
            exprTwo = "select * from " + typeof(SupportBean).FullName + "(IntBoxed not in [1:3))";
            exprThree = "select * from " + typeof(SupportBean).FullName + "(IntBoxed not in (1,1,1,33))";
            TryReuse(epService, new[]{exprOne, exprTwo, exprThree});
        }
    
        private void TryReuse(EPServiceProvider epService, string[] statements) {
            var testListener = new SupportUpdateListener[statements.Length];
            var stmt = new EPStatement[statements.Length];
    
            // create all statements
            for (int i = 0; i < statements.Length; i++) {
                testListener[i] = new SupportUpdateListener();
                stmt[i] = epService.EPAdministrator.CreateEPL(statements[i]);
                stmt[i].Events += testListener[i].Update;
            }
    
            // send event, all should receive the event
            SendBean(epService, "IntBoxed", 3);
            for (int i = 0; i < testListener.Length; i++) {
                Assert.IsTrue(testListener[i].IsInvoked);
                testListener[i].Reset();
            }
    
            // stop first, then second, then third etc statement
            for (int toStop = 0; toStop < statements.Length; toStop++) {
                stmt[toStop].Stop();
    
                // send event, all remaining statement received it
                SendBean(epService, "IntBoxed", 3);
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
            SendBean(epService, "IntBoxed", 3);
            for (int i = 0; i < testListener.Length; i++) {
                Assert.IsFalse(testListener[i].IsInvoked);
            }
        }
    
        private void TryExpr(EPServiceProvider epService, string filterExpr, string fieldName, object[] values, bool[] isInvoked) {
            string expr = "select * from " + typeof(SupportBean).FullName + filterExpr;
            EPStatement stmt = epService.EPAdministrator.CreateEPL(expr);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
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
            if (fieldName.Equals("TheString")) {
                theEvent.TheString = (string) value;
            }
            else if (fieldName.Equals("BoolPrimitive")) {
                theEvent.BoolPrimitive = (bool) value;
            }
            else if (fieldName.Equals("IntBoxed")) {
                theEvent.IntBoxed = (int?) value;
            }
            else if (fieldName.Equals("LongBoxed")) {
                theEvent.LongBoxed = (long) value;
            }
            epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void TryInvalid(EPServiceProvider epService, string expr) {
            try {
                epService.EPAdministrator.CreateEPL(expr);
                Assert.Fail();
            } catch (EPException) {
                // expected
            }
        }
    }
} // end of namespace
