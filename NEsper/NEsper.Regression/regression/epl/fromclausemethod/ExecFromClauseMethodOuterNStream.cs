///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.execution;

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl.fromclausemethod
{
    public class ExecFromClauseMethodOuterNStream : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.AddEventType(typeof(SupportBeanInt));
            configuration.AddEventType<SupportBean>();
            configuration.AddImport(typeof(SupportJoinMethods));
            configuration.AddVariable("var1", typeof(int?), 0);
            configuration.AddVariable("var2", typeof(int?), 0);
            configuration.AddVariable("var3", typeof(int?), 0);
            configuration.AddVariable("var4", typeof(int?), 0);
        }
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("on SupportBeanInt(id like 'V%') set var1=p00, var2=p01");
    
            RunAssertion1Stream2HistStarSubordinateLeftRight(epService);
            RunAssertion1Stream2HistStarSubordinateInner(epService);
            RunAssertion1Stream2HistForwardSubordinate(epService);
            RunAssertion1Stream3HistForwardSubordinate(epService);
            RunAssertion1Stream3HistForwardSubordinateChain(epService);
            RunAssertionInvalid(epService);
            RunAssertion2Stream1HistStarSubordinateLeftRight(epService);
            RunAssertion1Stream2HistStarNoSubordinateLeftRight(epService);
        }
    
        private void RunAssertion1Stream2HistStarSubordinateLeftRight(EPServiceProvider epService) {
            string expression;
    
            expression = "select s0.id as id, h0.val as valh0, h1.val as valh1 " +
                    "from SupportBeanInt(id like 'E%')#keepall as s0 " +
                    " left outer join " +
                    "method:SupportJoinMethods.FetchValMultiRow('H0', p00, p04) as h0 " +
                    " on s0.p02 = h0.index " +
                    " left outer join " +
                    "method:SupportJoinMethods.FetchValMultiRow('H1', p01, p05) as h1 " +
                    " on s0.p03 = h1.index" +
                    " order by valh0, valh1";
            TryAssertionOne(epService, expression);
    
            expression = "select s0.id as id, h0.val as valh0, h1.val as valh1 from " +
                    "method:SupportJoinMethods.FetchValMultiRow('H1', p01, p05) as h1 " +
                    " right outer join " +
                    "SupportBeanInt(id like 'E%')#keepall as s0 " +
                    " on s0.p03 = h1.index " +
                    " left outer join " +
                    "method:SupportJoinMethods.FetchValMultiRow('H0', p00, p04) as h0 " +
                    " on s0.p02 = h0.index" +
                    " order by valh0, valh1";
            TryAssertionOne(epService, expression);
    
            expression = "select s0.id as id, h0.val as valh0, h1.val as valh1 from " +
                    "method:SupportJoinMethods.FetchValMultiRow('H0', p00, p04) as h0 " +
                    " right outer join " +
                    "SupportBeanInt(id like 'E%')#keepall as s0 " +
                    " on s0.p02 = h0.index" +
                    " left outer join " +
                    "method:SupportJoinMethods.FetchValMultiRow('H1', p01, p05) as h1 " +
                    " on s0.p03 = h1.index " +
                    " order by valh0, valh1";
            TryAssertionOne(epService, expression);
    
            expression = "select s0.id as id, h0.val as valh0, h1.val as valh1 from " +
                    "method:SupportJoinMethods.FetchValMultiRow('H0', p00, p04) as h0 " +
                    " full outer join " +
                    "SupportBeanInt(id like 'E%')#keepall as s0 " +
                    " on s0.p02 = h0.index" +
                    " full outer join " +
                    "method:SupportJoinMethods.FetchValMultiRow('H1', p01, p05) as h1 " +
                    " on s0.p03 = h1.index " +
                    " order by valh0, valh1";
            TryAssertionOne(epService, expression);
        }
    
        private void RunAssertion1Stream2HistStarSubordinateInner(EPServiceProvider epService) {
            string expression;
    
            expression = "select s0.id as id, h0.val as valh0, h1.val as valh1 " +
                    "from SupportBeanInt(id like 'E%')#keepall as s0 " +
                    " inner join " +
                    "method:SupportJoinMethods.FetchValMultiRow('H0', p00, p04) as h0 " +
                    " on s0.p02 = h0.index " +
                    " inner join " +
                    "method:SupportJoinMethods.FetchValMultiRow('H1', p01, p05) as h1 " +
                    " on s0.p03 = h1.index" +
                    " order by valh0, valh1";
            TryAssertionTwo(epService, expression);
    
            expression = "select s0.id as id, h0.val as valh0, h1.val as valh1 from " +
                    "method:SupportJoinMethods.FetchValMultiRow('H0', p00, p04) as h0 " +
                    " inner join " +
                    "SupportBeanInt(id like 'E%')#keepall as s0 " +
                    " on s0.p02 = h0.index " +
                    " inner join " +
                    "method:SupportJoinMethods.FetchValMultiRow('H1', p01, p05) as h1 " +
                    " on s0.p03 = h1.index" +
                    " order by valh0, valh1";
            TryAssertionTwo(epService, expression);
        }
    
        private void RunAssertion1Stream2HistForwardSubordinate(EPServiceProvider epService) {
            string expression;
    
            expression = "select s0.id as id, h0.val as valh0, h1.val as valh1 " +
                    "from SupportBeanInt(id like 'E%')#lastevent as s0 " +
                    " left outer join " +
                    "method:SupportJoinMethods.FetchVal('H0', p00) as h0 " +
                    " on s0.p02 = h0.index " +
                    " left outer join " +
                    "method:SupportJoinMethods.FetchVal('H1', p01) as h1 " +
                    " on h0.index = h1.index" +
                    " order by valh0, valh1";
            TryAssertionThree(epService, expression);
        }
    
        private void TryAssertionThree(EPServiceProvider epService, string expression) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            string[] fields = "id,valh0,valh1".Split(',');
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
    
            SendBeanInt(epService, "E1", 0, 0, 1);
            var result = new object[][]{new object[] {"E1", null, null}};
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, result);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, result);
    
            SendBeanInt(epService, "E2", 0, 1, 1);
            result = new object[][]{new object[] {"E2", null, null}};
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, result);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, result);
    
            SendBeanInt(epService, "E3", 1, 0, 1);
            result = new object[][]{new object[] {"E3", "H01", null}};
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, result);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, result);
    
            SendBeanInt(epService, "E4", 1, 1, 1);
            result = new object[][]{new object[] {"E4", "H01", "H11"}};
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, result);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, result);
    
            SendBeanInt(epService, "E5", 4, 4, 2);
            result = new object[][]{new object[] {"E5", "H02", "H12"}};
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, result);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, result);
    
            stmt.Dispose();
        }
    
        private void RunAssertion1Stream3HistForwardSubordinate(EPServiceProvider epService) {
            string expression;
    
            expression = "select s0.id as id, h0.val as valh0, h1.val as valh1, h2.val as valh2 " +
                    "from SupportBeanInt(id like 'E%')#lastevent as s0 " +
                    " left outer join " +
                    "method:SupportJoinMethods.FetchVal('H0', p00) as h0 " +
                    " on s0.p03 = h0.index " +
                    " left outer join " +
                    "method:SupportJoinMethods.FetchVal('H1', p01) as h1 " +
                    " on h0.index = h1.index" +
                    " left outer join " +
                    "method:SupportJoinMethods.FetchVal('H2', p02) as h2 " +
                    " on h1.index = h2.index" +
                    " order by valh0, valh1, valh2";
            TryAssertionFour(epService, expression);
    
            expression = "select s0.id as id, h0.val as valh0, h1.val as valh1, h2.val as valh2 from " +
                    "method:SupportJoinMethods.FetchVal('H0', p00) as h0 " +
                    " right outer join " +
                    "SupportBeanInt(id like 'E%')#lastevent as s0 " +
                    " on s0.p03 = h0.index " +
                    " left outer join " +
                    "method:SupportJoinMethods.FetchVal('H1', p01) as h1 " +
                    " on h0.index = h1.index" +
                    " full outer join " +
                    "method:SupportJoinMethods.FetchVal('H2', p02) as h2 " +
                    " on h1.index = h2.index" +
                    " order by valh0, valh1, valh2";
            TryAssertionFour(epService, expression);
        }
    
        private void TryAssertionFour(EPServiceProvider epService, string expression) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            string[] fields = "id,valh0,valh1,valh2".Split(',');
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
    
            SendBeanInt(epService, "E1", 0, 0, 0, 1);
            var result = new object[][]{new object[] {"E1", null, null, null}};
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, result);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, result);
    
            SendBeanInt(epService, "E2", 0, 1, 1, 1);
            result = new object[][]{new object[] {"E2", null, null, null}};
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, result);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, result);
    
            SendBeanInt(epService, "E3", 1, 1, 1, 1);
            result = new object[][]{new object[] {"E3", "H01", "H11", "H21"}};
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, result);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, result);
    
            SendBeanInt(epService, "E4", 1, 0, 1, 1);
            result = new object[][]{new object[] {"E4", "H01", null, null}};
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, result);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, result);
    
            SendBeanInt(epService, "E5", 4, 4, 4, 2);
            result = new object[][]{new object[] {"E5", "H02", "H12", "H22"}};
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, result);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, result);
    
            stmt.Dispose();
        }
    
        private void RunAssertion1Stream3HistForwardSubordinateChain(EPServiceProvider epService) {
            string expression;
    
            expression = "select s0.id as id, h0.val as valh0, h1.val as valh1, h2.val as valh2 " +
                    "from SupportBeanInt(id like 'E%')#lastevent as s0 " +
                    " left outer join " +
                    "method:SupportJoinMethods.FetchVal(s0.id || '-H0', p00) as h0 " +
                    " on s0.p03 = h0.index " +
                    " left outer join " +
                    "method:SupportJoinMethods.FetchVal(h0.val || '-H1', p01) as h1 " +
                    " on h0.index = h1.index" +
                    " left outer join " +
                    "method:SupportJoinMethods.FetchVal(h1.val || '-H2', p02) as h2 " +
                    " on h1.index = h2.index" +
                    " order by valh0, valh1, valh2";
            TryAssertionFive(epService, expression);
        }
    
        private void TryAssertionFive(EPServiceProvider epService, string expression) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            string[] fields = "id,valh0,valh1,valh2".Split(',');
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
    
            SendBeanInt(epService, "E1", 0, 0, 0, 1);
            var result = new object[][]{new object[] {"E1", null, null, null}};
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, result);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, result);
    
            SendBeanInt(epService, "E2", 0, 1, 1, 1);
            result = new object[][]{new object[] {"E2", null, null, null}};
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, result);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, result);
    
            SendBeanInt(epService, "E3", 1, 1, 1, 1);
            result = new object[][]{new object[] {"E3", "E3-H01", "E3-H01-H11", "E3-H01-H11-H21"}};
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, result);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, result);
    
            SendBeanInt(epService, "E4", 1, 0, 1, 1);
            result = new object[][]{new object[] {"E4", "E4-H01", null, null}};
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, result);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, result);
    
            SendBeanInt(epService, "E5", 4, 4, 4, 2);
            result = new object[][]{new object[] {"E5", "E5-H02", "E5-H02-H12", "E5-H02-H12-H22"}};
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, result);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, result);
    
            stmt.Dispose();
        }
    
        private void TryAssertionOne(EPServiceProvider epService, string expression) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            string[] fields = "id,valh0,valh1".Split(',');
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
    
            SendBeanInt(epService, "E1", 0, 0, 0, 0, 1, 1);
            var resultOne = new object[][]{new object[] {"E1", null, null}};
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, resultOne);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, resultOne);
    
            SendBeanInt(epService, "E2", 1, 1, 1, 1, 1, 1);
            var resultTwo = new object[][]{new object[] {"E2", "H01_0", "H11_0"}};
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, resultTwo);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, EPAssertionUtil.ConcatenateArray2Dim(resultOne, resultTwo));
    
            SendBeanInt(epService, "E3", 5, 5, 3, 4, 1, 1);
            var resultThree = new object[][]{new object[] {"E3", "H03_0", "H14_0"}};
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, resultThree);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, EPAssertionUtil.ConcatenateArray2Dim(resultOne, resultTwo, resultThree));
    
            SendBeanInt(epService, "E4", 0, 5, 3, 4, 1, 1);
            var resultFour = new object[][]{new object[] {"E4", null, "H14_0"}};
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, resultFour);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, EPAssertionUtil.ConcatenateArray2Dim(resultOne, resultTwo, resultThree, resultFour));
    
            SendBeanInt(epService, "E5", 2, 0, 2, 1, 1, 1);
            var resultFive = new object[][]{new object[] {"E5", "H02_0", null}};
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, resultFive);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, EPAssertionUtil.ConcatenateArray2Dim(resultOne, resultTwo, resultThree, resultFour, resultFive));
    
            // set 2 rows for H0
            SendBeanInt(epService, "E6", 2, 2, 2, 2, 2, 1);
            var resultSix = new object[][]{new object[] {"E6", "H02_0", "H12_0"}, new object[] {"E6", "H02_1", "H12_0"}};
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, resultSix);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, EPAssertionUtil.ConcatenateArray2Dim(resultOne, resultTwo, resultThree, resultFour, resultFive, resultSix));
    
            SendBeanInt(epService, "E7", 10, 10, 4, 5, 1, 2);
            var resultSeven = new object[][]{new object[] {"E7", "H04_0", "H15_0"}, new object[] {"E7", "H04_0", "H15_1"}};
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, resultSeven);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, EPAssertionUtil.ConcatenateArray2Dim(resultOne, resultTwo, resultThree, resultFour, resultFive, resultSix, resultSeven));
    
            stmt.Dispose();
        }
    
        private void TryAssertionTwo(EPServiceProvider epService, string expression) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            string[] fields = "id,valh0,valh1".Split(',');
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
    
            SendBeanInt(epService, "E1", 0, 0, 0, 0, 1, 1);
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
    
            SendBeanInt(epService, "E2", 1, 1, 1, 1, 1, 1);
            var resultTwo = new object[][]{new object[] {"E2", "H01_0", "H11_0"}};
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, resultTwo);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, EPAssertionUtil.ConcatenateArray2Dim(resultTwo));
    
            SendBeanInt(epService, "E3", 5, 5, 3, 4, 1, 1);
            var resultThree = new object[][]{new object[] {"E3", "H03_0", "H14_0"}};
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, resultThree);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, EPAssertionUtil.ConcatenateArray2Dim(resultTwo, resultThree));
    
            SendBeanInt(epService, "E4", 0, 5, 3, 4, 1, 1);
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, EPAssertionUtil.ConcatenateArray2Dim(resultTwo, resultThree));
    
            SendBeanInt(epService, "E5", 2, 0, 2, 1, 1, 1);
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, EPAssertionUtil.ConcatenateArray2Dim(resultTwo, resultThree));
    
            // set 2 rows for H0
            SendBeanInt(epService, "E6", 2, 2, 2, 2, 2, 1);
            var resultSix = new object[][]{new object[] {"E6", "H02_0", "H12_0"}, new object[] {"E6", "H02_1", "H12_0"}};
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, resultSix);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, EPAssertionUtil.ConcatenateArray2Dim(resultTwo, resultThree, resultSix));
    
            SendBeanInt(epService, "E7", 10, 10, 4, 5, 1, 2);
            var resultSeven = new object[][]{new object[] {"E7", "H04_0", "H15_0"}, new object[] {"E7", "H04_0", "H15_1"}};
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, resultSeven);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, EPAssertionUtil.ConcatenateArray2Dim(resultTwo, resultThree, resultSix, resultSeven));
    
            stmt.Dispose();
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            string expression;
            // Invalid dependency order: a historical depends on it's own outer join child or descendant
            //              S0
            //      H0  (depends H1)
            //      H1
            expression = "select * from " +
                    "SupportBeanInt#lastevent as s0 " +
                    " left outer join " +
                    "method:SupportJoinMethods.FetchVal(h1.val, 1) as h0 " +
                    " on s0.p00 = h0.index " +
                    " left outer join " +
                    "method:SupportJoinMethods.FetchVal('H1', 1) as h1 " +
                    " on h0.index = h1.index";
            TryInvalid(epService, expression, "Error starting statement: Historical stream 1 parameter dependency originating in stream 2 cannot or may not be satisfied by the join [select * from SupportBeanInt#lastevent as s0  left outer join method:SupportJoinMethods.FetchVal(h1.val, 1) as h0  on s0.p00 = h0.index  left outer join method:SupportJoinMethods.FetchVal('H1', 1) as h1  on h0.index = h1.index]");
    
            // Optimization conflict : required streams are always executed before optional streams
            //              S0
            //  full outer join H0 to S0
            //  left outer join H1 to S0 (H1 depends on H0)
            expression = "select * from " +
                    "SupportBeanInt#lastevent as s0 " +
                    " full outer join " +
                    "method:SupportJoinMethods.FetchVal('x', 1) as h0 " +
                    " on s0.p00 = h0.index " +
                    " left outer join " +
                    "method:SupportJoinMethods.FetchVal(h0.val, 1) as h1 " +
                    " on s0.p00 = h1.index";
            TryInvalid(epService, expression, "Error starting statement: Historical stream 2 parameter dependency originating in stream 1 cannot or may not be satisfied by the join [select * from SupportBeanInt#lastevent as s0  full outer join method:SupportJoinMethods.FetchVal('x', 1) as h0  on s0.p00 = h0.index  left outer join method:SupportJoinMethods.FetchVal(h0.val, 1) as h1  on s0.p00 = h1.index]");
        }
    
        private void RunAssertion2Stream1HistStarSubordinateLeftRight(EPServiceProvider epService) {
            string expression;
    
            //   S1 -> S0 -> H0
            expression = "select s0.id as s0id, s1.id as s1id, h0.val as valh0 from " +
                    "SupportBeanInt(id like 'E%')#keepall as s0 " +
                    " left outer join " +
                    "method:SupportJoinMethods.FetchVal(s0.id || 'H0', s0.p00) as h0 " +
                    " on s0.p01 = h0.index " +
                    " right outer join " +
                    "SupportBeanInt(id like 'F%')#keepall as s1 " +
                    " on s1.p01 = s0.p01";
            TryAssertionSix(epService, expression);
    
            expression = "select s0.id as s0id, s1.id as s1id, h0.val as valh0 from " +
                    "SupportBeanInt(id like 'F%')#keepall as s1 " +
                    " left outer join " +
                    "SupportBeanInt(id like 'E%')#keepall as s0 " +
                    " on s1.p01 = s0.p01" +
                    " left outer join " +
                    "method:SupportJoinMethods.FetchVal(s0.id || 'H0', s0.p00) as h0 " +
                    " on s0.p01 = h0.index ";
            TryAssertionSix(epService, expression);
    
            expression = "select s0.id as s0id, s1.id as s1id, h0.val as valh0 from " +
                    "method:SupportJoinMethods.FetchVal(s0.id || 'H0', s0.p00) as h0 " +
                    " right outer join " +
                    "SupportBeanInt(id like 'E%')#keepall as s0 " +
                    " on s0.p01 = h0.index " +
                    " right outer join " +
                    "SupportBeanInt(id like 'F%')#keepall as s1 " +
                    " on s1.p01 = s0.p01";
            TryAssertionSix(epService, expression);
        }
    
        private void TryAssertionSix(EPServiceProvider epService, string expression) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            string[] fields = "s0id,s1id,valh0".Split(',');
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
    
            SendBeanInt(epService, "E1", 1, 1);
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
    
            SendBeanInt(epService, "F1", 1, 1);
            var resultOne = new object[][]{new object[] {"E1", "F1", "E1H01"}};
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, resultOne);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, resultOne);
    
            SendBeanInt(epService, "F2", 2, 2);
            var resultTwo = new object[][]{new object[] {null, "F2", null}};
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, resultTwo);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, EPAssertionUtil.ConcatenateArray2Dim(resultOne, resultTwo));
    
            SendBeanInt(epService, "E2", 2, 2);
            var resultThree = new object[][]{new object[] {"E2", "F2", "E2H02"}};
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, resultThree);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, EPAssertionUtil.ConcatenateArray2Dim(resultOne, resultThree));
    
            SendBeanInt(epService, "F3", 3, 3);
            var resultFour = new object[][]{new object[] {null, "F3", null}};
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, resultFour);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, EPAssertionUtil.ConcatenateArray2Dim(resultOne, resultThree, resultFour));
    
            SendBeanInt(epService, "E3", 0, 3);
            var resultFive = new object[][]{new object[] {"E3", "F3", null}};
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, resultFive);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, EPAssertionUtil.ConcatenateArray2Dim(resultOne, resultThree, resultFive));
    
            stmt.Dispose();
        }
    
        private void RunAssertion1Stream2HistStarNoSubordinateLeftRight(EPServiceProvider epService) {
            string expression;
    
            expression = "select s0.id as s0id, h0.val as valh0, h1.val as valh1 from " +
                    "SupportBeanInt(id like 'E%')#keepall as s0 " +
                    " right outer join " +
                    "method:SupportJoinMethods.FetchVal('H0', 2) as h0 " +
                    " on s0.p00 = h0.index " +
                    " right outer join " +
                    "method:SupportJoinMethods.FetchVal('H1', 2) as h1 " +
                    " on s0.p00 = h1.index";
            TryAssertionSeven(epService, expression);
    
            expression = "select s0.id as s0id, h0.val as valh0, h1.val as valh1 from " +
                    "method:SupportJoinMethods.FetchVal('H1', 2) as h1 " +
                    " left outer join " +
                    "SupportBeanInt(id like 'E%')#keepall as s0 " +
                    " on s0.p00 = h1.index" +
                    " right outer join " +
                    "method:SupportJoinMethods.FetchVal('H0', 2) as h0 " +
                    " on s0.p00 = h0.index ";
            TryAssertionSeven(epService, expression);
        }
    
        private void TryAssertionSeven(EPServiceProvider epService, string expression) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            string[] fields = "s0id,valh0,valh1".Split(',');
            var resultOne = new object[][]{new object[] {null, "H01", null}, new object[] {null, "H02", null}, new object[] {null, null, "H11"}, new object[] {null, null, "H12"}};
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, resultOne);
    
            SendBeanInt(epService, "E1", 0);
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, resultOne);
    
            SendBeanInt(epService, "E2", 2);
            var resultTwo = new object[][]{new object[] {"E2", "H02", "H12"}};
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, resultTwo);
            var resultIt = new object[][]{new object[] {null, "H01", null}, new object[] {null, null, "H11"}, new object[] {"E2", "H02", "H12"}};
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, resultIt);
    
            SendBeanInt(epService, "E3", 1);
            resultTwo = new object[][]{new object[] {"E3", "H01", "H11"}};
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, resultTwo);
            resultIt = new object[][]{new object[] {"E3", "H01", "H11"}, new object[] {"E2", "H02", "H12"}};
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, resultIt);
    
            SendBeanInt(epService, "E4", 1);
            resultTwo = new object[][]{new object[] {"E4", "H01", "H11"}};
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, resultTwo);
            resultIt = new object[][]{new object[] {"E3", "H01", "H11"}, new object[] {"E4", "H01", "H11"}, new object[] {"E2", "H02", "H12"}};
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, resultIt);
    
            stmt.Dispose();
        }
    
        private void SendBeanInt(EPServiceProvider epService, string id, int p00, int p01, int p02, int p03, int p04, int p05) {
            epService.EPRuntime.SendEvent(new SupportBeanInt(id, p00, p01, p02, p03, p04, p05));
        }
    
        private void SendBeanInt(EPServiceProvider epService, string id, int p00, int p01, int p02, int p03) {
            SendBeanInt(epService, id, p00, p01, p02, p03, -1, -1);
        }
    
        private void SendBeanInt(EPServiceProvider epService, string id, int p00, int p01, int p02) {
            SendBeanInt(epService, id, p00, p01, p02, -1);
        }
    
        private void SendBeanInt(EPServiceProvider epService, string id, int p00, int p01) {
            SendBeanInt(epService, id, p00, p01, -1, -1);
        }
    
        private void SendBeanInt(EPServiceProvider epService, string id, int p00) {
            SendBeanInt(epService, id, p00, -1, -1, -1);
        }
    }
} // end of namespace
