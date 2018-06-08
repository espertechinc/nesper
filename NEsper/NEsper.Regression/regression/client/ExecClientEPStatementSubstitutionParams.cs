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
using com.espertech.esper.compat;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;


using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    public class ExecClientEPStatementSubstitutionParams : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionNamedParameter(epService);
            RunAssertionMethodInvocation(epService);
            RunAssertionPattern(epService);
            RunAssertionSubselect(epService);
            RunAssertionSimpleOneParameter(epService);
            RunAssertionSimpleTwoParameterFilter(epService);
            RunAssertionSimpleTwoParameterWhere(epService);
            RunAssertionSimpleTwoParameterWhereNamed(epService);
            RunAssertionSimpleNoParameter(epService);
            RunAssertionInvalidParameterNotSet(epService);
            RunAssertionInvalidParameterType(epService);
            RunAssertionInvalidNoParameters(epService);
            RunAssertionInvalidSetObject(epService);
            RunAssertionInvalidCreateEPL(epService);
            RunAssertionInvalidCreatePattern(epService);
            RunAssertionInvalidCompile(epService);
            RunAssertionInvalidViewParameter(epService);
        }
    
        private void RunAssertionNamedParameter(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            string epl = "select ?:my/value/int as c0 from SupportBean(TheString = ?:somevalue, IntPrimitive=?:my/value/int, LongPrimitive=?:/my/value/long)";
            EPPreparedStatement prepared = epService.EPAdministrator.PrepareEPL(epl);
            prepared.SetObject("somevalue", "E1");
            prepared.SetObject("my/value/int", 10);
            prepared.SetObject("/my/value/long", 100L);
            var listenerOne = new SupportUpdateListener();
            epService.EPAdministrator.Create(prepared).Events += listenerOne.Update;
    
            SupportBean @event = new SupportBean("E1", 10);
            @event.LongPrimitive = 100;
            epService.EPRuntime.SendEvent(@event);
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), "c0".Split(','), new object[]{10});
    
            SupportMessageAssertUtil.TryInvalid(epService, "select ?,?:a from SupportBean",
                    "Inconsistent use of substitution parameters, expecting all substitutions to either all provide a name or provide no name");
    
            SupportMessageAssertUtil.TryInvalid(epService, "select ?:select from SupportBean",
                    "Incorrect syntax near ':' ('select' is a reserved keyword) at line 1 column 8 near reserved keyword 'select' [");
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionMethodInvocation(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            EPPreparedStatement prepared = epService.EPAdministrator.PrepareEPL(
                "select * from SupportBean(TheString = ?.get_TheString())");
            prepared.SetObject(1, new SupportBean("E1", 0));
            var listenerOne = new SupportUpdateListener();
            epService.EPAdministrator.Create(prepared).Events += listenerOne.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            Assert.IsTrue(listenerOne.IsInvoked);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionPattern(EPServiceProvider epService) {
            string stmt = typeof(SupportBean).FullName + "(TheString=?)";
            EPPreparedStatement prepared = epService.EPAdministrator.PreparePattern(stmt);
    
            prepared.SetObject(1, "e1");
            EPStatement statement = epService.EPAdministrator.Create(prepared);
            var listenerOne = new SupportUpdateListener();
            statement.Events += listenerOne.Update;
            Assert.AreEqual("select * from pattern [" + typeof(SupportBean).FullName + "(TheString=\"e1\")]", statement.Text);
    
            prepared.SetObject(1, "e2");
            statement = epService.EPAdministrator.Create(prepared);
            var listenerTwo = new SupportUpdateListener();
            statement.Events += listenerTwo.Update;
            Assert.AreEqual("select * from pattern [com.espertech.esper.supportregression.bean.SupportBean(TheString=\"e2\")]", statement.Text);
    
            epService.EPRuntime.SendEvent(new SupportBean("e2", 10));
            Assert.IsFalse(listenerOne.IsInvoked);
            Assert.IsTrue(listenerTwo.GetAndClearIsInvoked());
    
            epService.EPRuntime.SendEvent(new SupportBean("e1", 10));
            Assert.IsFalse(listenerTwo.IsInvoked);
            Assert.IsTrue(listenerOne.GetAndClearIsInvoked());
    
            statement.Dispose();
            prepared = epService.EPAdministrator.PrepareEPL("create window MyWindow#time(?) as " + typeof(SupportBean).FullName);
            prepared.SetObject(1, 300);
            statement = epService.EPAdministrator.Create(prepared);
            Assert.AreEqual("create window MyWindow#time(300) as select * from " + typeof(SupportBean).FullName, statement.Text);
        }
    
        private void RunAssertionSubselect(EPServiceProvider epService) {
            string stmtText = "select (" +
                    "select symbol from " + typeof(SupportMarketDataBean).FullName + "(symbol=?)#lastevent) as mysymbol from " +
                    typeof(SupportBean).FullName;
    
            EPPreparedStatement preparedStmt = epService.EPAdministrator.PrepareEPL(stmtText);
    
            preparedStmt.SetObject(1, "S1");
            EPStatement stmtS1 = epService.EPAdministrator.Create(preparedStmt);
            var listenerOne = new SupportUpdateListener();
            stmtS1.Events += listenerOne.Update;
    
            preparedStmt.SetObject(1, "S2");
            EPStatement stmtS2 = epService.EPAdministrator.Create(preparedStmt);
            var listenerTwo = new SupportUpdateListener();
            stmtS2.Events += listenerTwo.Update;
    
            // test no event, should return null
            epService.EPRuntime.SendEvent(new SupportBean("e1", -1));
            Assert.AreEqual(null, listenerOne.AssertOneGetNewAndReset().Get("mysymbol"));
            Assert.AreEqual(null, listenerTwo.AssertOneGetNewAndReset().Get("mysymbol"));
    
            // test one non-matching event
            epService.EPRuntime.SendEvent(new SupportMarketDataBean("XX", 0, 0L, ""));
            epService.EPRuntime.SendEvent(new SupportBean("e1", -1));
            Assert.AreEqual(null, listenerOne.AssertOneGetNewAndReset().Get("mysymbol"));
            Assert.AreEqual(null, listenerTwo.AssertOneGetNewAndReset().Get("mysymbol"));
    
            // test S2 matching event
            epService.EPRuntime.SendEvent(new SupportMarketDataBean("S2", 0, 0L, ""));
            epService.EPRuntime.SendEvent(new SupportBean("e1", -1));
            Assert.AreEqual(null, listenerOne.AssertOneGetNewAndReset().Get("mysymbol"));
            Assert.AreEqual("S2", listenerTwo.AssertOneGetNewAndReset().Get("mysymbol"));
    
            // test S1 matching event
            epService.EPRuntime.SendEvent(new SupportMarketDataBean("S1", 0, 0L, ""));
            epService.EPRuntime.SendEvent(new SupportBean("e1", -1));
            Assert.AreEqual("S1", listenerOne.AssertOneGetNewAndReset().Get("mysymbol"));
            Assert.AreEqual("S2", listenerTwo.AssertOneGetNewAndReset().Get("mysymbol"));
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionSimpleOneParameter(EPServiceProvider epService) {
            string stmt = "select * from " + typeof(SupportBean).FullName + "(TheString=?)";
            EPPreparedStatement prepared = epService.EPAdministrator.PrepareEPL(stmt);
    
            prepared.SetObject(1, "e1");
            EPStatement statement = epService.EPAdministrator.Create(prepared);
            var listenerOne = new SupportUpdateListener();
            statement.Events += listenerOne.Update;
            Assert.AreEqual("select * from " + typeof(SupportBean).FullName + "(TheString=\"e1\")", statement.Text);
    
            prepared.SetObject(1, "e2");
            statement = epService.EPAdministrator.Create(prepared);
            var listenerTwo = new SupportUpdateListener();
            statement.Events += listenerTwo.Update;
            Assert.AreEqual("select * from com.espertech.esper.supportregression.bean.SupportBean(TheString=\"e2\")", statement.Text);
    
            epService.EPRuntime.SendEvent(new SupportBean("e2", 10));
            Assert.IsFalse(listenerOne.IsInvoked);
            Assert.IsTrue(listenerTwo.GetAndClearIsInvoked());
    
            epService.EPRuntime.SendEvent(new SupportBean("e1", 10));
            Assert.IsFalse(listenerTwo.IsInvoked);
            Assert.IsTrue(listenerOne.GetAndClearIsInvoked());
    
            // Test substitution parameter and inheritance in key matching
            epService.EPAdministrator.Configuration.AddEventType("MyEventOne", typeof(MyEventOne));
            string epl = "select * from MyEventOne(key = ?)";
            EPPreparedStatement preparedStatement = epService.EPAdministrator.PrepareEPL(epl);
            var lKey = new MyObjectKeyInterface();
            preparedStatement.SetObject(1, lKey);
            statement = epService.EPAdministrator.Create(preparedStatement);
            statement.Events += listenerOne.Update;
    
            epService.EPRuntime.SendEvent(new MyEventOne(lKey));
            Assert.IsTrue(listenerOne.GetAndClearIsInvoked());
    
            // Test substitution parameter and concrete subclass in key matching
            epService.EPAdministrator.Configuration.AddEventType("MyEventTwo", typeof(MyEventTwo));
            epl = "select * from MyEventTwo where key = ?";
            preparedStatement = epService.EPAdministrator.PrepareEPL(epl);
            var cKey = new MyObjectKeyConcrete();
            preparedStatement.SetObject(1, cKey);
            statement = epService.EPAdministrator.Create(preparedStatement);
            statement.Events += listenerOne.Update;
    
            epService.EPRuntime.SendEvent(new MyEventTwo(cKey));
            Assert.IsTrue(listenerOne.GetAndClearIsInvoked());
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionSimpleTwoParameterFilter(EPServiceProvider epService) {
            string stmt = "select * from " + typeof(SupportBean).FullName + "(TheString=?,IntPrimitive=?)";
            RunSimpleTwoParameter(epService, stmt, null, true);
        }
    
        private void RunAssertionSimpleTwoParameterWhere(EPServiceProvider epService) {
            string stmt = "select * from " + typeof(SupportBean).FullName + " where TheString=? and IntPrimitive=?";
            RunSimpleTwoParameter(epService, stmt, null, false);
        }
    
        private void RunAssertionSimpleTwoParameterWhereNamed(EPServiceProvider epService) {
            string stmt = "select * from " + typeof(SupportBean).FullName + " where TheString=? and IntPrimitive=?";
            RunSimpleTwoParameter(epService, stmt, "s1", false);
        }
    
        private void RunSimpleTwoParameter(EPServiceProvider epService, string stmtText, string statementName, bool compareText) {
            EPPreparedStatement prepared = epService.EPAdministrator.PrepareEPL(stmtText);
    
            prepared.SetObject(1, "e1");
            prepared.SetObject(2, 1);
            EPStatement statement;
            if (statementName != null) {
                statement = epService.EPAdministrator.Create(prepared, statementName);
            } else {
                statement = epService.EPAdministrator.Create(prepared);
            }
            var listenerOne = new SupportUpdateListener();
            statement.Events += listenerOne.Update;
            if (compareText) {
                Assert.AreEqual("select * from " + typeof(SupportBean).FullName + "(TheString=\"e1\" and IntPrimitive=1)", statement.Text);
            }
    
            prepared.SetObject(1, "e2");
            prepared.SetObject(2, 2);
            if (statementName != null) {
                statement = epService.EPAdministrator.Create(prepared, statementName + "_1");
            } else {
                statement = epService.EPAdministrator.Create(prepared);
            }
            var listenerTwo = new SupportUpdateListener();
            statement.Events += listenerTwo.Update;
            if (compareText) {
                Assert.AreEqual("select * from " + typeof(SupportBean).FullName + "(TheString=\"e2\" and IntPrimitive=2)", statement.Text);
            }
    
            epService.EPRuntime.SendEvent(new SupportBean("e2", 2));
            Assert.IsFalse(listenerOne.IsInvoked);
            Assert.IsTrue(listenerTwo.GetAndClearIsInvoked());
    
            epService.EPRuntime.SendEvent(new SupportBean("e1", 1));
            Assert.IsFalse(listenerTwo.IsInvoked);
            Assert.IsTrue(listenerOne.GetAndClearIsInvoked());
    
            epService.EPRuntime.SendEvent(new SupportBean("e1", 2));
            Assert.IsFalse(listenerOne.IsInvoked);
            Assert.IsFalse(listenerTwo.IsInvoked);
    
            statement.Dispose();
        }
    
        private void RunAssertionSimpleNoParameter(EPServiceProvider epService) {
            string stmt = "select * from " + typeof(SupportBean).FullName + "(TheString=\"e1\")";
            EPPreparedStatement prepared = epService.EPAdministrator.PrepareEPL(stmt);
    
            EPStatement statement = epService.EPAdministrator.Create(prepared);
            var listenerOne = new SupportUpdateListener();
            statement.Events += listenerOne.Update;
            Assert.AreEqual("select * from " + typeof(SupportBean).FullName + "(TheString=\"e1\")", statement.Text);
    
            statement = epService.EPAdministrator.Create(prepared);
            var listenerTwo = new SupportUpdateListener();
            statement.Events += listenerTwo.Update;
            Assert.AreEqual("select * from com.espertech.esper.supportregression.bean.SupportBean(TheString=\"e1\")", statement.Text);
    
            epService.EPRuntime.SendEvent(new SupportBean("e2", 10));
            Assert.IsFalse(listenerOne.IsInvoked);
            Assert.IsFalse(listenerTwo.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("e1", 10));
            Assert.IsTrue(listenerOne.GetAndClearIsInvoked());
            Assert.IsTrue(listenerTwo.GetAndClearIsInvoked());
    
            statement.Dispose();
        }
    
        private void RunAssertionInvalidParameterNotSet(EPServiceProvider epService) {
            string stmt = "select * from " + typeof(SupportBean).FullName + "(TheString=?)";
            EPPreparedStatement prepared = epService.EPAdministrator.PrepareEPL(stmt);
    
            try {
                epService.EPAdministrator.Create(prepared);
                Assert.Fail();
            } catch (EPException ex) {
                Assert.AreEqual("Substitution parameter value for index 1 not set, please provide a value for this parameter", ex.Message);
            }
    
            stmt = "select * from " + typeof(SupportBean).FullName + "(TheString in (?, ?))";
            prepared = epService.EPAdministrator.PrepareEPL(stmt);
    
            try {
                epService.EPAdministrator.Create(prepared);
                Assert.Fail();
            } catch (EPException) {
                // expected
            }
    
            try {
                prepared.SetObject(1, "");
                epService.EPAdministrator.Create(prepared);
                Assert.Fail();
            } catch (EPException) {
                // expected
            }
    
            // success
            prepared.SetObject(2, "");
            epService.EPAdministrator.Create(prepared).Dispose();
        }
    
        private void RunAssertionInvalidParameterType(EPServiceProvider epService) {
            string stmt = "select * from " + typeof(SupportBean).FullName + "(TheString=?)";
            EPPreparedStatement prepared = epService.EPAdministrator.PrepareEPL(stmt);
    
            try {
                prepared.SetObject(1, -1);
                epService.EPAdministrator.Create(prepared);
                Assert.Fail();
            } catch (EPException ex) {
                SupportMessageAssertUtil.AssertMessage(ex, "Failed to validate filter expression 'TheString=-1': Implicit conversion from datatype '" + Name.Clean<int>() + "' to '" + Name.Clean<string>() + "' is not allowed [");
            }
        }
    
        private void RunAssertionInvalidNoParameters(EPServiceProvider epService) {
            string stmt = "select * from " + typeof(SupportBean).FullName + "(TheString='ABC')";
            EPPreparedStatement prepared = epService.EPAdministrator.PrepareEPL(stmt);
    
            try {
                prepared.SetObject(1, -1);
                Assert.Fail();
            } catch (ArgumentException ex) {
                Assert.AreEqual("Statement does not have substitution parameters indicated by the '?' character", ex.Message);
            }
        }
    
        private void RunAssertionInvalidSetObject(EPServiceProvider epService) {
            string stmt = "select * from " + typeof(SupportBean).FullName + "(TheString=?)";
            EPPreparedStatement prepared = epService.EPAdministrator.PrepareEPL(stmt);
    
            try {
                prepared.SetObject(0, "");
                Assert.Fail();
            } catch (ArgumentException ex) {
                Assert.AreEqual("Substitution parameter index starts at 1", ex.Message);
            }
    
            try {
                prepared.SetObject(2, "");
                Assert.Fail();
            } catch (ArgumentException ex) {
                Assert.AreEqual("Invalid substitution parameter index of 2 supplied, the maximum for this statement is 1", ex.Message);
            }
        }
    
        private void RunAssertionInvalidCreateEPL(EPServiceProvider epService) {
            string stmt = "select * from " + typeof(SupportBean).FullName + "(TheString=?)";
            try {
                epService.EPAdministrator.CreateEPL(stmt);
            } catch (EPException ex) {
                SupportMessageAssertUtil.AssertMessage(ex, "Failed to validate filter expression 'TheString=?': Invalid use of substitution parameters marked by '?' in statement, use the prepare method to prepare statements with substitution parameters");
            }
        }
    
        private void RunAssertionInvalidCreatePattern(EPServiceProvider epService) {
            string stmt = typeof(SupportBean).FullName + "(TheString=?)";
            try {
                epService.EPAdministrator.CreatePattern(stmt);
            } catch (EPException ex) {
                SupportMessageAssertUtil.AssertMessage(ex,
                        "Failed to validate filter expression 'TheString=?': Invalid use of substitution parameters marked by '?' in statement, use the prepare method to prepare statements");
            }
        }
    
        private void RunAssertionInvalidCompile(EPServiceProvider epService) {
            string stmt = "select * from " + typeof(SupportBean).FullName + "(TheString=?)";
            try {
                epService.EPAdministrator.CompileEPL(stmt);
            } catch (EPException ex) {
                Assert.AreEqual("Invalid use of substitution parameters marked by '?' in statement, use the prepare method to prepare statements with substitution parameters", ex.Message);
            }
        }
    
        private void RunAssertionInvalidViewParameter(EPServiceProvider epService) {
            string stmt = "select * from " + typeof(SupportBean).FullName + "#length(?)";
            try {
                epService.EPAdministrator.PrepareEPL(stmt);
            } catch (EPException ex) {
                SupportMessageAssertUtil.AssertMessage(ex, "Incorrect syntax near '?' expecting a closing parenthesis ')' but found a questionmark '?' at line 1 column 70, please check the view specifications within the from clause [");
            }
        }
    
        public interface IKey {
        }
    
        public class MyObjectKeyInterface : IKey {
        }
    
        public class MyEventOne {
            private IKey key;
    
            public MyEventOne(IKey key) {
                this.key = key;
            }

            public IKey Key
            {
                get { return key; }
            }
        }
    
        [Serializable]
        public class MyObjectKeyConcrete  {
        }
    
        [Serializable]
        public class MyEventTwo  {
            private MyObjectKeyConcrete key;
    
            public MyEventTwo(MyObjectKeyConcrete key) {
                this.key = key;
            }

            public MyObjectKeyConcrete Key
            {
                get { return key; }
            }
        }
    }
} // end of namespace
