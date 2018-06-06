///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.expr.expr
{
    public class ExecExprCoalesce : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionCoalesceBeans(epService);
            RunAssertionCoalesceLong(epService);
            RunAssertionCoalesceLong_OM(epService);
            RunAssertionCoalesceLong_Compile(epService);
            RunAssertionCoalesceDouble(epService);
            RunAssertionCoalesceInvalid(epService);
        }
    
        private void RunAssertionCoalesceBeans(EPServiceProvider epService) {
            TryCoalesceBeans(epService, "select coalesce(a.TheString, b.TheString) as myString, coalesce(a, b) as myBean" +
                    " from pattern [every (a=" + typeof(SupportBean).FullName + "(TheString='s0') or b=" + typeof(SupportBean).FullName + "(TheString='s1'))]");
    
            TryCoalesceBeans(epService, "SELECT COALESCE(a.TheString, b.TheString) AS myString, COALESCE(a, b) AS myBean" +
                    " FROM PATTERN [EVERY (a=" + typeof(SupportBean).FullName + "(TheString='s0') OR b=" + typeof(SupportBean).FullName + "(TheString='s1'))]");
        }
    
        private void RunAssertionCoalesceLong(EPServiceProvider epService) {
            EPStatement stmt = SetupCoalesce(epService, "coalesce(LongBoxed, IntBoxed, ShortBoxed)");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            Assert.AreEqual(typeof(long?), stmt.EventType.GetPropertyType("result"));
    
            SendEvent(epService, 1L, 2, (short) 3);
            Assert.AreEqual(1L, listener.AssertOneGetNewAndReset().Get("result"));
    
            SendBoxedEvent(epService, null, 2, null);
            Assert.AreEqual(2L, listener.AssertOneGetNewAndReset().Get("result"));
    
            SendBoxedEvent(epService, null, null, short.Parse("3"));
            Assert.AreEqual(3L, listener.AssertOneGetNewAndReset().Get("result"));
    
            SendBoxedEvent(epService, null, null, null);
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("result"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionCoalesceLong_OM(EPServiceProvider epService) {
            string epl = "select coalesce(LongBoxed,IntBoxed,ShortBoxed) as result" +
                    " from " + typeof(SupportBean).FullName + "#length(1000)";
    
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create().Add(Expressions.Coalesce(
                    "LongBoxed", "IntBoxed", "ShortBoxed"), "result");
            model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportBean).FullName)
                .AddView("length", Expressions.Constant(1000)));
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
            Assert.AreEqual(epl, model.ToEPL());
    
            EPStatement stmt = epService.EPAdministrator.Create(model);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            Assert.AreEqual(typeof(long?), stmt.EventType.GetPropertyType("result"));
    
            TryCoalesceLong(epService, listener);
    
            stmt.Dispose();
        }
    
        private void RunAssertionCoalesceLong_Compile(EPServiceProvider epService) {
            string epl = "select coalesce(LongBoxed,IntBoxed,ShortBoxed) as result" +
                    " from " + typeof(SupportBean).FullName + "#length(1000)";
    
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(epl);
            Assert.AreEqual(epl, model.ToEPL());
    
            EPStatement stmt = epService.EPAdministrator.Create(model);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            Assert.AreEqual(typeof(long?), stmt.EventType.GetPropertyType("result"));
    
            TryCoalesceLong(epService, listener);
    
            stmt.Dispose();
        }
    
        private void RunAssertionCoalesceDouble(EPServiceProvider epService) {
            EPStatement stmt = SetupCoalesce(epService, "coalesce(null, byteBoxed, ShortBoxed, IntBoxed, LongBoxed, FloatBoxed, DoubleBoxed)");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            Assert.AreEqual(typeof(double?), stmt.EventType.GetPropertyType("result"));
    
            SendEventWithDouble(epService, null, null, null, null, null, null);
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("result"));
    
            SendEventWithDouble(epService, null, (short) 2, null, null, null, 1d);
            Assert.AreEqual(2d, listener.AssertOneGetNewAndReset().Get("result"));
    
            SendEventWithDouble(epService, null, null, null, null, null, 100d);
            Assert.AreEqual(100d, listener.AssertOneGetNewAndReset().Get("result"));
    
            SendEventWithDouble(epService, null, null, null, null, 10f, 100d);
            Assert.AreEqual(10d, listener.AssertOneGetNewAndReset().Get("result"));
    
            SendEventWithDouble(epService, null, null, 1, 5L, 10f, 100d);
            Assert.AreEqual(1d, listener.AssertOneGetNewAndReset().Get("result"));
    
            SendEventWithDouble(epService, (byte) 3, null, null, null, null, null);
            Assert.AreEqual(3d, listener.AssertOneGetNewAndReset().Get("result"));
    
            SendEventWithDouble(epService, null, null, null, 5L, 10f, 100d);
            Assert.AreEqual(5d, listener.AssertOneGetNewAndReset().Get("result"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionCoalesceInvalid(EPServiceProvider epService) {
            string epl = "select coalesce(null, null) as result" +
                    " from " + typeof(SupportBean).FullName + "#length(3) ";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            Assert.AreEqual(null, stmt.EventType.GetPropertyType("result"));
    
            TryCoalesceInvalid(epService, "coalesce(IntPrimitive)");
            TryCoalesceInvalid(epService, "coalesce(IntPrimitive, string)");
            TryCoalesceInvalid(epService, "coalesce(IntPrimitive, xxx)");
            TryCoalesceInvalid(epService, "coalesce(IntPrimitive, booleanBoxed)");
            TryCoalesceInvalid(epService, "coalesce(charPrimitive, LongBoxed)");
            TryCoalesceInvalid(epService, "coalesce(charPrimitive, string, string)");
            TryCoalesceInvalid(epService, "coalesce(string, LongBoxed)");
            TryCoalesceInvalid(epService, "coalesce(null, LongBoxed, string)");
            TryCoalesceInvalid(epService, "coalesce(null, null, BoolBoxed, 1l)");
        }
    
        private EPStatement SetupCoalesce(EPServiceProvider epService, string coalesceExpr) {
            string epl = "select " + coalesceExpr + " as result" +
                    " from " + typeof(SupportBean).FullName + "#length(1000) ";
            return epService.EPAdministrator.CreateEPL(epl);
        }
    
        private void TryCoalesceInvalid(EPServiceProvider epService, string coalesceExpr) {
            string epl = "select " + coalesceExpr + " as result" +
                    " from " + typeof(SupportBean).FullName + "#length(3) ";
    
            try {
                epService.EPAdministrator.CreateEPL(epl);
                Assert.Fail();
            } catch (EPStatementException) {
                // expected
            }
        }
    
        private void TryCoalesceBeans(EPServiceProvider epService, string epl) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SupportBean theEvent = SendEvent(epService, "s0");
            EventBean eventReceived = listener.AssertOneGetNewAndReset();
            Assert.AreEqual("s0", eventReceived.Get("myString"));
            Assert.AreSame(theEvent, eventReceived.Get("myBean"));
    
            theEvent = SendEvent(epService, "s1");
            eventReceived = listener.AssertOneGetNewAndReset();
            Assert.AreEqual("s1", eventReceived.Get("myString"));
            Assert.AreSame(theEvent, eventReceived.Get("myBean"));
    
            stmt.Dispose();
        }
    
        private void TryCoalesceLong(EPServiceProvider epService, SupportUpdateListener listener) {
            SendEvent(epService, 1L, 2, (short) 3);
            Assert.AreEqual(1L, listener.AssertOneGetNewAndReset().Get("result"));
    
            SendBoxedEvent(epService, null, 2, null);
            Assert.AreEqual(2L, listener.AssertOneGetNewAndReset().Get("result"));
    
            SendBoxedEvent(epService, null, null, 3);
            Assert.AreEqual(3L, listener.AssertOneGetNewAndReset().Get("result"));
    
            SendBoxedEvent(epService, null, null, null);
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("result"));
        }
    
        private SupportBean SendEvent(EPServiceProvider epService, string theString) {
            var bean = new SupportBean();
            bean.TheString = theString;
            epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    
        private void SendEvent(EPServiceProvider epService, long longBoxed, int intBoxed, short shortBoxed) {
            SendBoxedEvent(epService, longBoxed, intBoxed, shortBoxed);
        }
    
        private void SendBoxedEvent(EPServiceProvider epService, long? longBoxed, int? intBoxed, short? shortBoxed) {
            var bean = new SupportBean();
            bean.LongBoxed = longBoxed;
            bean.IntBoxed = intBoxed;
            bean.ShortBoxed = shortBoxed;
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendEventWithDouble(EPServiceProvider epService, byte? byteBoxed, short? shortBoxed, int? intBoxed, long? longBoxed, float? floatBoxed, double? doubleBoxed) {
            var bean = new SupportBean();
            bean.ByteBoxed = byteBoxed;
            bean.ShortBoxed = shortBoxed;
            bean.IntBoxed = intBoxed;
            bean.LongBoxed = longBoxed;
            bean.FloatBoxed = floatBoxed;
            bean.DoubleBoxed = doubleBoxed;
            epService.EPRuntime.SendEvent(bean);
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
