///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    [TestFixture]
    public class TestEPStatementSubstitutionParams 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listenerOne;
        private SupportUpdateListener _listenerTwo;
    
        [SetUp]
        public void SetUp()
        {
            _listenerOne = new SupportUpdateListener();
            _listenerTwo = new SupportUpdateListener();
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }
    
        [TearDown]
        public void TearDown() 
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listenerOne = null;
            _listenerTwo = null;
        }

        [Test]
        public void TestNamedParameter()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();

            String epl = "select ?:my/value/int as c0 from SupportBean(theString = ?:somevalue, intPrimitive=?:my/value/int, longPrimitive=?:/my/value/long)";
            EPPreparedStatement prepared = _epService.EPAdministrator.PrepareEPL(epl);
            prepared.SetObject("somevalue", "E1");
            prepared.SetObject("my/value/int", 10);
            prepared.SetObject("/my/value/long", 100L);
            _epService.EPAdministrator.Create(prepared).AddListener(_listenerOne);

            SupportBean @event = new SupportBean("E1", 10);
            @event.LongPrimitive = 100;
            _epService.EPRuntime.SendEvent(@event);
            EPAssertionUtil.AssertProps(_listenerOne.AssertOneGetNewAndReset(), "c0".Split(','), new Object[] {10});

            SupportMessageAssertUtil.TryInvalid(_epService, "select ?,?:a from SupportBean",
                    "Inconsistent use of substitution parameters, expecting all substitutions to either all provide a name or provide no name");

            SupportMessageAssertUtil.TryInvalid(_epService, "select ?:select from SupportBean",
                    "Incorrect syntax near ':' ('select' is a reserved keyword) at line 1 column 8 near reserved keyword 'select' [");
        }


        [Test]
        public void TestMethodInvocation()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();

            var prepared = _epService.EPAdministrator.PrepareEPL("select * from SupportBean(TheString = ?.get_TheString())");
            prepared.SetObject(1, new SupportBean("E1", 0));
            _epService.EPAdministrator.Create(prepared).Events += _listenerOne.Update;
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            Assert.IsTrue(_listenerOne.IsInvoked);
        }

        [Test]
        public void TestPattern()
        {
            String stmt = typeof(SupportBean).FullName + "(TheString=?)";
            EPPreparedStatement prepared = _epService.EPAdministrator.PreparePattern(stmt);
    
            prepared.SetObject(1, "e1");
            EPStatement statement = _epService.EPAdministrator.Create(prepared);
            statement.Events += _listenerOne.Update;
            Assert.AreEqual("select * from pattern [com.espertech.esper.support.bean.SupportBean(TheString=\"e1\")]", statement.Text);
    
            prepared.SetObject(1, "e2");
            statement = _epService.EPAdministrator.Create(prepared);
            statement.Events += _listenerTwo.Update;
            Assert.AreEqual("select * from pattern [com.espertech.esper.support.bean.SupportBean(TheString=\"e2\")]", statement.Text);
    
            _epService.EPRuntime.SendEvent(new SupportBean("e2", 10));
            Assert.IsFalse(_listenerOne.IsInvoked);
            Assert.IsTrue(_listenerTwo.GetAndClearIsInvoked());
    
            _epService.EPRuntime.SendEvent(new SupportBean("e1", 10));
            Assert.IsFalse(_listenerTwo.IsInvoked);
            Assert.IsTrue(_listenerOne.GetAndClearIsInvoked());
    
            statement.Dispose();
            prepared = _epService.EPAdministrator.PrepareEPL("create window MyWindow.win:time(?) as " + typeof(SupportBean).FullName);
            prepared.SetObject(1, 300);
            statement = _epService.EPAdministrator.Create(prepared);
            Assert.AreEqual("create window MyWindow.win:time(300) as select * from com.espertech.esper.support.bean.SupportBean", statement.Text);
        }
    
        [Test]
        public void TestSubselect()
        {
            String stmtText = "select (" +
               "select symbol from " + typeof(SupportMarketDataBean).FullName + "(symbol=?).std:lastevent()) as mysymbol from " +
                    typeof(SupportBean).FullName;
    
            EPPreparedStatement preparedStmt = _epService.EPAdministrator.PrepareEPL(stmtText);
    
            preparedStmt.SetObject(1, "S1");
            EPStatement stmtS1 = _epService.EPAdministrator.Create(preparedStmt);
            stmtS1.Events += _listenerOne.Update;
    
            preparedStmt.SetObject(1, "S2");
            EPStatement stmtS2 = _epService.EPAdministrator.Create(preparedStmt);
            stmtS2.Events += _listenerTwo.Update;
    
            // test no event, should return null
            _epService.EPRuntime.SendEvent(new SupportBean("e1", -1));
            Assert.AreEqual(null, _listenerOne.AssertOneGetNewAndReset().Get("mysymbol"));
            Assert.AreEqual(null, _listenerTwo.AssertOneGetNewAndReset().Get("mysymbol"));
    
            // test one non-matching event
            _epService.EPRuntime.SendEvent(new SupportMarketDataBean("XX", 0, 0L, ""));
            _epService.EPRuntime.SendEvent(new SupportBean("e1", -1));
            Assert.AreEqual(null, _listenerOne.AssertOneGetNewAndReset().Get("mysymbol"));
            Assert.AreEqual(null, _listenerTwo.AssertOneGetNewAndReset().Get("mysymbol"));
    
            // test S2 matching event
            _epService.EPRuntime.SendEvent(new SupportMarketDataBean("S2", 0, 0L, ""));
            _epService.EPRuntime.SendEvent(new SupportBean("e1", -1));
            Assert.AreEqual(null, _listenerOne.AssertOneGetNewAndReset().Get("mysymbol"));
            Assert.AreEqual("S2", _listenerTwo.AssertOneGetNewAndReset().Get("mysymbol"));
    
            // test S1 matching event
            _epService.EPRuntime.SendEvent(new SupportMarketDataBean("S1", 0, 0L, ""));
            _epService.EPRuntime.SendEvent(new SupportBean("e1", -1));
            Assert.AreEqual("S1", _listenerOne.AssertOneGetNewAndReset().Get("mysymbol"));
            Assert.AreEqual("S2", _listenerTwo.AssertOneGetNewAndReset().Get("mysymbol"));
        }
    
        [Test]
        public void TestSimpleOneParameter()
        {
            String stmt = "select * from " + typeof(SupportBean).FullName + "(TheString=?)";
            EPPreparedStatement prepared = _epService.EPAdministrator.PrepareEPL(stmt);
    
            prepared.SetObject(1, "e1");
            EPStatement statement = _epService.EPAdministrator.Create(prepared);
            statement.Events += _listenerOne.Update;
            Assert.AreEqual("select * from com.espertech.esper.support.bean.SupportBean(TheString=\"e1\")", statement.Text);
    
            prepared.SetObject(1, "e2");
            statement = _epService.EPAdministrator.Create(prepared);
            statement.Events += _listenerTwo.Update;
            Assert.AreEqual("select * from com.espertech.esper.support.bean.SupportBean(TheString=\"e2\")", statement.Text);
    
            _epService.EPRuntime.SendEvent(new SupportBean("e2", 10));
            Assert.IsFalse(_listenerOne.IsInvoked);
            Assert.IsTrue(_listenerTwo.GetAndClearIsInvoked());
    
            _epService.EPRuntime.SendEvent(new SupportBean("e1", 10));
            Assert.IsFalse(_listenerTwo.IsInvoked);
            Assert.IsTrue(_listenerOne.GetAndClearIsInvoked());
    
            // Test substitution parameter and inheritance in key matching
            _epService.EPAdministrator.Configuration.AddEventType("MyEventOne", typeof(MyEventOne));
            String epl = "select * from MyEventOne(key = ?)";
            EPPreparedStatement preparedStatement = _epService.EPAdministrator.PrepareEPL(epl);
            MyObjectKeyInterface lKey = new MyObjectKeyInterface();
            preparedStatement.SetObject(1, lKey);
            statement = _epService.EPAdministrator.Create(preparedStatement);
            statement.Events += _listenerOne.Update;
    
            _epService.EPRuntime.SendEvent(new MyEventOne(lKey));
            Assert.IsTrue(_listenerOne.GetAndClearIsInvoked());
    
            // Test substitution parameter and concrete subclass in key matching 
            _epService.EPAdministrator.Configuration.AddEventType("MyEventTwo", typeof(MyEventTwo));
            epl = "select * from MyEventTwo where key = ?";
            preparedStatement = _epService.EPAdministrator.PrepareEPL(epl);
            MyObjectKeyConcrete cKey = new MyObjectKeyConcrete();
            preparedStatement.SetObject(1, cKey);
            statement = _epService.EPAdministrator.Create(preparedStatement);
            statement.Events += _listenerOne.Update;
    
            _epService.EPRuntime.SendEvent(new MyEventTwo(cKey));
            Assert.IsTrue(_listenerOne.GetAndClearIsInvoked());
        }
    
        [Test]
        public void TestSimpleTwoParameterFilter()
        {
            String stmt = "select * from " + typeof(SupportBean).FullName + "(TheString=?,IntPrimitive=?)";
            RunSimpleTwoParameter(stmt, null, true);
        }
    
        [Test]
        public void TestSimpleTwoParameterWhere()
        {
            String stmt = "select * from " + typeof(SupportBean).FullName + " where TheString=? and IntPrimitive=?";
            RunSimpleTwoParameter(stmt, null, false);
        }
    
        [Test]
        public void TestSimpleTwoParameterWhereNamed()
        {
            String stmt = "select * from " + typeof(SupportBean).FullName + " where TheString=? and IntPrimitive=?";
            RunSimpleTwoParameter(stmt, "s1", false);
        }
    
        private void RunSimpleTwoParameter(String stmtText, String statementName, bool compareText)
        {
            EPPreparedStatement prepared = _epService.EPAdministrator.PrepareEPL(stmtText);
    
            prepared.SetObject(1, "e1");
            prepared.SetObject(2, 1);
            EPStatement statement;
            if (statementName != null)
            {
                statement = _epService.EPAdministrator.Create(prepared, statementName);
            }
            else
            {
                statement = _epService.EPAdministrator.Create(prepared);
            }
            statement.Events += _listenerOne.Update;
            if (compareText)
            {
                Assert.AreEqual("select * from com.espertech.esper.support.bean.SupportBean(TheString=\"e1\" and IntPrimitive=1)", statement.Text);
            }
    
            prepared.SetObject(1, "e2");
            prepared.SetObject(2, 2);
            if (statementName != null)
            {
                statement = _epService.EPAdministrator.Create(prepared, statementName + "_1");
            }
            else
            {
                statement = _epService.EPAdministrator.Create(prepared);
            }
            statement.Events += _listenerTwo.Update;
            if (compareText)
            {
                Assert.AreEqual("select * from com.espertech.esper.support.bean.SupportBean(TheString=\"e2\" and IntPrimitive=2)", statement.Text);
            }
    
            _epService.EPRuntime.SendEvent(new SupportBean("e2", 2));
            Assert.IsFalse(_listenerOne.IsInvoked);
            Assert.IsTrue(_listenerTwo.GetAndClearIsInvoked());
    
            _epService.EPRuntime.SendEvent(new SupportBean("e1", 1));
            Assert.IsFalse(_listenerTwo.IsInvoked);
            Assert.IsTrue(_listenerOne.GetAndClearIsInvoked());
    
            _epService.EPRuntime.SendEvent(new SupportBean("e1", 2));
            Assert.IsFalse(_listenerOne.IsInvoked);
            Assert.IsFalse(_listenerTwo.IsInvoked);
        }
    
        [Test]
        public void TestSimpleNoParameter()
        {
            String stmt = "select * from " + typeof(SupportBean).FullName + "(TheString=\"e1\")";
            EPPreparedStatement prepared = _epService.EPAdministrator.PrepareEPL(stmt);
    
            EPStatement statement = _epService.EPAdministrator.Create(prepared);
            statement.Events += _listenerOne.Update;
            Assert.AreEqual("select * from com.espertech.esper.support.bean.SupportBean(TheString=\"e1\")", statement.Text);
    
            statement = _epService.EPAdministrator.Create(prepared);
            statement.Events += _listenerTwo.Update;
            Assert.AreEqual("select * from com.espertech.esper.support.bean.SupportBean(TheString=\"e1\")", statement.Text);
    
            _epService.EPRuntime.SendEvent(new SupportBean("e2", 10));
            Assert.IsFalse(_listenerOne.IsInvoked);
            Assert.IsFalse(_listenerTwo.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("e1", 10));
            Assert.IsTrue(_listenerOne.GetAndClearIsInvoked());
            Assert.IsTrue(_listenerTwo.GetAndClearIsInvoked());
        }
    
        [Test]
        public void TestInvalidParameterNotSet()
        {
            String stmt = "select * from " + typeof(SupportBean).FullName + "(TheString=?)";
            EPPreparedStatement prepared = _epService.EPAdministrator.PrepareEPL(stmt);
    
            try
            {
                _epService.EPAdministrator.Create(prepared);
                Assert.Fail();
            }
            catch (EPException ex)
            {
                Assert.AreEqual("Substitution parameter value for index 1 not set, please provide a value for this parameter", ex.Message);
            }
    
            stmt = "select * from " + typeof(SupportBean).FullName + "(TheString in (?, ?))";
            prepared = _epService.EPAdministrator.PrepareEPL(stmt);
    
            try
            {
                _epService.EPAdministrator.Create(prepared);
                Assert.Fail();
            }
            catch (EPException ex)
            {
                // expected
            }
    
            try
            {
                prepared.SetObject(1, "");
                _epService.EPAdministrator.Create(prepared);
                Assert.Fail();
            }
            catch (EPException ex)
            {
                // expected
            }
    
            // success
            prepared.SetObject(2, "");
            _epService.EPAdministrator.Create(prepared);
        }
    
        [Test]
        public void TestInvalidParameterType()
        {
            String stmt = "select * from " + typeof(SupportBean).FullName + "(TheString=?)";
            EPPreparedStatement prepared = _epService.EPAdministrator.PrepareEPL(stmt);
    
            try
            {
                prepared.SetObject(1, -1);
                _epService.EPAdministrator.Create(prepared);
                Assert.Fail();
            }
            catch (EPException ex)
            {
                Assert.AreEqual(string.Format("Failed to validate filter expression 'TheString=-1': Implicit conversion from datatype '{0}' to 'System.String' is not allowed [select * from com.espertech.esper.support.bean.SupportBean(TheString=-1)]", typeof(int?).FullName), ex.Message);
            }
        }
    
        [Test]
        public void TestInvalidNoParameters()
        {
            String stmt = "select * from " + typeof(SupportBean).FullName + "(TheString='ABC')";
            EPPreparedStatement prepared = _epService.EPAdministrator.PrepareEPL(stmt);
    
            try
            {
                prepared.SetObject(1, -1);
                Assert.Fail();
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Statement does not have substitution parameters indicated by the '?' character", ex.Message);
            }
        }
    
        [Test]
        public void TestInvalidSetObject()
        {
            String stmt = "select * from " + typeof(SupportBean).FullName + "(TheString=?)";
            EPPreparedStatement prepared = _epService.EPAdministrator.PrepareEPL(stmt);
    
            try
            {
                prepared.SetObject(0, "");
                Assert.Fail();
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Substitution parameter index starts at 1", ex.Message);
            }
    
            try
            {
                prepared.SetObject(2, "");
                Assert.Fail();
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Invalid substitution parameter index of 2 supplied, the maximum for this statement is 1", ex.Message);
            }
        }
    
        [Test]
        public void TestInvalidCreateEPL()
        {
            String stmt = "select * from " + typeof(SupportBean).FullName + "(TheString=?)";
            try
            {
                _epService.EPAdministrator.CreateEPL(stmt);
            }
            catch (EPException ex)
            {
                SupportMessageAssertUtil.AssertMessage(ex,
                    "Failed to validate filter expression 'TheString=?': Invalid use of substitution parameters marked by '?' in statement, use the prepare method to prepare statements with substitution parameters");
            }
        }
    
        [Test]
        public void TestInvalidCreatePattern()
        {
            String stmt = typeof(SupportBean).FullName + "(TheString=?)";
            try
            {
                _epService.EPAdministrator.CreatePattern(stmt);
            }
            catch (EPException ex)
            {
                SupportMessageAssertUtil.AssertMessage(ex,
                     "Failed to validate filter expression 'TheString=?': Invalid use of substitution parameters marked by '?' in statement, use the prepare method to prepare statements");
            }
        }
    
        [Test]
        public void TestInvalidCompile()
        {
            String stmt = "select * from " + typeof(SupportBean).FullName + "(TheString=?)";
            try
            {
                _epService.EPAdministrator.CompileEPL(stmt);
            }
            catch (EPException ex)
            {
                Assert.AreEqual("Invalid use of substitution parameters marked by '?' in statement, use the prepare method to prepare statements with substitution parameters", ex.Message);
            }
        }
    
        [Test]
        public void TestInvalidViewParameter()
        {
            String stmt = "select * from " + typeof(SupportBean).FullName + ".win:length(?)";
            try
            {
                _epService.EPAdministrator.PrepareEPL(stmt);
            }
            catch (EPException ex)
            {
                Assert.AreEqual("Incorrect syntax near '?' expecting a closing parenthesis ')' but found a questionmark '?' at line 1 column 70, please check the view specifications within the from clause [select * from com.espertech.esper.support.bean.SupportBean.win:length(?)]", ex.Message);
            }
        }
    
        public interface IKey
        {
        }
    
        [Serializable]
        public class MyObjectKeyInterface : IKey
        {
        }

        [Serializable]
        public class MyEventOne
        {
            private IKey key;
    
            public MyEventOne(IKey key) {
                this.key = key;
            }
    
            public IKey GetKey() {
                return key;
            }
        }

        [Serializable]
        public class MyObjectKeyConcrete
        {
        }

        [Serializable]
        public class MyEventTwo
        {
            private MyObjectKeyConcrete key;
    
            public MyEventTwo(MyObjectKeyConcrete key) {
                this.key = key;
            }
    
            public MyObjectKeyConcrete GetKey() {
                return key;
            }
        }
    }
}
