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

namespace com.espertech.esper.regression.epl.other
{
    public class ExecEPLIStreamRStreamKeywords : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionRStreamOnly_OM(epService);
            RunAssertionRStreamOnly_Compile(epService);
            RunAssertionRStreamOnly(epService);
            RunAssertionRStreamInsertInto(epService);
            RunAssertionRStreamInsertIntoRStream(epService);
            RunAssertionRStreamJoin(epService);
            RunAssertionIStreamOnly(epService);
            RunAssertionIStreamInsertIntoRStream(epService);
            RunAssertionIStreamJoin(epService);
        }
    
        private void RunAssertionRStreamOnly_OM(EPServiceProvider epService) {
            string stmtText = "select rstream * from " + typeof(SupportBean).FullName + "#length(3)";
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.CreateWildcard(StreamSelector.RSTREAM_ONLY);
            FromClause fromClause = FromClause.Create(FilterStream.Create(typeof(SupportBean).FullName).AddView(View.Create("length", Expressions.Constant(3))));
            model.FromClause = fromClause;
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
    
            Assert.AreEqual(stmtText, model.ToEPL());
            EPStatement statement = epService.EPAdministrator.Create(model);
            var testListener = new SupportUpdateListener();
            statement.Events += testListener.Update;
    
            Object theEvent = SendEvent(epService, "a", 2);
            Assert.IsFalse(testListener.IsInvoked);
    
            SendEvents(epService, new string[]{"a", "b"});
            Assert.IsFalse(testListener.IsInvoked);
    
            SendEvent(epService, "d", 2);
            Assert.AreSame(theEvent, testListener.LastNewData[0].Underlying);    // receive 'a' as new data
            Assert.IsNull(testListener.LastOldData);  // receive no more old data
    
            statement.Dispose();
        }
    
        private void RunAssertionRStreamOnly_Compile(EPServiceProvider epService) {
            string stmtText = "select rstream * from " + typeof(SupportBean).FullName + "#length(3)";
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(stmtText);
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
    
            Assert.AreEqual(stmtText, model.ToEPL());
            EPStatement statement = epService.EPAdministrator.Create(model);
            var testListener = new SupportUpdateListener();
            statement.Events += testListener.Update;
    
            Object theEvent = SendEvent(epService, "a", 2);
            Assert.IsFalse(testListener.IsInvoked);
    
            SendEvents(epService, new string[]{"a", "b"});
            Assert.IsFalse(testListener.IsInvoked);
    
            SendEvent(epService, "d", 2);
            Assert.AreSame(theEvent, testListener.LastNewData[0].Underlying);    // receive 'a' as new data
            Assert.IsNull(testListener.LastOldData);  // receive no more old data
    
            statement.Dispose();
        }
    
        private void RunAssertionRStreamOnly(EPServiceProvider epService) {
            EPStatement statement = epService.EPAdministrator.CreateEPL(
                    "select rstream * from " + typeof(SupportBean).FullName + "#length(3)");
            var testListener = new SupportUpdateListener();
            statement.Events += testListener.Update;
    
            Object theEvent = SendEvent(epService, "a", 2);
            Assert.IsFalse(testListener.IsInvoked);
    
            SendEvents(epService, new string[]{"a", "b"});
            Assert.IsFalse(testListener.IsInvoked);
    
            SendEvent(epService, "d", 2);
            Assert.AreSame(theEvent, testListener.LastNewData[0].Underlying);    // receive 'a' as new data
            Assert.IsNull(testListener.LastOldData);  // receive no more old data
    
            statement.Dispose();
        }
    
        private void RunAssertionRStreamInsertInto(EPServiceProvider epService) {
            EPStatement statement = epService.EPAdministrator.CreateEPL(
                    "insert into NextStream " +
                            "select rstream s0.TheString as TheString from " + typeof(SupportBean).FullName + "#length(3) as s0");
            var testListener = new SupportUpdateListener();
            statement.Events += testListener.Update;
    
            statement = epService.EPAdministrator.CreateEPL("select * from NextStream");
            var testListenerInsertInto = new SupportUpdateListener();
            statement.Events += testListenerInsertInto.Update;
    
            SendEvent(epService, "a", 2);
            Assert.IsFalse(testListener.IsInvoked);
            Assert.AreEqual("a", testListenerInsertInto.AssertOneGetNewAndReset().Get("TheString"));    // insert into unchanged
    
            SendEvents(epService, new string[]{"b", "c"});
            Assert.IsFalse(testListener.IsInvoked);
            Assert.AreEqual(2, testListenerInsertInto.NewDataList.Count);    // insert into unchanged
            testListenerInsertInto.Reset();
    
            SendEvent(epService, "d", 2);
            Assert.AreSame("a", testListener.LastNewData[0].Get("TheString"));    // receive 'a' as new data
            Assert.IsNull(testListener.LastOldData);  // receive no more old data
            Assert.AreEqual("d", testListenerInsertInto.LastNewData[0].Get("TheString"));    // insert into unchanged
            Assert.IsNull(testListenerInsertInto.LastOldData);  // receive no old data in insert into
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionRStreamInsertIntoRStream(EPServiceProvider epService) {
            EPStatement statement = epService.EPAdministrator.CreateEPL(
                    "insert rstream into NextStream " +
                            "select rstream s0.TheString as TheString from " + typeof(SupportBean).FullName + "#length(3) as s0");
            var testListener = new SupportUpdateListener();
            statement.Events += testListener.Update;
    
            statement = epService.EPAdministrator.CreateEPL("select * from NextStream");
            var testListenerInsertInto = new SupportUpdateListener();
            statement.Events += testListenerInsertInto.Update;
    
            SendEvent(epService, "a", 2);
            Assert.IsFalse(testListener.IsInvoked);
            Assert.IsFalse(testListenerInsertInto.IsInvoked);
    
            SendEvents(epService, new string[]{"b", "c"});
            Assert.IsFalse(testListener.IsInvoked);
            Assert.IsFalse(testListenerInsertInto.IsInvoked);
    
            SendEvent(epService, "d", 2);
            Assert.AreSame("a", testListener.LastNewData[0].Get("TheString"));    // receive 'a' as new data
            Assert.IsNull(testListener.LastOldData);  // receive no more old data
            Assert.AreEqual("a", testListenerInsertInto.LastNewData[0].Get("TheString"));    // insert into unchanged
            Assert.IsNull(testListener.LastOldData);  // receive no old data in insert into
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionRStreamJoin(EPServiceProvider epService) {
            EPStatement statement = epService.EPAdministrator.CreateEPL(
                    "select rstream s1.IntPrimitive as aID, s2.IntPrimitive as bID " +
                            "from " + typeof(SupportBean).FullName + "(TheString='a')#length(2) as s1, "
                            + typeof(SupportBean).FullName + "(TheString='b')#keepall as s2" +
                            " where s1.IntPrimitive = s2.IntPrimitive");
            var testListener = new SupportUpdateListener();
            statement.Events += testListener.Update;
    
            SendEvent(epService, "a", 1);
            SendEvent(epService, "b", 1);
            Assert.IsFalse(testListener.IsInvoked);
    
            SendEvent(epService, "a", 2);
            Assert.IsFalse(testListener.IsInvoked);
    
            SendEvent(epService, "a", 3);
            Assert.AreEqual(1, testListener.LastNewData[0].Get("aID"));    // receive 'a' as new data
            Assert.AreEqual(1, testListener.LastNewData[0].Get("bID"));
            Assert.IsNull(testListener.LastOldData);  // receive no more old data
    
            statement.Dispose();
        }
    
        private void RunAssertionIStreamOnly(EPServiceProvider epService) {
            EPStatement statement = epService.EPAdministrator.CreateEPL(
                    "select istream * from " + typeof(SupportBean).FullName + "#length(1)");
            var testListener = new SupportUpdateListener();
            statement.Events += testListener.Update;
    
            Object theEvent = SendEvent(epService, "a", 2);
            Assert.AreSame(theEvent, testListener.AssertOneGetNewAndReset().Underlying);
    
            theEvent = SendEvent(epService, "b", 2);
            Assert.AreSame(theEvent, testListener.LastNewData[0].Underlying);
            Assert.IsNull(testListener.LastOldData); // receive no old data, just istream events
    
            statement.Dispose();
        }
    
        private void RunAssertionIStreamInsertIntoRStream(EPServiceProvider epService) {
            EPStatement statement = epService.EPAdministrator.CreateEPL(
                    "insert rstream into NextStream " +
                            "select istream a.TheString as TheString from " + typeof(SupportBean).FullName + "#length(1) as a");
            var testListener = new SupportUpdateListener();
            statement.Events += testListener.Update;
    
            statement = epService.EPAdministrator.CreateEPL("select * from NextStream");
            var testListenerInsertInto = new SupportUpdateListener();
            statement.Events += testListenerInsertInto.Update;
    
            SendEvent(epService, "a", 2);
            Assert.AreEqual("a", testListener.AssertOneGetNewAndReset().Get("TheString"));
            Assert.IsFalse(testListenerInsertInto.IsInvoked);
    
            SendEvent(epService, "b", 2);
            Assert.AreEqual("b", testListener.LastNewData[0].Get("TheString"));
            Assert.IsNull(testListener.LastOldData);
            Assert.AreEqual("a", testListenerInsertInto.LastNewData[0].Get("TheString"));
            Assert.IsNull(testListenerInsertInto.LastOldData);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionIStreamJoin(EPServiceProvider epService) {
            EPStatement statement = epService.EPAdministrator.CreateEPL(
                    "select istream s1.IntPrimitive as aID, s2.IntPrimitive as bID " +
                            "from " + typeof(SupportBean).FullName + "(TheString='a')#length(2) as s1, "
                            + typeof(SupportBean).FullName + "(TheString='b')#keepall as s2" +
                            " where s1.IntPrimitive = s2.IntPrimitive");
            var testListener = new SupportUpdateListener();
            statement.Events += testListener.Update;
    
            SendEvent(epService, "a", 1);
            SendEvent(epService, "b", 1);
            Assert.AreEqual(1, testListener.LastNewData[0].Get("aID"));    // receive 'a' as new data
            Assert.AreEqual(1, testListener.LastNewData[0].Get("bID"));
            Assert.IsNull(testListener.LastOldData);  // receive no more old data
            testListener.Reset();
    
            SendEvent(epService, "a", 2);
            Assert.IsFalse(testListener.IsInvoked);
    
            SendEvent(epService, "a", 3);
            Assert.IsFalse(testListener.IsInvoked);
    
            statement.Dispose();
        }
    
        private void SendEvents(EPServiceProvider epService, string[] stringValue) {
            for (int i = 0; i < stringValue.Length; i++) {
                SendEvent(epService, stringValue[i], 2);
            }
        }
    
        private Object SendEvent(EPServiceProvider epService, string stringValue, int intPrimitive) {
            var theEvent = new SupportBean();
            theEvent.TheString = stringValue;
            theEvent.IntPrimitive = intPrimitive;
            epService.EPRuntime.SendEvent(theEvent);
            return theEvent;
        }
    }
} // end of namespace
