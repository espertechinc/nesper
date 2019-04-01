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
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.expr.expr
{
    public class ExecExprAnyAllSomeExpr : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType("ArrayBean", typeof(SupportBeanArrayCollMap));
    
            RunAssertionEqualsAll(epService);
            RunAssertionEqualsAllArray(epService);
            RunAssertionEqualsAnyArray(epService);
            RunAssertionRelationalOpAllArray(epService);
            RunAssertionRelationalOpNullOrNoRows(epService);
            RunAssertionRelationalOpAnyArray(epService);
            RunAssertionEqualsAny(epService);
            RunAssertionRelationalOpAll(epService);
            RunAssertionRelationalOpAny(epService);
            RunAssertionEqualsInNullOrNoRows(epService);
            RunAssertionInvalid(epService);
        }
    
        private void RunAssertionEqualsAll(EPServiceProvider epService) {
            string[] fields = "eq,neq,sqlneq,nneq".Split(',');
            string stmtText = "select " +
                    "IntPrimitive=all(1,IntBoxed) as eq, " +
                    "IntPrimitive!=all(1,IntBoxed) as neq, " +
                    "IntPrimitive<>all(1,IntBoxed) as sqlneq, " +
                    "not IntPrimitive=all(1,IntBoxed) as nneq " +
                    "from SupportBean(TheString like \"E%\")";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // in the format intPrimitive, intBoxed
            int[][] testdata = {
                    new int[] {1, 1},
                    new int[] {1, 2},
                    new int[] {2, 2},
                    new int[] {2, 1},
            };
    
            object[][] result = {
                    new object[] {true, false, false, false}, // 1, 1
                    new object[] {false, false, false, true}, // 1, 2
                    new object[] {false, false, false, true}, // 2, 2
                    new object[] {false, true, true, true}    // 2, 1
            };
    
            for (int i = 0; i < testdata.Length; i++) {
                var bean = new SupportBean("E", testdata[i][0]);
                bean.IntBoxed = testdata[i][1];
                epService.EPRuntime.SendEvent(bean);
                //Log.Info("line " + i);
                EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, result[i]);
            }
    
            // test OM
            stmt.Dispose();
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(stmtText);
            Assert.AreEqual(stmtText.Replace("<>", "!="), model.ToEPL());
            stmt = epService.EPAdministrator.Create(model);
            stmt.Events += listener.Update;
    
            for (int i = 0; i < testdata.Length; i++) {
                var bean = new SupportBean("E", testdata[i][0]);
                bean.IntBoxed = testdata[i][1];
                epService.EPRuntime.SendEvent(bean);
                //Log.Info("line " + i);
                EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, result[i]);
            }
    
            stmt.Dispose();
        }
    
        private void RunAssertionEqualsAllArray(EPServiceProvider epService) {
            string[] fields = "e,ne".Split(',');
            string stmtText = "select " +
                    "LongBoxed = all ({1, 1}, intArr, longCol) as e, " +
                    "LongBoxed != all ({1, 1}, intArr, longCol) as ne " +
                    "from ArrayBean";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var arrayBean = new SupportBeanArrayCollMap(new int[]{1, 1});
            arrayBean.LongCol = Collections.List<long?>(1L, 1L);
            arrayBean.LongBoxed = 1L;
            epService.EPRuntime.SendEvent(arrayBean);
    
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{true, false});
    
            arrayBean.IntArr = new int[]{1, 1, 0};
            epService.EPRuntime.SendEvent(arrayBean);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{false, false});
    
            arrayBean.LongBoxed = 2L;
            epService.EPRuntime.SendEvent(arrayBean);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{false, true});
    
            stmt.Dispose();
        }
    
        private void RunAssertionEqualsAnyArray(EPServiceProvider epService) {
            string[] fields = "e,ne".Split(',');
            string stmtText = "select " +
                    "LongBoxed = any ({1, 1}, intArr, longCol) as e, " +
                    "LongBoxed != any ({1, 1}, intArr, longCol) as ne " +
                    "from ArrayBean";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var arrayBean = new SupportBeanArrayCollMap(new int[]{1, 1});
            arrayBean.LongCol = Collections.List<long?>(1L, 1L);
            arrayBean.LongBoxed = 1L;
            epService.EPRuntime.SendEvent(arrayBean);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{true, false});
    
            arrayBean.IntArr = new int[]{1, 1, 0};
            epService.EPRuntime.SendEvent(arrayBean);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{true, true});
    
            arrayBean.LongBoxed = 2L;
            epService.EPRuntime.SendEvent(arrayBean);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{false, true});
    
            stmt.Dispose();
        }
    
        private void RunAssertionRelationalOpAllArray(EPServiceProvider epService) {
            string[] fields = "g,ge".Split(',');
            string stmtText = "select " +
                    "LongBoxed>all({1,2},intArr,intCol) as g, " +
                    "LongBoxed>=all({1,2},intArr,intCol) as ge " +
                    "from ArrayBean";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var arrayBean = new SupportBeanArrayCollMap(new int[]{1, 2});
            arrayBean.IntCol = Collections.List(1, 2);
            arrayBean.LongBoxed = 3L;
            epService.EPRuntime.SendEvent(arrayBean);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{true, true});
    
            arrayBean.LongBoxed = 2L;
            epService.EPRuntime.SendEvent(arrayBean);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{false, true});
    
            arrayBean = new SupportBeanArrayCollMap(new int[]{1, 3});
            arrayBean.IntCol = Collections.List(1, 2);
            arrayBean.LongBoxed = 3L;
            epService.EPRuntime.SendEvent(arrayBean);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{false, true});
    
            arrayBean = new SupportBeanArrayCollMap(new int[]{1, 2});
            arrayBean.IntCol = Collections.List(1, 3);
            arrayBean.LongBoxed = 3L;
            epService.EPRuntime.SendEvent(arrayBean);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{false, true});
    
            // test OM
            stmt.Dispose();
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(stmtText);
            Assert.AreEqual(stmtText.Replace("<>", "!="), model.ToEPL());
            stmt = epService.EPAdministrator.Create(model);
            stmt.Events += listener.Update;
    
            arrayBean = new SupportBeanArrayCollMap(new int[]{1, 2});
            arrayBean.IntCol = Collections.List(1, 2);
            arrayBean.LongBoxed = 3L;
            epService.EPRuntime.SendEvent(arrayBean);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{true, true});
    
            stmt.Dispose();
        }
    
        private void RunAssertionRelationalOpNullOrNoRows(EPServiceProvider epService) {
            // test array
            string[] fields = "vall,vany".Split(',');
            string stmtText = "select " +
                    "IntBoxed >= all ({DoubleBoxed, LongBoxed}) as vall, " +
                    "IntBoxed >= any ({DoubleBoxed, LongBoxed}) as vany " +
                    " from SupportBean(TheString like 'E%')";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEvent(epService, "E3", null, null, null);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null});
            SendEvent(epService, "E4", 1, null, null);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null});
    
            SendEvent(epService, "E5", null, 1d, null);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null});
            SendEvent(epService, "E6", 1, 1d, null);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, true});
            SendEvent(epService, "E7", 0, 1d, null);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{false, false});
    
            // test fields
            stmt.Dispose();
            fields = "vall,vany".Split(',');
            stmtText = "select " +
                    "IntBoxed >= all (DoubleBoxed, LongBoxed) as vall, " +
                    "IntBoxed >= any (DoubleBoxed, LongBoxed) as vany " +
                    " from SupportBean(TheString like 'E%')";
            stmt = epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += listener.Update;
    
            SendEvent(epService, "E3", null, null, null);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null});
            SendEvent(epService, "E4", 1, null, null);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null});
    
            SendEvent(epService, "E5", null, 1d, null);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null});
            SendEvent(epService, "E6", 1, 1d, null);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, true});
            SendEvent(epService, "E7", 0, 1d, null);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{false, false});
    
            stmt.Dispose();
        }
    
        private void RunAssertionRelationalOpAnyArray(EPServiceProvider epService) {
            string[] fields = "g,ge".Split(',');
            string stmtText = "select " +
                    "LongBoxed > any ({1, 2}, intArr, intCol) as g, " +
                    "LongBoxed >= any ({1, 2}, intArr, intCol) as ge " +
                    "from ArrayBean";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var arrayBean = new SupportBeanArrayCollMap(new int[]{1, 2});
            arrayBean.IntCol = Collections.List(1, 2);
            arrayBean.LongBoxed = 1L;
            epService.EPRuntime.SendEvent(arrayBean);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{false, true});
    
            arrayBean.LongBoxed = 2L;
            epService.EPRuntime.SendEvent(arrayBean);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{true, true});
    
            arrayBean = new SupportBeanArrayCollMap(new int[]{2, 2});
            arrayBean.IntCol = Collections.List(2, 1);
            arrayBean.LongBoxed = 1L;
            epService.EPRuntime.SendEvent(arrayBean);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{false, true});
    
            arrayBean = new SupportBeanArrayCollMap(new int[]{1, 1});
            arrayBean.IntCol = Collections.List(1, 1);
            arrayBean.LongBoxed = 0L;
            epService.EPRuntime.SendEvent(arrayBean);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{false, false});
    
            stmt.Dispose();
        }
    
        private void RunAssertionEqualsAny(EPServiceProvider epService) {
            string[] fields = "eq,neq,sqlneq,nneq".Split(',');
            string stmtText = "select " +
                    "IntPrimitive = any (1, IntBoxed) as eq, " +
                    "IntPrimitive != any (1, IntBoxed) as neq, " +
                    "IntPrimitive <> any (1, IntBoxed) as sqlneq, " +
                    "not IntPrimitive = any (1, IntBoxed) as nneq " +
                    " from SupportBean(TheString like 'E%')";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // in the format intPrimitive, intBoxed
            int[][] testdata = {
                    new int[] {1, 1},
                    new int[] {1, 2},
                    new int[] {2, 2},
                    new int[] {2, 1},
            };
    
            object[][] result = {
                    new object[] {true, false, false, false}, // 1, 1
                    new object[] {true, true, true, false}, // 1, 2
                    new object[] {true, true, true, false}, // 2, 2
                    new object[] {false, true, true, true} // 2, 1
            };
    
            for (int i = 0; i < testdata.Length; i++) {
                var bean = new SupportBean("E", testdata[i][0]);
                bean.IntBoxed = testdata[i][1];
                epService.EPRuntime.SendEvent(bean);
                //Log.Info("line " + i);
                EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, result[i]);
            }
    
            stmt.Dispose();
        }
    
        private void RunAssertionRelationalOpAll(EPServiceProvider epService) {
            string[] fields = "g,ge,l,le".Split(',');
            string stmtText = "select " +
                    "IntPrimitive > all (1, 3, 4) as g, " +
                    "IntPrimitive >= all (1, 3, 4) as ge, " +
                    "IntPrimitive < all (1, 3, 4) as l, " +
                    "IntPrimitive <= all (1, 3, 4) as le " +
                    " from SupportBean(TheString like 'E%')";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            object[][] result = {
                    new object[] {false, false, true, true},
                    new object[] {false, false, false, true},
                    new object[] {false, false, false, false},
                    new object[] {false, false, false, false},
                    new object[] {false, true, false, false},
                    new object[] {true, true, false, false}
            };
    
            for (int i = 0; i < 6; i++) {
                epService.EPRuntime.SendEvent(new SupportBean("E1", i));
                //Log.Info("line " + i);
                EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, result[i]);
            }
    
            stmt.Dispose();
        }
    
        private void RunAssertionRelationalOpAny(EPServiceProvider epService) {
            string[] fields = "g,ge,l,le".Split(',');
            string stmtText = "select " +
                    "IntPrimitive > any (1, 3, 4) as g, " +
                    "IntPrimitive >= some (1, 3, 4) as ge, " +
                    "IntPrimitive < any (1, 3, 4) as l, " +
                    "IntPrimitive <= some (1, 3, 4) as le " +
                    " from SupportBean(TheString like 'E%')";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            object[][] result = {
                    new object[] {false, false, true, true},
                    new object[] {false, true, true, true},
                    new object[] {true, true, true, true},
                    new object[] {true, true, true, true},
                    new object[] {true, true, false, true},
                    new object[] {true, true, false, false}
            };
    
            for (int i = 0; i < 6; i++) {
                epService.EPRuntime.SendEvent(new SupportBean("E1", i));
                //Log.Info("line " + i);
                EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, result[i]);
            }
    
            stmt.Dispose();
        }
    
        private void RunAssertionEqualsInNullOrNoRows(EPServiceProvider epService) {
            // test fixed array case
            string[] fields = "eall,eany,neall,neany,isin".Split(',');
            string stmtText = "select " +
                    "IntBoxed = all ({DoubleBoxed, LongBoxed}) as eall, " +
                    "IntBoxed = any ({DoubleBoxed, LongBoxed}) as eany, " +
                    "IntBoxed != all ({DoubleBoxed, LongBoxed}) as neall, " +
                    "IntBoxed != any ({DoubleBoxed, LongBoxed}) as neany, " +
                    "IntBoxed in ({DoubleBoxed, LongBoxed}) as isin " +
                    " from SupportBean";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEvent(epService, "E3", null, null, null);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null, null, null, null});
            SendEvent(epService, "E4", 1, null, null);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null, null, null, null});
    
            SendEvent(epService, "E5", null, null, 1L);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null, null, null, null});
            SendEvent(epService, "E6", 1, null, 1L);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, true, false, null, true});
            SendEvent(epService, "E7", 0, null, 1L);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{false, null, null, true, null});
    
            // test non-array case
            stmt.Dispose();
            fields = "eall,eany,neall,neany,isin".Split(',');
            stmtText = "select " +
                    "IntBoxed = all (DoubleBoxed, LongBoxed) as eall, " +
                    "IntBoxed = any (DoubleBoxed, LongBoxed) as eany, " +
                    "IntBoxed != all (DoubleBoxed, LongBoxed) as neall, " +
                    "IntBoxed != any (DoubleBoxed, LongBoxed) as neany, " +
                    "IntBoxed in (DoubleBoxed, LongBoxed) as isin " +
                    " from SupportBean";
            stmt = epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += listener.Update;
    
            SendEvent(epService, "E3", null, null, null);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null, null, null, null});
            SendEvent(epService, "E4", 1, null, null);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null, null, null, null});
    
            SendEvent(epService, "E5", null, null, 1L);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null, null, null, null});
            SendEvent(epService, "E6", 1, null, 1L);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, true, false, null, true});
            SendEvent(epService, "E7", 0, null, 1L);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{false, null, null, true, null});
    
            stmt.Dispose();
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            try {
                string stmtText = "select intArr = all (1, 2, 3) as r1 from ArrayBean";
                epService.EPAdministrator.CreateEPL(stmtText);
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual("Error starting statement: Failed to validate select-clause expression 'intArr=all(1,2,3)': Collection or array comparison is not allowed for the IN, ANY, SOME or ALL keywords [select intArr = all (1, 2, 3) as r1 from ArrayBean]", ex.Message);
            }
    
            try {
                string stmtText = "select intArr > all (1, 2, 3) as r1 from ArrayBean";
                epService.EPAdministrator.CreateEPL(stmtText);
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual("Error starting statement: Failed to validate select-clause expression 'intArr>all(1,2,3)': Collection or array comparison is not allowed for the IN, ANY, SOME or ALL keywords [select intArr > all (1, 2, 3) as r1 from ArrayBean]", ex.Message);
            }
        }
    
        public void SendEvent(EPServiceProvider epService, string theString, int? intBoxed, double? doubleBoxed, long? longBoxed) {
            var bean = new SupportBean(theString, -1);
            bean.IntBoxed = intBoxed;
            bean.DoubleBoxed = doubleBoxed;
            bean.LongBoxed = longBoxed;
            epService.EPRuntime.SendEvent(bean);
        }
    }
} // end of namespace
