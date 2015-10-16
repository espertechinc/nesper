///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestFilterInAndBetween
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            _testListener = new SupportUpdateListener();
            _epService = EPServiceProviderManager.GetDefaultProvider(
                SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _testListener = null;
        }

        #endregion

        private EPServiceProvider _epService;
        private SupportUpdateListener _testListener;

        private void TryReuse(String[] statements)
        {
            var testListener = new SupportUpdateListener[statements.Length];
            var stmt = new EPStatement[statements.Length];

            // create all statements
            for (int i = 0; i < statements.Length; i++)
            {
                testListener[i] = new SupportUpdateListener();
                stmt[i] = _epService.EPAdministrator.CreateEPL(statements[i]);
                stmt[i].Events += testListener[i].Update;
            }

            // send event, all should receive the event
            SendBean("IntBoxed", 3);
            for (int i = 0; i < testListener.Length; i++)
            {
                Assert.IsTrue(testListener[i].IsInvoked);
                testListener[i].Reset();
            }

            // stop first, then second, then third etc statement
            for (int toStop = 0; toStop < statements.Length; toStop++)
            {
                stmt[toStop].Stop();

                // send event, all remaining statement received it
                SendBean("IntBoxed", 3);
                for (int i = 0; i <= toStop; i++)
                {
                    Assert.IsFalse(testListener[i].IsInvoked);
                    testListener[i].Reset();
                }
                for (int i = toStop + 1; i < testListener.Length; i++)
                {
                    Assert.IsTrue(testListener[i].IsInvoked);
                    testListener[i].Reset();
                }
            }

            // now all statements are stopped, send event and verify no listener received
            SendBean("IntBoxed", 3);
            for (int i = 0; i < testListener.Length; i++)
            {
                Assert.IsFalse(testListener[i].IsInvoked);
            }
        }

        private void TryExpr(String filterExpr, String fieldName, Object[] values, bool[] isInvoked)
        {
            String expr = "select * from " + typeof (SupportBean).FullName
                          + filterExpr;
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(expr);

            stmt.Events += _testListener.Update;

            for (int i = 0; i < values.Length; i++)
            {
                SendBean(fieldName, values[i]);
                Assert.AreEqual(
                    isInvoked[i],
                    _testListener.IsInvoked,
                    "Listener invocation unexpected for " + filterExpr + " field " + fieldName + "=" + values[i]
                    );
                _testListener.Reset();
            }

            stmt.Stop();
        }

        private void SendBeanInt(int intPrimitive)
        {
            var theEvent = new SupportBean();

            theEvent.IntPrimitive = intPrimitive;
            _epService.EPRuntime.SendEvent(theEvent);
        }

        private void SendBeanString(String value)
        {
            var theEvent = new SupportBean();

            theEvent.TheString = value;
            _epService.EPRuntime.SendEvent(theEvent);
        }

        private void SendBeanNumeric(int intOne, int intTwo)
        {
            var num = new SupportBeanNumeric(intOne, intTwo);

            _epService.EPRuntime.SendEvent(num);
        }

        private void SendBean(String fieldName, Object value)
        {
            var theEvent = new SupportBean();

            if (fieldName.Equals("TheString"))
            {
                theEvent.TheString = (String) value;
            }
            if (fieldName.Equals("BoolPrimitive"))
            {
                theEvent.BoolPrimitive = value.AsBoolean();
            }
            if (fieldName.Equals("IntBoxed"))
            {
                theEvent.IntBoxed = value.AsInt();
            }
            if (fieldName.Equals("LongBoxed"))
            {
                theEvent.LongBoxed = value.AsBoxedLong();
            }
            _epService.EPRuntime.SendEvent(theEvent);
        }

        private void TryInvalid(String expr)
        {
            try
            {
                _epService.EPAdministrator.CreateEPL(expr);
                Assert.Fail();
            }
            catch (EPException ex)
            {
                // expected
            }
        }

        [Test]
        public void TestInDynamic()
        {
            String expr = "select * from pattern [a="
                          + typeof (SupportBeanNumeric).FullName + " -> every b="
                          + typeof (SupportBean).FullName
                          + "(IntPrimitive in (a.intOne, a.intTwo))]";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(expr);

            stmt.Events += _testListener.Update;

            SendBeanNumeric(10, 20);
            SendBeanInt(10);
            Assert.IsTrue(_testListener.GetAndClearIsInvoked());
            SendBeanInt(11);
            Assert.IsFalse(_testListener.GetAndClearIsInvoked());
            SendBeanInt(20);
            Assert.IsTrue(_testListener.GetAndClearIsInvoked());
            stmt.Stop();

            expr = "select * from pattern [a=" + typeof (SupportBean_S0).FullName
                   + " -> every b=" + typeof (SupportBean).FullName
                   + "(TheString in (a.p00, a.p01, a.p02))]";
            stmt = _epService.EPAdministrator.CreateEPL(expr);
            stmt.Events += _testListener.Update;

            _epService.EPRuntime.SendEvent(
                new SupportBean_S0(1, "a", "b", "c", "d"));
            SendBeanString("a");
            Assert.IsTrue(_testListener.GetAndClearIsInvoked());
            SendBeanString("x");
            Assert.IsFalse(_testListener.GetAndClearIsInvoked());
            SendBeanString("b");
            Assert.IsTrue(_testListener.GetAndClearIsInvoked());
            SendBeanString("c");
            Assert.IsTrue(_testListener.GetAndClearIsInvoked());
            SendBeanString("d");
            Assert.IsFalse(_testListener.GetAndClearIsInvoked());
        }

        [Test]
        public void TestInExpr()
        {
            TryExpr("(TheString > 'b')", "TheString", new String[]
            {
                "a", "b", "c", "d"
            }
                    , new bool[]
                    {
                        false, false, true, true
                    }
                );
            TryExpr("(TheString < 'b')", "TheString", new String[]
            {
                "a", "b", "c", "d"
            }
                    , new bool[]
                    {
                        true, false, false, false
                    }
                );
            TryExpr("(TheString >= 'b')", "TheString", new String[]
            {
                "a", "b", "c", "d"
            }
                    , new bool[]
                    {
                        false, true, true, true
                    }
                );
            TryExpr("(TheString <= 'b')", "TheString", new String[]
            {
                "a", "b", "c", "d"
            }
                    , new bool[]
                    {
                        true, true, false, false
                    }
                );
            TryExpr("(TheString in ['b':'d'])", "TheString", new String[]
            {
                "a", "b", "c", "d", "e"
            }
                    , new bool[]
                    {
                        false, true, true, true, false
                    }
                );
            TryExpr("(TheString in ('b':'d'])", "TheString", new String[]
            {
                "a", "b", "c", "d", "e"
            }
                    , new bool[]
                    {
                        false, false, true, true, false
                    }
                );
            TryExpr("(TheString in ['b':'d'))", "TheString", new String[]
            {
                "a", "b", "c", "d", "e"
            }
                    , new bool[]
                    {
                        false, true, true, false, false
                    }
                );
            TryExpr("(TheString in ('b':'d'))", "TheString", new String[]
            {
                "a", "b", "c", "d", "e"
            }
                    , new bool[]
                    {
                        false, false, true, false, false
                    }
                );
            TryExpr("(BoolPrimitive in (false))", "BoolPrimitive", new Object[]
            {
                true, false
            }
                    , new bool[]
                    {
                        false, true
                    }
                );
            TryExpr("(BoolPrimitive in (false, false, false))", "BoolPrimitive",
                    new Object[]
                    {
                        true, false
                    }
                    , new bool[]
                    {
                        false, true
                    }
                );
            TryExpr("(BoolPrimitive in (false, true, false))", "BoolPrimitive",
                    new Object[]
                    {
                        true, false
                    }
                    , new bool[]
                    {
                        true, true
                    }
                );
            TryExpr("(IntBoxed in (4, 6, 1))", "IntBoxed", new Object[]
            {
                0, 1, 2, 3, 4, 5, 6
            }
                    , new bool[]
                    {
                        false, true, false, false, true, false, true
                    }
                );
            TryExpr("(IntBoxed in (3))", "IntBoxed", new Object[]
            {
                0, 1, 2, 3, 4, 5, 6
            }
                    , new bool[]
                    {
                        false, false, false, true, false, false, false
                    }
                );
            TryExpr("(LongBoxed in (3))", "LongBoxed", new Object[]
            {
                0L, 1L, 2L, 3L, 4L, 5L, 6L
            }
                    , new bool[]
                    {
                        false, false, false, true, false, false, false
                    }
                );
            TryExpr("(IntBoxed between 4 and 6)", "IntBoxed", new Object[]
            {
                0, 1, 2, 3, 4, 5, 6
            }
                    , new bool[]
                    {
                        false, false, false, false, true, true, true
                    }
                );
            TryExpr("(IntBoxed between 2 and 1)", "IntBoxed", new Object[]
            {
                0, 1, 2, 3, 4, 5, 6
            }
                    , new bool[]
                    {
                        false, true, true, false, false, false, false
                    }
                );
            TryExpr("(IntBoxed between 4 and -1)", "IntBoxed", new Object[]
            {
                0, 1, 2, 3, 4, 5, 6
            }
                    , new bool[]
                    {
                        true, true, true, true, true, false, false
                    }
                );
            TryExpr("(IntBoxed in [2:4])", "IntBoxed", new Object[]
            {
                0, 1, 2, 3, 4, 5, 6
            }
                    , new bool[]
                    {
                        false, false, true, true, true, false, false
                    }
                );
            TryExpr("(IntBoxed in (2:4])", "IntBoxed", new Object[]
            {
                0, 1, 2, 3, 4, 5, 6
            }
                    , new bool[]
                    {
                        false, false, false, true, true, false, false
                    }
                );
            TryExpr("(IntBoxed in [2:4))", "IntBoxed", new Object[]
            {
                0, 1, 2, 3, 4, 5, 6
            }
                    , new bool[]
                    {
                        false, false, true, true, false, false, false
                    }
                );
            TryExpr("(IntBoxed in (2:4))", "IntBoxed", new Object[]
            {
                0, 1, 2, 3, 4, 5, 6
            }
                    , new bool[]
                    {
                        false, false, false, true, false, false, false
                    }
                );
        }

        [Test]
        public void TestInvalid()
        {
            // we do not coerce
            TryInvalid(
                "select * from " + typeof (SupportBean).FullName
                + "(IntPrimitive in (1L, 10L))");
            TryInvalid(
                "select * from " + typeof (SupportBean).FullName
                + "(IntPrimitive in (1, 10L))");
            TryInvalid(
                "select * from " + typeof (SupportBean).FullName
                + "(IntPrimitive in (1, 'x'))");

            String expr = "select * from pattern [a=" + typeof (SupportBean).FullName
                          + " -> b=" + typeof (SupportBean).FullName
                          + "(IntPrimitive in (a.LongPrimitive, a.LongBoxed))]";

            TryInvalid(expr);
        }

        [Test]
        public void TestNotInExpr()
        {
            TryExpr("(IntBoxed not between 4 and 6)", "IntBoxed", new Object[]
            {
                0, 1, 2, 3, 4, 5, 6
            }
                    , new bool[]
                    {
                        true, true, true, true, false, false, false
                    }
                );
            TryExpr("(IntBoxed not between 2 and 1)", "IntBoxed", new Object[]
            {
                0, 1, 2, 3, 4, 5, 6
            }
                    , new bool[]
                    {
                        true, false, false, true, true, true, true
                    }
                );
            TryExpr("(IntBoxed not between 4 and -1)", "IntBoxed", new Object[]
            {
                0, 1, 2, 3, 4, 5, 6
            }
                    , new bool[]
                    {
                        false, false, false, false, false, true, true
                    }
                );
            TryExpr("(IntBoxed not in [2:4])", "IntBoxed", new Object[]
            {
                0, 1, 2, 3, 4, 5, 6
            }
                    , new bool[]
                    {
                        true, true, false, false, false, true, true
                    }
                );
            TryExpr("(IntBoxed not in (2:4])", "IntBoxed", new Object[]
            {
                0, 1, 2, 3, 4, 5, 6
            }
                    , new bool[]
                    {
                        true, true, true, false, false, true, true
                    }
                );
            TryExpr("(IntBoxed not in [2:4))", "IntBoxed", new Object[]
            {
                0, 1, 2, 3, 4, 5, 6
            }
                    , new bool[]
                    {
                        true, true, false, false, true, true, true
                    }
                );
            TryExpr("(IntBoxed not in (2:4))", "IntBoxed", new Object[]
            {
                0, 1, 2, 3, 4, 5, 6
            }
                    , new bool[]
                    {
                        true, true, true, false, true, true, true
                    }
                );
            TryExpr("(TheString not in ['b':'d'])", "TheString", new String[]
            {
                "a", "b", "c", "d", "e"
            }
                    , new bool[]
                    {
                        true, false, false, false, true
                    }
                );
            TryExpr("(TheString not in ('b':'d'])", "TheString", new String[]
            {
                "a", "b", "c", "d", "e"
            }
                    , new bool[]
                    {
                        true, true, false, false, true
                    }
                );
            TryExpr("(TheString not in ['b':'d'))", "TheString", new String[]
            {
                "a", "b", "c", "d", "e"
            }
                    , new bool[]
                    {
                        true, false, false, true, true
                    }
                );
            TryExpr("(TheString not in ('b':'d'))", "TheString", new String[]
            {
                "a", "b", "c", "d", "e"
            }
                    , new bool[]
                    {
                        true, true, false, true, true
                    }
                );
            TryExpr("(TheString not in ('a', 'b'))", "TheString", new String[]
            {
                "a", "x", "b", "y"
            }
                    , new bool[]
                    {
                        false, true, false, true
                    }
                );
            TryExpr("(BoolPrimitive not in (false))", "BoolPrimitive", new Object[]
            {
                true, false
            }
                    , new bool[]
                    {
                        true, false
                    }
                );
            TryExpr("(BoolPrimitive not in (false, false, false))", "BoolPrimitive",
                    new Object[]
                    {
                        true, false
                    }
                    , new bool[]
                    {
                        true, false
                    }
                );
            TryExpr("(BoolPrimitive not in (false, true, false))", "BoolPrimitive",
                    new Object[]
                    {
                        true, false
                    }
                    , new bool[]
                    {
                        false, false
                    }
                );
            TryExpr("(IntBoxed not in (4, 6, 1))", "IntBoxed", new Object[]
            {
                0, 1, 2, 3, 4, 5, 6
            }
                    , new bool[]
                    {
                        true, false, true, true, false, true, false
                    }
                );
            TryExpr("(IntBoxed not in (3))", "IntBoxed", new Object[]
            {
                0, 1, 2, 3, 4, 5, 6
            }
                    , new bool[]
                    {
                        true, true, true, false, true, true, true
                    }
                );
            TryExpr("(LongBoxed not in (3))", "LongBoxed", new Object[]
            {
                0L, 1L, 2L, 3L, 4L, 5L, 6L
            }
                    , new bool[]
                    {
                        true, true, true, false, true, true, true
                    }
                );
        }

        [Test]
        public void TestReuse()
        {
            String expr = "select * from " + typeof (SupportBean).FullName
                          + "(IntBoxed in [2:4])";

            TryReuse(new String[]
            {
                expr, expr
            }
                );

            expr = "select * from " + typeof (SupportBean).FullName
                   + "(IntBoxed in (1, 2, 3))";
            TryReuse(new String[]
            {
                expr, expr
            }
                );

            String exprOne = "select * from " + typeof (SupportBean).FullName
                             + "(IntBoxed in (2:3])";
            String exprTwo = "select * from " + typeof (SupportBean).FullName
                             + "(IntBoxed in (1:3])";

            TryReuse(new String[]
            {
                exprOne, exprTwo
            }
                );

            exprOne = "select * from " + typeof (SupportBean).FullName
                      + "(IntBoxed in (2, 3, 4))";
            exprTwo = "select * from " + typeof (SupportBean).FullName
                      + "(IntBoxed in (1, 3))";
            TryReuse(new String[]
            {
                exprOne, exprTwo
            }
                );

            exprOne = "select * from " + typeof (SupportBean).FullName
                      + "(IntBoxed in (2, 3, 4))";
            exprTwo = "select * from " + typeof (SupportBean).FullName
                      + "(IntBoxed in (1, 3))";
            String exprThree = "select * from " + typeof (SupportBean).FullName
                               + "(IntBoxed in (8, 3))";

            TryReuse(new String[]
            {
                exprOne, exprTwo, exprThree
            }
                );

            exprOne = "select * from " + typeof (SupportBean).FullName
                      + "(IntBoxed in (3, 1, 3))";
            exprTwo = "select * from " + typeof (SupportBean).FullName
                      + "(IntBoxed in (3, 3))";
            exprThree = "select * from " + typeof (SupportBean).FullName
                        + "(IntBoxed in (1, 3))";
            TryReuse(new String[]
            {
                exprOne, exprTwo, exprThree
            }
                );

            exprOne = "select * from " + typeof (SupportBean).FullName
                      + "(BoolPrimitive=false, IntBoxed in (1, 2, 3))";
            exprTwo = "select * from " + typeof (SupportBean).FullName
                      + "(BoolPrimitive=false, IntBoxed in (3, 4))";
            exprThree = "select * from " + typeof (SupportBean).FullName
                        + "(BoolPrimitive=false, IntBoxed in (3))";
            TryReuse(new String[]
            {
                exprOne, exprTwo, exprThree
            }
                );

            exprOne = "select * from " + typeof (SupportBean).FullName
                      + "(IntBoxed in (1, 2, 3), LongPrimitive >= 0)";
            exprTwo = "select * from " + typeof (SupportBean).FullName
                      + "(IntBoxed in (3, 4), IntPrimitive >= 0)";
            exprThree = "select * from " + typeof (SupportBean).FullName
                        + "(IntBoxed in (3), BytePrimitive < 1)";
            TryReuse(new String[]
            {
                exprOne, exprTwo, exprThree
            }
                );
        }

        [Test]
        public void TestReuseNot()
        {
            String expr = "select * from " + typeof (SupportBean).FullName
                          + "(IntBoxed not in [1:2])";

            TryReuse(new String[]
            {
                expr, expr
            }
                );

            String exprOne = "select * from " + typeof (SupportBean).FullName
                             + "(IntBoxed in (3, 1, 3))";
            String exprTwo = "select * from " + typeof (SupportBean).FullName
                             + "(IntBoxed not in (2, 1))";
            String exprThree = "select * from " + typeof (SupportBean).FullName
                               + "(IntBoxed not between 0 and -3)";

            TryReuse(new String[]
            {
                exprOne, exprTwo, exprThree
            }
                );

            exprOne = "select * from " + typeof (SupportBean).FullName
                      + "(IntBoxed not in (1, 4, 5))";
            exprTwo = "select * from " + typeof (SupportBean).FullName
                      + "(IntBoxed not in (1, 4, 5))";
            exprThree = "select * from " + typeof (SupportBean).FullName
                        + "(IntBoxed not in (4, 5, 1))";
            TryReuse(new String[]
            {
                exprOne, exprTwo, exprThree
            }
                );

            exprOne = "select * from " + typeof (SupportBean).FullName
                      + "(IntBoxed not in (3:4))";
            exprTwo = "select * from " + typeof (SupportBean).FullName
                      + "(IntBoxed not in [1:3))";
            exprThree = "select * from " + typeof (SupportBean).FullName
                        + "(IntBoxed not in (1,1,1,33))";
            TryReuse(new String[]
            {
                exprOne, exprTwo, exprThree
            }
                );
        }

        [Test]
        public void TestSimpleIntAndEnumWrite()
        {
            String expr = "select * from " + typeof (SupportBean).FullName
                          + "(IntPrimitive in (1, 10))";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(expr);

            stmt.Events += _testListener.Update;

            SendBeanInt(10);
            Assert.IsTrue(_testListener.GetAndClearIsInvoked());
            SendBeanInt(11);
            Assert.IsFalse(_testListener.GetAndClearIsInvoked());
            SendBeanInt(1);
            Assert.IsTrue(_testListener.GetAndClearIsInvoked());
            stmt.Dispose();

            // try enum - ESPER-459
            ICollection<SupportEnum> types = new HashSet<SupportEnum>();

            types.Add(SupportEnum.ENUM_VALUE_2);
            EPPreparedStatement inPstmt = _epService.EPAdministrator.PrepareEPL(
                string.Format("select * from {0} ev where ev.EnumValue in (?)", typeof (SupportBean).FullName));

            inPstmt.SetObject(1, types);

            EPStatement inStmt = _epService.EPAdministrator.Create(inPstmt);

            inStmt.Events += _testListener.Update;

            var theEvent = new SupportBean();

            theEvent.EnumValue = SupportEnum.ENUM_VALUE_2;
            _epService.EPRuntime.SendEvent(theEvent);

            Assert.IsTrue(_testListener.IsInvoked);
        }
    }
}
