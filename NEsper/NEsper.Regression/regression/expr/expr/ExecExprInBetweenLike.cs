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
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.expr.expr
{
    public class ExecExprInBetweenLike : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionInObject(epService);
            RunAssertionInArraySubstitution(epService);
            RunAssertionInCollection(epService);
            RunAssertionInStringExprOM(epService);
            RunAssertionInStringExpr(epService);
            RunAssertionBetweenStringExpr(epService);
            RunAssertionInNumericExpr(epService);
            RunAssertionBetweenNumericExpr(epService);
            RunAssertionInBoolExpr(epService);
            RunAssertionInNumericCoercionLong(epService);
            RunAssertionInNumericCoercionDouble(epService);
            RunAssertionBetweenNumericCoercionLong(epService);
            RunAssertionInRange(epService);
            RunAssertionBetweenNumericCoercionDouble(epService);
            RunAssertionInvalid(epService);
        }
    
        private void RunAssertionInObject(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("ArrayBean", typeof(SupportBeanArrayCollMap));
            var stmtText = "select s0.anyObject in (objectArr) as value from ArrayBean s0";
    
            var stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var s1 = new SupportBean_S1(100);
            var arrayBean = new SupportBeanArrayCollMap(s1);
            arrayBean.ObjectArr = new object[]{null, "a", false, s1};
            epService.EPRuntime.SendEvent(arrayBean);
            Assert.AreEqual(true, listener.AssertOneGetNewAndReset().Get("value"));
    
            arrayBean.AnyObject = null;
            epService.EPRuntime.SendEvent(arrayBean);
            Assert.IsNull(listener.AssertOneGetNewAndReset().Get("value"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionInArraySubstitution(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            var stmtText = "select IntPrimitive in (?) as result from SupportBean";
            var prepared = epService.EPAdministrator.PrepareEPL(stmtText);
            prepared.SetObject(1, new int[]{10, 20, 30});
            var stmt = epService.EPAdministrator.Create(prepared);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            Assert.IsTrue((bool?) listener.AssertOneGetNewAndReset().Get("result"));
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 9));
            Assert.IsFalse((bool?) listener.AssertOneGetNewAndReset().Get("result"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionInCollection(EPServiceProvider epService) {
            var listener = new SupportUpdateListener();
            var fields = "resOne, resTwo".Split(',');

#if false
            var stmtText = "select 10 in (arrayProperty) as result from " + typeof(SupportBeanComplexProps).FullName;
            var stmt = epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += listener.Update;
            Assert.AreEqual(typeof(bool?), stmt.EventType.GetPropertyType("result"));
    
            stmtText = "select 5 in (arrayProperty) as result from " + typeof(SupportBeanComplexProps).FullName;
            var selectTestCaseTwo = epService.EPAdministrator.CreateEPL(stmtText);
            var listenerTwo = new SupportUpdateListener();
            selectTestCaseTwo.Events += listenerTwo.Update;
    
            epService.EPRuntime.SendEvent(SupportBeanComplexProps.MakeDefaultBean());
            Assert.AreEqual(true, listener.AssertOneGetNewAndReset().Get("result"));
            Assert.AreEqual(false, listenerTwo.AssertOneGetNewAndReset().Get("result"));
    
            stmt.Stop();
            selectTestCaseTwo.Stop();
    
            // Arrays
            stmtText = "select 1 in (IntArr, LongArr) as resOne, 1 not in (IntArr, LongArr) as resTwo from "
                       + typeof(SupportBeanArrayCollMap).FullName;
            stmt = epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBeanArrayCollMap(new int[]{10, 20, 30}));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{false, true});
            epService.EPRuntime.SendEvent(new SupportBeanArrayCollMap(new int[]{10, 1, 30}));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{true, false});
            epService.EPRuntime.SendEvent(new SupportBeanArrayCollMap(new int[]{30}, new long?[]{20L, 1L}));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{true, false});
            epService.EPRuntime.SendEvent(new SupportBeanArrayCollMap(new int[]{}, new long?[]{null, 1L}));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{true, false});
            epService.EPRuntime.SendEvent(new SupportBeanArrayCollMap(null, new long?[]{1L, 100L}));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{true, false});
            epService.EPRuntime.SendEvent(new SupportBeanArrayCollMap(null, new long?[]{0L, 100L}));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{false, true});
            stmt.Dispose();
#endif

            // Collection
            var stmtText = "select 1 in (IntCol, LongCol) as resOne, 1 not in (LongCol, IntCol) as resTwo from " 
                       + typeof(SupportBeanArrayCollMap).FullName;
            var stmt = epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBeanArrayCollMap(true, new int[] {10, 20, 30}, null));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {false, true});
            epService.EPRuntime.SendEvent(new SupportBeanArrayCollMap(true, new int[] {10, 20, 1}, null));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {true, false});
            epService.EPRuntime.SendEvent(new SupportBeanArrayCollMap(true, new int[] {30}, new long?[] {20L, 1L}));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {true, false});
            epService.EPRuntime.SendEvent(new SupportBeanArrayCollMap(true, new int[] { }, new long?[] {null, 1L}));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {true, false});
            epService.EPRuntime.SendEvent(new SupportBeanArrayCollMap(true, null, new long?[] {1L, 100L}));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {true, false});
            stmt.Dispose();
    
            // Maps
            stmtText = "select 1 in (LongMap, IntMap) as resOne, 1 not in (LongMap, IntMap) as resTwo from " + 
                       typeof(SupportBeanArrayCollMap).FullName;
            stmt = epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBeanArrayCollMap(false, new int[] {10, 20, 30}, null));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {false, true});
            epService.EPRuntime.SendEvent(new SupportBeanArrayCollMap(false, new int[] {10, 20, 1}, null));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {true, false});
            epService.EPRuntime.SendEvent(new SupportBeanArrayCollMap(false, new int[] {30}, new long?[] {20L, 1L}));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {true, false});
            epService.EPRuntime.SendEvent(new SupportBeanArrayCollMap(false, new int[] { }, new long?[] {null, 1L}));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {true, false});
            epService.EPRuntime.SendEvent(new SupportBeanArrayCollMap(false, null, new long?[] {1L, 100L}));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {true, false});
            stmt.Dispose();
    
            // Mixed
            stmtText = "select 1 in (LongBoxed, IntArr, LongMap, IntCol) as resOne, 1 not in (LongBoxed, IntArr, LongMap, IntCol) as resTwo from "
                       + typeof(SupportBeanArrayCollMap).FullName;
            stmt = epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBeanArrayCollMap(1L, new int[0], new long?[0], new int[0]));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {true, false});
            epService.EPRuntime.SendEvent(new SupportBeanArrayCollMap(2L, null, new long?[0], new int[0]));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {false, true});

            epService.EPRuntime.SendEvent(new SupportBeanArrayCollMap(null, null, null, new int[] {3, 4, 5, 6, 7, 7, 7, 8, 8, 8, 1}));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {true, false});

            epService.EPRuntime.SendEvent(new SupportBeanArrayCollMap(-1L, null, new long?[] {1L}, new int[] {3, 4, 5, 6, 7, 7, 7, 8, 8}));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {true, false});
            epService.EPRuntime.SendEvent(new SupportBeanArrayCollMap(-1L, new int[] {1}, null, new int[] { }));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {true, false});
            stmt.Dispose();
    
            // Object array
            stmtText = "select 1 in (objectArr) as resOne, 2 in (objectArr) as resTwo from " + typeof(SupportBeanArrayCollMap).FullName;
            stmt = epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBeanArrayCollMap(new object[] { }));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {false, false});
            epService.EPRuntime.SendEvent(new SupportBeanArrayCollMap(new object[] {1, 2}));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {true, true});
            epService.EPRuntime.SendEvent(new SupportBeanArrayCollMap(new object[] {1d, 2L}));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {false, false});
            epService.EPRuntime.SendEvent(new SupportBeanArrayCollMap(new object[] {null, 2}));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {null, true});
            stmt.Dispose();
    
            // Object array
            stmtText = "select 1 in ({1,2,3}) as resOne, 2 in ({0, 1}) as resTwo from " + typeof(SupportBeanArrayCollMap).FullName;
            stmt = epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBeanArrayCollMap(new object[]{}));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {true, false});
    
            stmt.Dispose();
        }
    
        private void RunAssertionInStringExprOM(EPServiceProvider epService) {
            var caseExpr = "select TheString in (\"a\",\"b\",\"c\") as result from " + typeof(SupportBean).FullName;
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create().Add(Expressions.In("TheString", "a", "b", "c"), "result");
            model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportBean).FullName));
    
            TryString(epService, model, caseExpr,
                    new string[]{"0", "a", "b", "c", "d", null},
                    new bool?[]{false, true, true, true, false, null});
    
            model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create().Add(Expressions.NotIn("TheString", "a", "b", "c"), "result");
            model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportBean).FullName));
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
    
            TryString(epService, "TheString not in ('a', 'b', 'c')",
                    new string[]{"0", "a", "b", "c", "d", null},
                    new bool?[]{true, false, false, false, true, null});
        }
    
        private void RunAssertionInStringExpr(EPServiceProvider epService) {
            TryString(epService, "TheString in ('a', 'b', 'c')",
                    new string[]{"0", "a", "b", "c", "d", null},
                    new bool?[]{false, true, true, true, false, null});
    
            TryString(epService, "TheString in ('a')",
                    new string[]{"0", "a", "b", "c", "d", null},
                    new bool?[]{false, true, false, false, false, null});
    
            TryString(epService, "TheString in ('a', 'b')",
                    new string[]{"0", "b", "a", "c", "d", null},
                    new bool?[]{false, true, true, false, false, null});
    
            TryString(epService, "TheString in ('a', null)",
                    new string[]{"0", "b", "a", "c", "d", null},
                    new bool?[]{null, null, true, null, null, null});
    
            TryString(epService, "TheString in (null)",
                    new string[]{"0", null, "b"},
                    new bool?[]{null, null, null});
    
            TryString(epService, "TheString not in ('a', 'b', 'c')",
                    new string[]{"0", "a", "b", "c", "d", null},
                    new bool?[]{true, false, false, false, true, null});
    
            TryString(epService, "TheString not in (null)",
                    new string[]{"0", null, "b"},
                    new bool?[]{null, null, null});
        }
    
        private void RunAssertionBetweenStringExpr(EPServiceProvider epService) {
            string[] input = null;
            bool?[] result = null;
    
            input = new string[]{"0", "a1", "a10", "c", "d", null, "a0", "b9", "b90"};
            result = new bool?[]{false, true, true, false, false, false, true, true, false};
            TryString(epService, "TheString between 'a0' and 'b9'", input, result);
            TryString(epService, "TheString between 'b9' and 'a0'", input, result);
    
            TryString(epService, "TheString between null and 'b9'",
                    new string[]{"0", null, "a0", "b9"},
                    new bool?[]{false, false, false, false});
    
            TryString(epService, "TheString between null and null",
                    new string[]{"0", null, "a0", "b9"},
                    new bool?[]{false, false, false, false});
    
            TryString(epService, "TheString between 'a0' and null",
                    new string[]{"0", null, "a0", "b9"},
                    new bool?[]{false, false, false, false});
    
            input = new string[]{"0", "a1", "a10", "c", "d", null, "a0", "b9", "b90"};
            result = new bool?[]{true, false, false, true, true, false, false, false, true};
            TryString(epService, "TheString not between 'a0' and 'b9'", input, result);
            TryString(epService, "TheString not between 'b9' and 'a0'", input, result);
        }
    
        private void RunAssertionInNumericExpr(EPServiceProvider epService) {
            var input = new double?[]{1d, null, 1.1d, 1.0d, 1.0999999999, 2d, 4d};
            var result = new bool?[]{false, null, true, false, false, true, true};
            TryNumeric(epService, "DoubleBoxed in (1.1d, 7/3.5, 2*6/3, 0)", input, result);
    
            TryNumeric(epService, "DoubleBoxed in (7/3d, null)",
                    new double?[]{2d, 7 / 3d, null},
                    new bool?[]{null, true, null});
    
            TryNumeric(epService, "DoubleBoxed in (5,5,5,5,5, -1)",
                    new double?[]{5.0, 5d, 0d, null, -1d},
                    new bool?[]{true, true, false, null, true});
    
            TryNumeric(epService, "DoubleBoxed not in (1.1d, 7/3.5, 2*6/3, 0)",
                    new double?[]{1d, null, 1.1d, 1.0d, 1.0999999999, 2d, 4d},
                    new bool?[]{true, null, false, true, true, false, false});
        }
    
        private void RunAssertionBetweenNumericExpr(EPServiceProvider epService) {
            var input = new double?[]{1d, null, 1.1d, 2d, 1.0999999999, 2d, 4d, 15d, 15.00001d};
            var result = new bool?[]{false, false, true, true, false, true, true, true, false};
            TryNumeric(epService, "DoubleBoxed between 1.1 and 15", input, result);
            TryNumeric(epService, "DoubleBoxed between 15 and 1.1", input, result);
    
            TryNumeric(epService, "DoubleBoxed between null and 15",
                    new double?[]{1d, null, 1.1d},
                    new bool?[]{false, false, false});
    
            TryNumeric(epService, "DoubleBoxed between 15 and null",
                    new double?[]{1d, null, 1.1d},
                    new bool?[]{false, false, false});
    
            TryNumeric(epService, "DoubleBoxed between null and null",
                    new double?[]{1d, null, 1.1d},
                    new bool?[]{false, false, false});
    
            input = new double?[]{1d, null, 1.1d, 2d, 1.0999999999, 2d, 4d, 15d, 15.00001d};
            result = new bool?[]{true, false, false, false, true, false, false, false, true};
            TryNumeric(epService, "DoubleBoxed not between 1.1 and 15", input, result);
            TryNumeric(epService, "DoubleBoxed not between 15 and 1.1", input, result);
    
            TryNumeric(epService, "DoubleBoxed not between 15 and null",
                    new double?[]{1d, null, 1.1d},
                    new bool?[]{false, false, false});
        }
    
        private void RunAssertionInBoolExpr(EPServiceProvider epService) {
            TryInBoolean(epService, "BoolBoxed in (true, true)",
                    new bool?[]{true, false},
                    new bool[]{true, false});
    
            TryInBoolean(epService, "BoolBoxed in (1>2, 2=3, 4<=2)",
                    new bool?[]{true, false},
                    new bool[]{false, true});
    
            TryInBoolean(epService, "BoolBoxed not in (1>2, 2=3, 4<=2)",
                    new bool?[]{true, false},
                    new bool[]{true, false});
        }
    
        private void RunAssertionInNumericCoercionLong(EPServiceProvider epService) {
            var caseExpr = "select IntPrimitive in (ShortBoxed, IntBoxed, LongBoxed) as result from " + typeof(SupportBean).FullName;
    
            var selectTestCase = epService.EPAdministrator.CreateEPL(caseExpr);
            var listener = new SupportUpdateListener();
            selectTestCase.Events += listener.Update;
            Assert.AreEqual(typeof(bool?), selectTestCase.EventType.GetPropertyType("result"));
    
            SendAndAssert(epService, listener, 1, 2, 3, 4L, false);
            SendAndAssert(epService, listener, 1, 1, 3, 4L, true);
            SendAndAssert(epService, listener, 1, 3, 1, 4L, true);
            SendAndAssert(epService, listener, 1, 3, 7, 1L, true);
            SendAndAssert(epService, listener, 1, 3, 7, null, null);
            SendAndAssert(epService, listener, 1, 1, null, null, true);
            SendAndAssert(epService, listener, 1, 0, null, 1L, true);
    
            selectTestCase.Stop();
        }
    
        private void RunAssertionInNumericCoercionDouble(EPServiceProvider epService) {
            var caseExpr = "select IntBoxed in (FloatBoxed, DoublePrimitive, LongBoxed) as result from " + typeof(SupportBean).FullName;
    
            var selectTestCase = epService.EPAdministrator.CreateEPL(caseExpr);
            var listener = new SupportUpdateListener();
            selectTestCase.Events += listener.Update;
            Assert.AreEqual(typeof(bool?), selectTestCase.EventType.GetPropertyType("result"));
    
            SendAndAssert(epService, listener, 1, 2f, 3d, 4L, false);
            SendAndAssert(epService, listener, 1, 1f, 3d, 4L, true);
            SendAndAssert(epService, listener, 1, 1.1f, 1.0d, 4L, true);
            SendAndAssert(epService, listener, 1, 1.1f, 1.2d, 1L, true);
            SendAndAssert(epService, listener, 1, null, 1.2d, 1L, true);
            SendAndAssert(epService, listener, null, null, 1.2d, 1L, null);
            SendAndAssert(epService, listener, null, 11f, 1.2d, 1L, null);
    
            selectTestCase.Stop();
        }
    
        private void RunAssertionBetweenNumericCoercionLong(EPServiceProvider epService) {
            var caseExpr = "select IntPrimitive between ShortBoxed and LongBoxed as result from " + typeof(SupportBean).FullName;
    
            var selectTestCase = epService.EPAdministrator.CreateEPL(caseExpr);
            var listener = new SupportUpdateListener();
            selectTestCase.Events += listener.Update;
            Assert.AreEqual(typeof(bool?), selectTestCase.EventType.GetPropertyType("result"));
    
            SendAndAssert(epService, listener, 1, 2, 3L, false);
            SendAndAssert(epService, listener, 2, 2, 3L, true);
            SendAndAssert(epService, listener, 3, 2, 3L, true);
            SendAndAssert(epService, listener, 4, 2, 3L, false);
            SendAndAssert(epService, listener, 5, 10, 1L, true);
            SendAndAssert(epService, listener, 1, 10, 1L, true);
            SendAndAssert(epService, listener, 10, 10, 1L, true);
            SendAndAssert(epService, listener, 11, 10, 1L, false);
    
            selectTestCase.Stop();
        }
    
        private void RunAssertionInRange(EPServiceProvider epService) {
    
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            var fields = "ro,rc,rho,rhc,nro,nrc,nrho,nrhc".Split(',');
            var stmt = epService.EPAdministrator.CreateEPL(
                    "select IntPrimitive in (2:4) as ro, IntPrimitive in [2:4] as rc, IntPrimitive in [2:4) as rho, IntPrimitive in (2:4] as rhc, " +
                            "IntPrimitive not in (2:4) as nro, IntPrimitive not in [2:4] as nrc, IntPrimitive not in [2:4) as nrho, IntPrimitive not in (2:4] as nrhc " +
                            "from SupportBean#lastevent");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{false, false, false, false, true, true, true, true});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{false, true, true, false, true, false, false, true});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 3));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{true, true, true, true, false, false, false, false});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 4));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{false, true, false, true, true, false, true, false});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 5));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{false, false, false, false, true, true, true, true});
    
            // test range reversed
            stmt.Dispose();
            stmt = epService.EPAdministrator.CreateEPL(
                    "select IntPrimitive between 4 and 2 as r1, IntPrimitive in [4:2] as r2 from SupportBean#lastevent");
            stmt.Events += listener.Update;
    
            fields = "r1,r2".Split(',');
            epService.EPRuntime.SendEvent(new SupportBean("E1", 3));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{true, true});
    
            // test string type
            stmt.Dispose();
            fields = "ro".Split(',');
            stmt = epService.EPAdministrator.CreateEPL("select TheString in ('a':'d') as ro from SupportBean#lastevent");
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("a", 5));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{false});
    
            epService.EPRuntime.SendEvent(new SupportBean("b", 5));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{true});
    
            epService.EPRuntime.SendEvent(new SupportBean("c", 5));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{true});
    
            epService.EPRuntime.SendEvent(new SupportBean("d", 5));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{false});
    
            stmt.Dispose();
        }
    
        private void RunAssertionBetweenNumericCoercionDouble(EPServiceProvider epService) {
            var caseExpr = "select IntBoxed between FloatBoxed and DoublePrimitive as result from " + typeof(SupportBean).FullName;
    
            var selectTestCase = epService.EPAdministrator.CreateEPL(caseExpr);
            var listener = new SupportUpdateListener();
            selectTestCase.Events += listener.Update;
            Assert.AreEqual(typeof(bool?), selectTestCase.EventType.GetPropertyType("result"));
    
            SendAndAssert(epService, listener, 1, 2f, 3d, false);
            SendAndAssert(epService, listener, 2, 2f, 3d, true);
            SendAndAssert(epService, listener, 3, 2f, 3d, true);
            SendAndAssert(epService, listener, 4, 2f, 3d, false);
            SendAndAssert(epService, listener, null, 2f, 3d, false);
            SendAndAssert(epService, listener, null, null, 3d, false);
            SendAndAssert(epService, listener, 1, 3f, 2d, false);
            SendAndAssert(epService, listener, 2, 3f, 2d, true);
            SendAndAssert(epService, listener, 3, 3f, 2d, true);
            SendAndAssert(epService, listener, 4, 3f, 2d, false);
            SendAndAssert(epService, listener, null, 3f, 2d, false);
            SendAndAssert(epService, listener, null, null, 2d, false);
    
            selectTestCase.Stop();
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType("ArrayBean", typeof(SupportBeanArrayCollMap));
            try {
                var stmtText = "select intArr in (1, 2, 3) as r1 from ArrayBean";
                epService.EPAdministrator.CreateEPL(stmtText);
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual("Error starting statement: Failed to validate select-clause expression 'intArr in (1,2,3)': Collection or array comparison is not allowed for the IN, ANY, SOME or ALL keywords [select intArr in (1, 2, 3) as r1 from ArrayBean]", ex.Message);
            }
        }
    
        private void SendAndAssert(EPServiceProvider epService, SupportUpdateListener listener, int? intBoxed, float? floatBoxed, double doublePrimitive, bool? result) {
            var bean = new SupportBean();
            bean.IntBoxed = intBoxed;
            bean.FloatBoxed = floatBoxed;
            bean.DoublePrimitive = doublePrimitive;
    
            epService.EPRuntime.SendEvent(bean);
    
            var theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(result, theEvent.Get("result"));
        }
    
        private void SendAndAssert(
            EPServiceProvider epService, 
            SupportUpdateListener listener,
            int intPrimitive, 
            short? shortBoxed, 
            int? intBoxed, 
            long? longBoxed,
            bool? result)
        {
            var bean = new SupportBean();
            bean.IntPrimitive = intPrimitive;
            bean.ShortBoxed = (short) shortBoxed;
            bean.IntBoxed = intBoxed;
            bean.LongBoxed = longBoxed;
    
            epService.EPRuntime.SendEvent(bean);
    
            var theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(result, theEvent.Get("result"));
        }
    
        private void SendAndAssert(
            EPServiceProvider epService,
            SupportUpdateListener listener, 
            int intPrimitive,
            short? shortBoxed,
            long? longBoxed,
            bool? result)
        {
            var bean = new SupportBean();
            bean.IntPrimitive = intPrimitive;
            bean.ShortBoxed = (short) shortBoxed;
            bean.LongBoxed = longBoxed;
    
            epService.EPRuntime.SendEvent(bean);
    
            var theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(result, theEvent.Get("result"));
        }
    
        private void SendAndAssert(
            EPServiceProvider epService, 
            SupportUpdateListener listener,
            int? intBoxed, 
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
    
            epService.EPRuntime.SendEvent(bean);
    
            var theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(result, theEvent.Get("result"));
        }
    
        private void TryInBoolean(EPServiceProvider epService, string expr, bool?[] input, bool[] result) {
            var caseExpr = "select " + expr + " as result from " + typeof(SupportBean).FullName;
    
            var selectTestCase = epService.EPAdministrator.CreateEPL(caseExpr);
            var listener = new SupportUpdateListener();
            selectTestCase.Events += listener.Update;
            Assert.AreEqual(typeof(bool?), selectTestCase.EventType.GetPropertyType("result"));
    
            for (var i = 0; i < input.Length; i++) {
                SendSupportBeanEvent(epService, input[i]);
                var theEvent = listener.AssertOneGetNewAndReset();
                Assert.AreEqual(result[i], theEvent.Get("result"), "Wrong result for " + input[i]);
            }
            selectTestCase.Stop();
        }
    
        private void TryNumeric(EPServiceProvider epService, string expr, double?[] input, bool?[] result) {
            var caseExpr = "select " + expr + " as result from " + typeof(SupportBean).FullName;
    
            var selectTestCase = epService.EPAdministrator.CreateEPL(caseExpr);
            var listener = new SupportUpdateListener();
            selectTestCase.Events += listener.Update;
            Assert.AreEqual(typeof(bool?), selectTestCase.EventType.GetPropertyType("result"));
    
            for (var i = 0; i < input.Length; i++) {
                SendSupportBeanEvent(epService, input[i]);
                var theEvent = listener.AssertOneGetNewAndReset();
                Assert.AreEqual(result[i], theEvent.Get("result"), "Wrong result for " + input[i]);
            }
            selectTestCase.Stop();
        }
    
        private void TryString(EPServiceProvider epService, string expression, string[] input, bool?[] result) {
            var caseExpr = "select " + expression + " as result from " + typeof(SupportBean).FullName;
    
            var selectTestCase = epService.EPAdministrator.CreateEPL(caseExpr);
            var listener = new SupportUpdateListener();
            selectTestCase.Events += listener.Update;
            Assert.AreEqual(typeof(bool?), selectTestCase.EventType.GetPropertyType("result"));
    
            for (var i = 0; i < input.Length; i++) {
                SendSupportBeanEvent(epService, input[i]);
                var theEvent = listener.AssertOneGetNewAndReset();
                Assert.AreEqual(result[i], theEvent.Get("result"), "Wrong result for " + input[i]);
            }
            selectTestCase.Stop();
        }
    
        private void TryString(EPServiceProvider epService, EPStatementObjectModel model, string epl, string[] input, bool?[] result) {
            var selectTestCase = epService.EPAdministrator.Create(model);
            Assert.AreEqual(epl, model.ToEPL());
    
            var compiled = epService.EPAdministrator.CompileEPL(epl);
            compiled = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, compiled);
            Assert.AreEqual(epl, compiled.ToEPL());
    
            var listener = new SupportUpdateListener();
            selectTestCase.Events += listener.Update;
            Assert.AreEqual(typeof(bool?), selectTestCase.EventType.GetPropertyType("result"));
    
            for (var i = 0; i < input.Length; i++) {
                SendSupportBeanEvent(epService, input[i]);
                var theEvent = listener.AssertOneGetNewAndReset();
                Assert.AreEqual(result[i], theEvent.Get("result"), "Wrong result for " + input[i]);
            }
            selectTestCase.Stop();
        }
    
        private void SendSupportBeanEvent(EPServiceProvider epService, double? doubleBoxed) {
            var theEvent = new SupportBean();
            theEvent.DoubleBoxed = doubleBoxed;
            epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void SendSupportBeanEvent(EPServiceProvider epService, string theString) {
            var theEvent = new SupportBean();
            theEvent.TheString = theString;
            epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void SendSupportBeanEvent(EPServiceProvider epService, bool? boolBoxed) {
            var theEvent = new SupportBean();
            theEvent.BoolBoxed = boolBoxed;
            epService.EPRuntime.SendEvent(theEvent);
        }
    }
} // end of namespace
