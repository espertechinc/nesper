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
using com.espertech.esper.client.soda;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.expr
{
    [TestFixture]
    public class TestInBetweenLikeExpr
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            _listener = new SupportUpdateListener();
            _testListenerTwo = new SupportUpdateListener();
            _epService = EPServiceProviderManager.GetDefaultProvider(
                SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
            _testListenerTwo = null;
        }

        #endregion

        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
        private SupportUpdateListener _testListenerTwo;

        private void SendAndAssert(int? intBoxed, float? floatBoxed, double doublePrimitive, bool result)
        {
            var bean = new SupportBean();

            bean.IntBoxed = intBoxed;
            bean.FloatBoxed = floatBoxed;
            bean.DoublePrimitive = doublePrimitive;

            _epService.EPRuntime.SendEvent(bean);

            EventBean theEvent = _listener.AssertOneGetNewAndReset();

            Assert.AreEqual(result, theEvent.Get("result"));
        }

        private void SendAndAssert(int intPrimitive, int shortBoxed, int? intBoxed, long? longBoxed, bool result)
        {
            var bean = new SupportBean();

            bean.IntPrimitive = intPrimitive;
            bean.ShortBoxed = (short) shortBoxed;
            bean.IntBoxed = intBoxed;
            bean.LongBoxed = longBoxed;

            _epService.EPRuntime.SendEvent(bean);

            EventBean theEvent = _listener.AssertOneGetNewAndReset();

            Assert.AreEqual(result, theEvent.Get("result"));
        }


        private void SendAndAssert(int intPrimitive, int shortBoxed, long longBoxed, bool boolBoxed)
        {
            SendAndAssert(intPrimitive, shortBoxed, longBoxed, (bool?) boolBoxed);
        }

        private void SendAndAssert(int intPrimitive, int shortBoxed, long? longBoxed, bool? result)
        {
            var bean = new SupportBean();

            bean.IntPrimitive = intPrimitive;
            bean.ShortBoxed = (short) shortBoxed;
            bean.LongBoxed = longBoxed;

            _epService.EPRuntime.SendEvent(bean);

            EventBean theEvent = _listener.AssertOneGetNewAndReset();

            Assert.AreEqual(result, theEvent.Get("result"));
        }

        private void SendAndAssert(int? intBoxed,
                                   float? floatBoxed,
                                   double doublePrimitve,
                                   long? longBoxed,
                                   bool? result)
        {
            var bean = new SupportBean();

            bean.IntBoxed = intBoxed;
            bean.FloatBoxed = floatBoxed;
            bean.DoublePrimitive = doublePrimitve;
            bean.LongBoxed = longBoxed;

            _epService.EPRuntime.SendEvent(bean);

            EventBean theEvent = _listener.AssertOneGetNewAndReset();

            Assert.AreEqual(result, theEvent.Get("result"));
        }

        private void TryInBoolean(String expr, bool?[] input, bool?[] result)
        {
            String caseExpr = "select " + expr + " as result from "
                              + typeof (SupportBean).FullName;

            EPStatement selectTestCase = _epService.EPAdministrator.CreateEPL(
                caseExpr);

            selectTestCase.Events += _listener.Update;
            Assert.AreEqual(
                typeof (bool?),
                selectTestCase.EventType.GetPropertyType("result"));

            for (int i = 0; i < input.Length; i++)
            {
                SendSupportBeanEvent(input[i]);
                EventBean theEvent = _listener.AssertOneGetNewAndReset();

                Assert.AreEqual(result[i], theEvent.Get("result"), "Wrong result for " + input[i]);
            }
            selectTestCase.Stop();
        }

        private void TryNumeric(String expr, double?[] input, bool?[] result)
        {
            String caseExpr = "select " + expr + " as result from "
                              + typeof (SupportBean).FullName;

            EPStatement selectTestCase = _epService.EPAdministrator.CreateEPL(
                caseExpr);

            selectTestCase.Events += _listener.Update;
            Assert.AreEqual(
                typeof (bool?),
                selectTestCase.EventType.GetPropertyType("result"));

            for (int i = 0; i < input.Length; i++)
            {
                SendSupportBeanEvent(input[i]);
                EventBean theEvent = _listener.AssertOneGetNewAndReset();

                Assert.AreEqual(result[i], theEvent.Get("result"), "Wrong result for " + input[i]);
            }
            selectTestCase.Stop();
        }

        private void TryString(String expression, String[] input, bool?[] result)
        {
            String caseExpr = "select " + expression + " as result from "
                              + typeof (SupportBean).FullName;

            EPStatement selectTestCase = _epService.EPAdministrator.CreateEPL(
                caseExpr);

            selectTestCase.Events += _listener.Update;
            Assert.AreEqual(
                typeof (bool?),
                selectTestCase.EventType.GetPropertyType("result"));

            for (int i = 0; i < input.Length; i++)
            {
                SendSupportBeanEvent(input[i]);
                EventBean theEvent = _listener.AssertOneGetNewAndReset();

                Assert.AreEqual(result[i], theEvent.Get("result"), "Wrong result for " + input[i]);
            }
            selectTestCase.Stop();
        }

        private void TryString(EPStatementObjectModel model, String epl, String[] input, bool?[] result)
        {
            EPStatement selectTestCase = _epService.EPAdministrator.Create(model);

            Assert.AreEqual(epl, model.ToEPL());

            EPStatementObjectModel compiled = _epService.EPAdministrator.CompileEPL(
                epl);

            compiled = (EPStatementObjectModel) SerializableObjectCopier.Copy(
                compiled);
            Assert.AreEqual(epl, compiled.ToEPL());

            selectTestCase.Events += _listener.Update;
            Assert.AreEqual(
                typeof (bool?),
                selectTestCase.EventType.GetPropertyType("result"));

            for (int i = 0; i < input.Length; i++)
            {
                SendSupportBeanEvent(input[i]);
                EventBean theEvent = _listener.AssertOneGetNewAndReset();

                Assert.AreEqual(result[i], theEvent.Get("result"), "Wrong result for " + input[i]);
            }
            selectTestCase.Stop();
        }

        private void SendSupportBeanEvent(double? doubleBoxed)
        {
            var theEvent = new SupportBean();

            theEvent.DoubleBoxed = doubleBoxed;
            _epService.EPRuntime.SendEvent(theEvent);
        }

        private void SendSupportBeanEvent(String theString)
        {
            var theEvent = new SupportBean();

            theEvent.TheString = theString;
            _epService.EPRuntime.SendEvent(theEvent);
        }

        private void SendSupportBeanEvent(bool? boolBoxed)
        {
            var theEvent = new SupportBean();

            theEvent.BoolBoxed = boolBoxed;
            _epService.EPRuntime.SendEvent(theEvent);
        }

        [Test]
        public void TestBetweenNumericCoercionDouble()
        {
            String caseExpr = "select IntBoxed between FloatBoxed and DoublePrimitive as result from "
                              + typeof (SupportBean).FullName;

            EPStatement selectTestCase = _epService.EPAdministrator.CreateEPL(caseExpr);

            selectTestCase.Events += _listener.Update;
            Assert.AreEqual(typeof (bool?), selectTestCase.EventType.GetPropertyType("result"));

            SendAndAssert(1, 2f, 3d, false);
            SendAndAssert(2, 2f, 3d, true);
            SendAndAssert(3, 2f, 3d, true);
            SendAndAssert(4, 2f, 3d, false);
            SendAndAssert(null, 2f, 3d, false);
            SendAndAssert(null, null, 3d, false);
            SendAndAssert(1, 3f, 2d, false);
            SendAndAssert(2, 3f, 2d, true);
            SendAndAssert(3, 3f, 2d, true);
            SendAndAssert(4, 3f, 2d, false);
            SendAndAssert(null, 3f, 2d, false);
            SendAndAssert(null, null, 2d, false);

            selectTestCase.Stop();
        }

        [Test]
        public void TestBetweenNumericCoercionLong()
        {
            String caseExpr = "select IntPrimitive between ShortBoxed and LongBoxed as result from "
                              + typeof (SupportBean).FullName;

            EPStatement selectTestCase = _epService.EPAdministrator.CreateEPL(
                caseExpr);

            selectTestCase.Events += _listener.Update;
            Assert.AreEqual(
                typeof (bool?),
                selectTestCase.EventType.GetPropertyType("result"));

            SendAndAssert(1, 2, 3l, false);
            SendAndAssert(2, 2, 3l, true);
            SendAndAssert(3, 2, 3l, true);
            SendAndAssert(4, 2, 3l, false);
            SendAndAssert(5, 10, 1L, true);
            SendAndAssert(1, 10, 1L, true);
            SendAndAssert(10, 10, 1L, true);
            SendAndAssert(11, 10, 1L, false);

            selectTestCase.Stop();
        }

        [Test]
        public void TestBetweenNumericExpr()
        {
            var input = new double?[]
            {
                1d, null, 1.1d, 2d, 1.0999999999, 2d, 4d, 15d, 15.00001d
            }
                ;
            var result = new bool?[]
            {
                false, false, true, true, false, true, true, true, false
            }
                ;

            TryNumeric("DoubleBoxed between 1.1 and 15", input, result);
            TryNumeric("DoubleBoxed between 15 and 1.1", input, result);

            TryNumeric(
                "DoubleBoxed between null and 15", new double?[]
                {
                    1d, null, 1.1d
                }
                , new bool?[]
                {
                    false, false, false
                }
                );

            TryNumeric(
                "DoubleBoxed between 15 and null", new double?[]
                {
                    1d, null, 1.1d
                }
                , new bool?[]
                {
                    false, false, false
                }
                );

            TryNumeric(
                "DoubleBoxed between null and null", new double?[]
                {
                    1d, null, 1.1d
                }
                , new bool?[]
                {
                    false, false, false
                }
                );

            input = new double?[]
            {
                1d, null, 1.1d, 2d, 1.0999999999, 2d, 4d, 15d, 15.00001d
            }
                ;
            result = new bool?[]
            {
                true, false, false, false, true, false, false, false, true
            }
                ;
            TryNumeric("DoubleBoxed not between 1.1 and 15", input, result);
            TryNumeric("DoubleBoxed not between 15 and 1.1", input, result);

            TryNumeric(
                "DoubleBoxed not between 15 and null", new double?[]
                {
                    1d, null, 1.1d
                }
                , new bool?[]
                {
                    false, false, false
                }
                );
        }

        [Test]
        public void TestBetweenStringExpr()
        {
            string[] input = null;
            bool?[] result = null;

            input = new String[]
            {
                "0", "a1", "a10", "c", "d", null, "a0", "b9", "b90"
            }
                ;
            result = new bool?[]
            {
                false, true, true, false, false, false, true, true, false
            }
                ;
            TryString("TheString between 'a0' and 'b9'", input, result);
            TryString("TheString between 'b9' and 'a0'", input, result);

            TryString(
                "TheString between null and 'b9'", new String[]
                {
                    "0", null, "a0", "b9"
                }
                , new bool?[]
                {
                    false, false, false, false
                }
                );

            TryString(
                "TheString between null and null", new String[]
                {
                    "0", null, "a0", "b9"
                }
                , new bool?[]
                {
                    false, false, false, false
                }
                );

            TryString(
                "TheString between 'a0' and null", new String[]
                {
                    "0", null, "a0", "b9"
                }
                , new bool?[]
                {
                    false, false, false, false
                }
                );

            input = new String[]
            {
                "0", "a1", "a10", "c", "d", null, "a0", "b9", "b90"
            }
                ;
            result = new bool?[]
            {
                true, false, false, true, true, false, false, false, true
            }
                ;
            TryString("TheString not between 'a0' and 'b9'", input, result);
            TryString("TheString not between 'b9' and 'a0'", input, result);
        }

        [Test]
        public void TestInArraySubstitution()
        {
            _epService.EPAdministrator.Configuration.AddEventType(
                "SupportBean", typeof (SupportBean));
            String stmtText = "select IntPrimitive in (?) as result from SupportBean";
            EPPreparedStatement prepared = _epService.EPAdministrator.PrepareEPL(
                stmtText);

            prepared.SetObject(
                1, new int[]
                {
                    10, 20, 30
                }
                );
            EPStatement stmt = _epService.EPAdministrator.Create(prepared);

            stmt.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            Assert.IsTrue((bool) _listener.AssertOneGetNewAndReset().Get("result"));

            _epService.EPRuntime.SendEvent(new SupportBean("E2", 9));
            Assert.IsFalse((bool) _listener.AssertOneGetNewAndReset().Get("result"));
        }

        [Test]
        public void TestInBoolExpr()
        {
            TryInBoolean(
                "BoolBoxed in (true, true)",
                new bool?[]
                {
                    true, false
                }
                , new bool?[]
                {
                    true, false
                }
                );

            TryInBoolean(
                "BoolBoxed in (1>2, 2=3, 4<=2)",
                new bool?[]
                {
                    true, false
                }
                , new bool?[]
                {
                    false, true
                }
                );

            TryInBoolean(
                "BoolBoxed not in (1>2, 2=3, 4<=2)",
                new bool?[]
                {
                    true, false
                }
                , new bool?[]
                {
                    true, false
                }
                );
        }

        [Test]
        public void TestInCollection()
        {
            String stmtText = "select 10 in (arrayProperty) as result from "
                              + typeof (SupportBeanComplexProps).FullName;
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);

            stmt.Events += _listener.Update;
            Assert.AreEqual(typeof (bool?), stmt.EventType.GetPropertyType("result"));

            stmtText = "select 5 in (arrayProperty) as result from "
                       + typeof (SupportBeanComplexProps).FullName;
            EPStatement selectTestCaseTwo = _epService.EPAdministrator.CreateEPL(stmtText);

            selectTestCaseTwo.Events += _testListenerTwo.Update;

            _epService.EPRuntime.SendEvent(
                SupportBeanComplexProps.MakeDefaultBean());
            Assert.AreEqual(true, _listener.AssertOneGetNewAndReset().Get("result"));
            Assert.AreEqual(false, _testListenerTwo.AssertOneGetNewAndReset().Get("result"));

            stmt.Stop();
            selectTestCaseTwo.Stop();

            // Arrays
            stmtText = "select 1 in (IntArr, LongArr) as resOne, 1 not in (IntArr, LongArr) as resTwo from "
                       + typeof (SupportBeanArrayCollMap).FullName;
            stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;

            String[] fields = "resOne, resTwo".Split(',');

            _epService.EPRuntime.SendEvent(new SupportBeanArrayCollMap(new int[] { 10, 20, 30 }));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { false, true });
            _epService.EPRuntime.SendEvent(new SupportBeanArrayCollMap(new int[] { 10, 1, 30 }));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { true, false });
            _epService.EPRuntime.SendEvent(new SupportBeanArrayCollMap(new int[] { 30 }, new long?[] { 20L, 1L }));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { true, false });
            _epService.EPRuntime.SendEvent(new SupportBeanArrayCollMap( new int[] { }, new long?[] { null, 1L }));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { true, false });
            _epService.EPRuntime.SendEvent(new SupportBeanArrayCollMap( null, new long?[] { 1L, 100L }));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { true, false });
            _epService.EPRuntime.SendEvent(new SupportBeanArrayCollMap(null, new long?[]{ 0L, 100L }));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { false, true });
            stmt.Dispose();

            // Collection
            stmtText = "select 1 in (IntCol, LongCol) as resOne, 1 not in (LongCol, IntCol) as resTwo from "
                       + typeof (SupportBeanArrayCollMap).FullName;
            stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBeanArrayCollMap(true, new int[]{ 10, 20, 30 }, null));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { false, true });
            _epService.EPRuntime.SendEvent(new SupportBeanArrayCollMap(true, new int[] { 10, 20, 1 } , null));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { true, false });
            _epService.EPRuntime.SendEvent(new SupportBeanArrayCollMap(true, new int[] { 30 }, new long?[] { 20L, 1L }));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { true, false });
            _epService.EPRuntime.SendEvent(new SupportBeanArrayCollMap(true, new int[] { }, new long?[] { null, 1L }));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { true, false });
            _epService.EPRuntime.SendEvent(new SupportBeanArrayCollMap(true, null, new long?[] { 1L, 100L }));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { true, false });
            stmt.Dispose();

            // Maps
            stmtText = "select 1 in (LongMap, IntMap) as resOne, 1 not in (LongMap, IntMap) as resTwo from "
                       + typeof (SupportBeanArrayCollMap).FullName;
            stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBeanArrayCollMap(false, new int[] { 10, 20, 30 }, null));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { false, true });
            _epService.EPRuntime.SendEvent(new SupportBeanArrayCollMap(false, new int[] { 10, 20, 1 }, null));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { true, false });
            _epService.EPRuntime.SendEvent(new SupportBeanArrayCollMap(false, new int[] { 30 }, new long?[] { 20L, 1L }));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { true, false });
            _epService.EPRuntime.SendEvent(new SupportBeanArrayCollMap(false, new int[] { }, new long?[] { null, 1L }));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { true, false });
            _epService.EPRuntime.SendEvent(new SupportBeanArrayCollMap(false, null, new long?[] { 1L, 100L }));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { true, false });
            stmt.Dispose();

            // Mixed
            stmtText = "select 1 in (LongBoxed, IntArr, LongMap, IntCol) as resOne, 1 not in (LongBoxed, IntArr, LongMap, IntCol) as resTwo from "
                       + typeof (SupportBeanArrayCollMap).FullName;
            stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBeanArrayCollMap(1L, new int[0], new long?[0], new int[0]));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { true, false });
            _epService.EPRuntime.SendEvent(new SupportBeanArrayCollMap(2L, null, new long?[0], new int[0]));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { false, true });
            _epService.EPRuntime.SendEvent(new SupportBeanArrayCollMap(null, null, null, new int[] { 3, 4, 5, 6, 7, 7, 7, 8, 8, 8, 1}));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { true, false });
            _epService.EPRuntime.SendEvent(new SupportBeanArrayCollMap( -1L, null, new long?[] { 1L }, new int[] { 3, 4, 5, 6, 7, 7, 7, 8, 8 }));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { true, false }); // NEsper does type introspection & conversion
            _epService.EPRuntime.SendEvent(new SupportBeanArrayCollMap(-1L, new int[] { 1 }, null, new int[] { }));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { true, false });
            stmt.Dispose();

            // Object array
            stmtText = "select 1 in (objectArr) as resOne, 2 in (objectArr) as resTwo from "
                       + typeof (SupportBeanArrayCollMap).FullName;
            stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBeanArrayCollMap(new Object[] {}));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { false, false });
            _epService.EPRuntime.SendEvent(new SupportBeanArrayCollMap(new Object[] { 1, 2 }));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { true, true });
            _epService.EPRuntime.SendEvent(new SupportBeanArrayCollMap(new Object[] { 1d, 2L }));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { false, false });
            _epService.EPRuntime.SendEvent(new SupportBeanArrayCollMap(new Object[] { null, 2 }));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { null, true });
            stmt.Dispose();

            // Object array
            stmtText = "select 1 in ({1,2,3}) as resOne, 2 in ({0, 1}) as resTwo from "
                       + typeof (SupportBeanArrayCollMap).FullName;
            stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBeanArrayCollMap(new Object[] { }));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { true, false });
        }

        [Test]
        public void TestInNumericCoercionDouble()
        {
            String caseExpr = "select IntBoxed in (FloatBoxed, DoublePrimitive, LongBoxed) as result from "
                              + typeof (SupportBean).FullName;

            EPStatement selectTestCase = _epService.EPAdministrator.CreateEPL(
                caseExpr);

            selectTestCase.Events += _listener.Update;
            Assert.AreEqual(
                typeof (bool?),
                selectTestCase.EventType.GetPropertyType("result"));

            SendAndAssert(1, 2f, 3d, 4L, false);
            SendAndAssert(1, 1f, 3d, 4L, true);
            SendAndAssert(1, 1.1f, 1.0d, 4L, true);
            SendAndAssert(1, 1.1f, 1.2d, 1L, true);
            SendAndAssert(1, null, 1.2d, 1L, true);
            SendAndAssert(null, null, 1.2d, 1L, null);
            SendAndAssert(null, 11f, 1.2d, 1L, null);

            selectTestCase.Stop();
        }

        [Test]
        public void TestInNumericCoercionLong()
        {
            String caseExpr = "select IntPrimitive in (ShortBoxed, IntBoxed, LongBoxed) as result from "
                              + typeof (SupportBean).FullName;

            EPStatement selectTestCase = _epService.EPAdministrator.CreateEPL(
                caseExpr);

            selectTestCase.Events += _listener.Update;
            Assert.AreEqual(
                typeof (bool?),
                selectTestCase.EventType.GetPropertyType("result"));

            SendAndAssert(1, 2, 3, 4L, false);
            SendAndAssert(1, 1, 3, 4L, true);
            SendAndAssert(1, 3, 1, 4L, true);
            SendAndAssert(1, 3, 7, 1L, true);
            SendAndAssert(1, 3, 7, null, null);
            SendAndAssert(1, 1, null, null, true);
            SendAndAssert(1, 0, null, 1L, true);

            selectTestCase.Stop();
        }

        [Test]
        public void TestInNumericExpr()
        {
            var input = new double?[]
            {
                1d, null, 1.1d, 1.0d, 1.0999999999, 2d, 4d
            };
            var result = new bool?[]
            {
                false, null, true, false, false, true, true
            };

            TryNumeric("DoubleBoxed in (1.1d, 7/3.5, 2*6/3, 0)", input, result);

            TryNumeric(
                "DoubleBoxed in (7/3d, null)",
                new double?[]
                {
                    2d, 7/3d, null
                }
                , new bool?[]
                {
                    null, true, null
                }
                );

            TryNumeric(
                "DoubleBoxed in (5,5,5,5,5, -1)",
                new double?[]
                {
                    5.0, 5d, 0d, null, -1d
                }
                , new bool?[]
                {
                    true, true, false, null, true
                }
                );

            TryNumeric(
                "DoubleBoxed not in (1.1d, 7/3.5, 2*6/3, 0)",
                new double?[]
                {
                    1d, null, 1.1d, 1.0d, 1.0999999999, 2d, 4d
                }
                , new bool?[]
                {
                    true, null, false, true, true, false, false
                }
                );
        }

        [Test]
        public void TestInObject()
        {
            _epService.EPAdministrator.Configuration.AddEventType(
                "ArrayBean", typeof (SupportBeanArrayCollMap));
            String stmtText = "select s0.anyObject in (objectArr) as value from ArrayBean s0";

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);

            stmt.Events += _listener.Update;

            var s1 = new SupportBean_S1(100);
            var arrayBean = new SupportBeanArrayCollMap(s1);

            arrayBean.ObjectArr = new Object[]
            {
                null, "a", false, s1
            };
            _epService.EPRuntime.SendEvent(arrayBean);
            Assert.AreEqual(true, _listener.AssertOneGetNewAndReset().Get("value"));

            arrayBean.AnyObject = null;
            _epService.EPRuntime.SendEvent(arrayBean);
            Assert.IsNull(_listener.AssertOneGetNewAndReset().Get("value"));
        }

        [Test]
        public void TestInRange()
        {
            _epService.EPAdministrator.Configuration.AddEventType(
                "SupportBean", typeof (SupportBean));

            String[] fields = "ro,rc,rho,rhc,nro,nrc,nrho,nrhc".Split(',');
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                "select IntPrimitive in (2:4) as ro, IntPrimitive in [2:4] as rc, IntPrimitive in [2:4) as rho, IntPrimitive in (2:4] as rhc, "
                +
                "IntPrimitive not in (2:4) as nro, IntPrimitive not in [2:4] as nrc, IntPrimitive not in [2:4) as nrho, IntPrimitive not in (2:4] as nrhc "
                + "from SupportBean.std:lastevent()");

            stmt.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    false, false, false, false, true, true, true, true
                }
                );

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    false, true, true, false, true, false, false, true
                }
                );

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 3));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    true, true, true, true, false, false, false, false
                }
                );

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 4));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    false, true, false, true, true, false, true, false
                }
                );

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 5));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    false, false, false, false, true, true, true, true
                }
                );

            // test range reversed
            stmt.Dispose();
            stmt = _epService.EPAdministrator.CreateEPL(
                "select IntPrimitive between 4 and 2 as r1, IntPrimitive in [4:2] as r2 from SupportBean.std:lastevent()");
            stmt.Events += _listener.Update;

            fields = "r1,r2".Split(',');
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 3));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    true, true
                }
                );

            // test string type
            stmt.Dispose();
            fields = "ro".Split(',');
            stmt = _epService.EPAdministrator.CreateEPL(
                "select TheString in ('a':'d') as ro from SupportBean.std:lastevent()");
            stmt.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean("a", 5));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    false
                }
                );

            _epService.EPRuntime.SendEvent(new SupportBean("b", 5));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    true
                }
                );

            _epService.EPRuntime.SendEvent(new SupportBean("c", 5));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    true
                }
                );

            _epService.EPRuntime.SendEvent(new SupportBean("d", 5));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    false
                }
                );
        }

        [Test]
        public void TestInStringExpr()
        {
            TryString(
                "TheString in ('a', 'b', 'c')",
                new String[]
                {
                    "0", "a", "b", "c", "d", null
                },
                new bool?[]
                {
                    false, true, true, true, false, null
                }
                );

            TryString(
                "TheString in ('a')",
                new String[]
                {
                    "0", "a", "b", "c", "d", null
                },
                new bool?[]
                {
                    false, true, false, false, false, null
                }
                );

            TryString(
                "TheString in ('a', 'b')",
                new String[]
                {
                    "0", "b", "a", "c", "d", null
                },
                new bool?[]
                {
                    false, true, true, false, false, null
                }
                );

            TryString(
                "TheString in ('a', null)",
                new String[]
                {
                    "0", "b", "a", "c", "d", null
                },
                new bool?[]
                {
                    null, null, true, null, null, null
                }
                );

            TryString(
                "TheString in (null)",
                new String[]
                {
                    "0", null, "b"
                },
                new bool?[]
                {
                    null, null, null
                }
                );

            TryString(
                "TheString not in ('a', 'b', 'c')",
                new String[]
                {
                    "0", "a", "b", "c", "d", null
                },
                new bool?[]
                {
                    true, false, false, false, true, null
                }
                );

            TryString(
                "TheString not in (null)",
                new String[]
                {
                    "0", null, "b"
                },
                new bool?[]
                {
                    null, null, null
                }
                );
        }

        [Test]
        public void TestInStringExprOM()
        {
            String caseExpr = "select TheString in (\"a\",\"b\",\"c\") as result from "
                              + typeof (SupportBean).FullName;
            var model = new EPStatementObjectModel();

            model.SelectClause = SelectClause.Create().Add(
                Expressions.In("TheString", "a", "b", "c"), "result");
            model.FromClause = FromClause.Create(
                FilterStream.Create(typeof (SupportBean).FullName));

            TryString(
                model, caseExpr, new String[]
                {
                    "0", "a", "b", "c", "d", null
                }
                , new bool?[]
                {
                    false, true, true, true, false, null
                }
                );

            caseExpr = "select TheString not in (\"a\", \"b\", \"c\") as result from "
                       + typeof (SupportBean).FullName;
            model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create().Add(
                Expressions.NotIn("TheString", "a", "b", "c"), "result");
            model.FromClause = FromClause.Create(
                FilterStream.Create(typeof (SupportBean).FullName));
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);

            TryString(
                "TheString not in ('a', 'b', 'c')", new String[]
                {
                    "0", "a", "b", "c", "d", null
                }
                , new bool?[]
                {
                    true, false, false, false, true, null
                }
                );
        }

        [Test]
        public void TestInvalid()
        {
            _epService.EPAdministrator.Configuration.AddEventType(
                "SupportBean", typeof (SupportBean));
            _epService.EPAdministrator.Configuration.AddEventType(
                "ArrayBean", typeof (SupportBeanArrayCollMap));
            try
            {
                String stmtText = "select intArr in (1, 2, 3) as r1 from ArrayBean";

                _epService.EPAdministrator.CreateEPL(stmtText);
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual(
                    "Error starting statement: Failed to validate select-clause expression 'intArr in (1,2,3)': Collection or array comparison is not allowed for the IN, ANY, SOME or ALL keywords [select intArr in (1, 2, 3) as r1 from ArrayBean]",
                    ex.Message);
            }
        }
    }
}
