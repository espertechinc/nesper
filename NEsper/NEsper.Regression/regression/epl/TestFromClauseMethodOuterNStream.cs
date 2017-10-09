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
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.epl;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestFromClauseMethodOuterNStream  {
        private EPServiceProvider epService;
        private SupportUpdateListener listener;
    
        [SetUp]
        public void SetUp() {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType(typeof(SupportBeanInt));
            config.AddEventType<SupportBean>();
            config.AddImport(typeof(SupportJoinMethods).FullName);
            config.AddVariable("var1", typeof(int), 0);
            config.AddVariable("var2", typeof(int), 0);
            config.AddVariable("var3", typeof(int), 0);
            config.AddVariable("var4", typeof(int), 0);
            epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            listener = new SupportUpdateListener();
    
            epService.EPAdministrator.CreateEPL("on SupportBeanInt(id like 'V%') set var1=p00, var2=p01");
        }
    
        [TearDown]
        public void TearDown()
        {
            listener = null;
        }
    
        [Test]
        public void Test1Stream2HistStarSubordinateLeftRight() {
            String expression;
    
            expression = "select s0.id as id, h0.val as valh0, h1.val as valh1 " +
                    "from SupportBeanInt(id like 'E%')#keepall as s0 " +
                    " left outer join " +
                    "method:SupportJoinMethods.FetchValMultiRow('H0', p00, p04) as h0 " +
                    " on s0.p02 = h0.index " +
                    " left outer join " +
                    "method:SupportJoinMethods.FetchValMultiRow('H1', p01, p05) as h1 " +
                    " on s0.p03 = h1.index" +
                    " order by valh0, valh1";
            RunAssertionOne(expression);
    
            expression = "select s0.id as id, h0.val as valh0, h1.val as valh1 from " +
                    "method:SupportJoinMethods.FetchValMultiRow('H1', p01, p05) as h1 " +
                    " right outer join " +
                    "SupportBeanInt(id like 'E%')#keepall as s0 " +
                    " on s0.p03 = h1.index " +
                    " left outer join " +
                    "method:SupportJoinMethods.FetchValMultiRow('H0', p00, p04) as h0 " +
                    " on s0.p02 = h0.index" +
                    " order by valh0, valh1";
            RunAssertionOne(expression);
    
            expression = "select s0.id as id, h0.val as valh0, h1.val as valh1 from " +
                    "method:SupportJoinMethods.FetchValMultiRow('H0', p00, p04) as h0 " +
                    " right outer join " +
                    "SupportBeanInt(id like 'E%')#keepall as s0 " +
                    " on s0.p02 = h0.index" +
                    " left outer join " +
                    "method:SupportJoinMethods.FetchValMultiRow('H1', p01, p05) as h1 " +
                    " on s0.p03 = h1.index " +
                    " order by valh0, valh1";
            RunAssertionOne(expression);
    
            expression = "select s0.id as id, h0.val as valh0, h1.val as valh1 from " +
                    "method:SupportJoinMethods.FetchValMultiRow('H0', p00, p04) as h0 " +
                    " full outer join " +
                    "SupportBeanInt(id like 'E%')#keepall as s0 " +
                    " on s0.p02 = h0.index" +
                    " full outer join " +
                    "method:SupportJoinMethods.FetchValMultiRow('H1', p01, p05) as h1 " +
                    " on s0.p03 = h1.index " +
                    " order by valh0, valh1";
            RunAssertionOne(expression);
        }
    
        [Test]
        public void Test1Stream2HistStarSubordinateInner() {
            String expression;
    
            expression = "select s0.id as id, h0.val as valh0, h1.val as valh1 " +
                    "from SupportBeanInt(id like 'E%')#keepall as s0 " +
                    " inner join " +
                    "method:SupportJoinMethods.FetchValMultiRow('H0', p00, p04) as h0 " +
                    " on s0.p02 = h0.index " +
                    " inner join " +
                    "method:SupportJoinMethods.FetchValMultiRow('H1', p01, p05) as h1 " +
                    " on s0.p03 = h1.index" +
                    " order by valh0, valh1";
            RunAssertionTwo(expression);
    
            expression = "select s0.id as id, h0.val as valh0, h1.val as valh1 from " +
                    "method:SupportJoinMethods.FetchValMultiRow('H0', p00, p04) as h0 " +
                    " inner join " +
                    "SupportBeanInt(id like 'E%')#keepall as s0 " +
                    " on s0.p02 = h0.index " +
                    " inner join " +
                    "method:SupportJoinMethods.FetchValMultiRow('H1', p01, p05) as h1 " +
                    " on s0.p03 = h1.index" +
                    " order by valh0, valh1";
            RunAssertionTwo(expression);
        }
    
        [Test]
        public void Test1Stream2HistForwardSubordinate() {
            String expression;
    
            expression = "select s0.id as id, h0.val as valh0, h1.val as valh1 " +
                    "from SupportBeanInt(id like 'E%')#lastevent as s0 " +
                    " left outer join " +
                    "method:SupportJoinMethods.FetchVal('H0', p00) as h0 " +
                    " on s0.p02 = h0.index " +
                    " left outer join " +
                    "method:SupportJoinMethods.FetchVal('H1', p01) as h1 " +
                    " on h0.index = h1.index" +
                    " order by valh0, valh1";
            RunAssertionThree(expression);
        }
    
        private void RunAssertionThree(String expression) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(expression);
            listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            String[] fields = "id,valh0,valh1".Split(',');
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
    
            SendBeanInt("E1", 0, 0, 1);
            Object[][] result = new Object[][] { new Object[] { "E1", null, null } };
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, result);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, result);
    
            SendBeanInt("E2", 0, 1, 1);
            result = new Object[][] { new Object[] { "E2", null, null } };
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, result);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, result);
    
            SendBeanInt("E3", 1, 0, 1);
            result = new Object[][] { new Object[] { "E3", "H01", null } };
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, result);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, result);
    
            SendBeanInt("E4", 1, 1, 1);
            result = new Object[][] { new Object[] { "E4", "H01", "H11" } };
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, result);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, result);
    
            SendBeanInt("E5", 4, 4, 2);
            result = new Object[][] { new Object[] { "E5", "H02", "H12" } };
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, result);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, result);
        }
    
        [Test]
        public void Test1Stream3HistForwardSubordinate() {
            String expression;
    
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
            RunAssertionFour(expression);
    
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
            RunAssertionFour(expression);
        }
    
        private void RunAssertionFour(String expression) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(expression);
            listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            String[] fields = "id,valh0,valh1,valh2".Split(',');
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
    
            SendBeanInt("E1", 0, 0, 0, 1);
            Object[][] result = new Object[][] { new Object[] { "E1", null, null, null } };
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, result);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, result);
    
            SendBeanInt("E2", 0, 1, 1, 1);
            result = new Object[][] { new Object[] { "E2", null, null, null } };
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, result);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, result);
    
            SendBeanInt("E3", 1, 1, 1, 1);
            result = new Object[][] { new Object[] { "E3", "H01", "H11", "H21" } };
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, result);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, result);
    
            SendBeanInt("E4", 1, 0, 1, 1);
            result = new Object[][] { new Object[] { "E4", "H01", null, null } };
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, result);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, result);
    
            SendBeanInt("E5", 4, 4, 4, 2);
            result = new Object[][] { new Object[] { "E5", "H02", "H12", "H22" } };
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, result);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, result);
        }
    
        [Test]
        public void Test1Stream3HistForwardSubordinateChain() {
            String expression;
    
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
            RunAssertionFive(expression);
        }
    
        private void RunAssertionFive(String expression) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(expression);
            listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            String[] fields = "id,valh0,valh1,valh2".Split(',');
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
    
            SendBeanInt("E1", 0, 0, 0, 1);
            Object[][] result = new Object[][] { new Object[] { "E1", null, null, null } };
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, result);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, result);
    
            SendBeanInt("E2", 0, 1, 1, 1);
            result = new Object[][] { new Object[] { "E2", null, null, null } };
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, result);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, result);
    
            SendBeanInt("E3", 1, 1, 1, 1);
            result = new Object[][] { new Object[] { "E3", "E3-H01", "E3-H01-H11", "E3-H01-H11-H21" } };
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, result);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, result);
    
            SendBeanInt("E4", 1, 0, 1, 1);
            result = new Object[][] { new Object[] { "E4", "E4-H01", null, null } };
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, result);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, result);
    
            SendBeanInt("E5", 4, 4, 4, 2);
            result = new Object[][] { new Object[] { "E5", "E5-H02", "E5-H02-H12", "E5-H02-H12-H22" } };
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, result);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, result);
        }
    
        private void RunAssertionOne(String expression) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(expression);
            listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            String[] fields = "id,valh0,valh1".Split(',');
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
    
            SendBeanInt("E1", 0, 0, 0, 0, 1, 1);
            Object[][] resultOne = new Object[][] { new Object[] { "E1", null, null } };
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, resultOne);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, resultOne);
    
            SendBeanInt("E2", 1, 1, 1, 1, 1, 1);
            Object[][] resultTwo = new Object[][] { new Object[] { "E2", "H01_0", "H11_0" } };
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, resultTwo);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, EPAssertionUtil.ConcatenateArray2Dim(resultOne, resultTwo));
    
            SendBeanInt("E3", 5, 5, 3, 4, 1, 1);
            Object[][] resultThree = new Object[][] { new Object[] { "E3", "H03_0", "H14_0" } };
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, resultThree);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, EPAssertionUtil.ConcatenateArray2Dim(resultOne, resultTwo, resultThree));
    
            SendBeanInt("E4", 0, 5, 3, 4, 1, 1);
            Object[][] resultFour = new Object[][] { new Object[] { "E4", null, "H14_0" } };
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, resultFour);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, EPAssertionUtil.ConcatenateArray2Dim(resultOne, resultTwo, resultThree, resultFour));
    
            SendBeanInt("E5", 2, 0, 2, 1, 1, 1);
            Object[][] resultFive = new Object[][] { new Object[] { "E5", "H02_0", null } };
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, resultFive);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, EPAssertionUtil.ConcatenateArray2Dim(resultOne, resultTwo, resultThree, resultFour, resultFive));
    
            // set 2 rows for H0
            SendBeanInt("E6", 2, 2, 2, 2, 2, 1);
            Object[][] resultSix = new Object[][] { new Object[] { "E6", "H02_0", "H12_0" }, new Object[] { "E6", "H02_1", "H12_0" } };
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, resultSix);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, EPAssertionUtil.ConcatenateArray2Dim(resultOne, resultTwo, resultThree, resultFour, resultFive, resultSix));
    
            SendBeanInt("E7", 10, 10, 4, 5, 1, 2);
            Object[][] resultSeven = new Object[][] { new Object[] { "E7", "H04_0", "H15_0" }, new Object[] { "E7", "H04_0", "H15_1" } };
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, resultSeven);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, EPAssertionUtil.ConcatenateArray2Dim(resultOne, resultTwo, resultThree, resultFour, resultFive, resultSix, resultSeven));
        }
    
        private void RunAssertionTwo(String expression) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(expression);
            listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            String[] fields = "id,valh0,valh1".Split(',');
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
    
            SendBeanInt("E1", 0, 0, 0, 0, 1, 1);
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
    
            SendBeanInt("E2", 1, 1, 1, 1, 1, 1);
            Object[][] resultTwo = new Object[][] { new Object[] { "E2", "H01_0", "H11_0" } };
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, resultTwo);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, EPAssertionUtil.ConcatenateArray2Dim(resultTwo));
    
            SendBeanInt("E3", 5, 5, 3, 4, 1, 1);
            Object[][] resultThree = new Object[][] { new Object[] { "E3", "H03_0", "H14_0" } };
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, resultThree);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, EPAssertionUtil.ConcatenateArray2Dim(resultTwo, resultThree));
    
            SendBeanInt("E4", 0, 5, 3, 4, 1, 1);
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, EPAssertionUtil.ConcatenateArray2Dim(resultTwo, resultThree));
    
            SendBeanInt("E5", 2, 0, 2, 1, 1, 1);
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, EPAssertionUtil.ConcatenateArray2Dim(resultTwo, resultThree));
    
            // set 2 rows for H0
            SendBeanInt("E6", 2, 2, 2, 2, 2, 1);
            Object[][] resultSix = new Object[][] { new Object[] { "E6", "H02_0", "H12_0" }, new Object[] { "E6", "H02_1", "H12_0" } };
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, resultSix);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, EPAssertionUtil.ConcatenateArray2Dim(resultTwo, resultThree, resultSix));
    
            SendBeanInt("E7", 10, 10, 4, 5, 1, 2);
            Object[][] resultSeven = new Object[][] { new Object[] { "E7", "H04_0", "H15_0" }, new Object[] { "E7", "H04_0", "H15_1" } };
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, resultSeven);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, EPAssertionUtil.ConcatenateArray2Dim(resultTwo, resultThree, resultSix, resultSeven));
        }
    
        [Test]
        public void TestInvalid() {
            String expression;
            // Invalid dependency order: a historical depends on it's own outer join child or descendant
            //              S0
            //      H0  (depends H1)
            //      H1
            expression = "select * from " +
                    "SupportBeanInt#lastevent as s0 " +
                    " left outer join " +
                    "method:SupportJoinMethods.FetchVal(h1.val, 1) as h0 " +
                    " on s0.P00 = h0.index " +
                    " left outer join " +
                    "method:SupportJoinMethods.FetchVal('H1', 1) as h1 " +
                    " on h0.index = h1.index";
            TryInvalid(expression, "Error starting statement: Historical stream 1 parameter dependency originating in stream 2 cannot or may not be satisfied by the join [select * from SupportBeanInt#lastevent as s0  left outer join method:SupportJoinMethods.FetchVal(h1.val, 1) as h0  on s0.P00 = h0.index  left outer join method:SupportJoinMethods.FetchVal('H1', 1) as h1  on h0.index = h1.index]");
    
            // Optimization conflict : required streams are always executed before optional streams
            //              S0
            //  full outer join H0 to S0
            //  left outer join H1 to S0 (H1 depends on H0)
            expression = "select * from " +
                    "SupportBeanInt#lastevent as s0 " +
                    " full outer join " +
                    "method:SupportJoinMethods.FetchVal('x', 1) as h0 " +
                    " on s0.P00 = h0.index " +
                    " left outer join " +
                    "method:SupportJoinMethods.FetchVal(h0.val, 1) as h1 " +
                    " on s0.P00 = h1.index";
            TryInvalid(expression, "Error starting statement: Historical stream 2 parameter dependency originating in stream 1 cannot or may not be satisfied by the join [select * from SupportBeanInt#lastevent as s0  full outer join method:SupportJoinMethods.FetchVal('x', 1) as h0  on s0.P00 = h0.index  left outer join method:SupportJoinMethods.FetchVal(h0.val, 1) as h1  on s0.P00 = h1.index]");
        }
    
        [Test]
        public void Test2Stream1HistStarSubordinateLeftRight() {
            String expression;
    
            //   S1 -> S0 -> H0
            expression = "select s0.id as s0id, s1.id as s1id, h0.val as valh0 from " +
                    "SupportBeanInt(id like 'E%')#keepall as s0 " +
                    " left outer join " +
                    "method:SupportJoinMethods.FetchVal(s0.id || 'H0', s0.P00) as h0 " +
                    " on s0.p01 = h0.index " +
                    " right outer join " +
                    "SupportBeanInt(id like 'F%')#keepall as s1 " +
                    " on s1.p01 = s0.p01";
            RunAssertionSix(expression);
    
            expression = "select s0.id as s0id, s1.id as s1id, h0.val as valh0 from " +
                    "SupportBeanInt(id like 'F%')#keepall as s1 " +
                    " left outer join " +
                    "SupportBeanInt(id like 'E%')#keepall as s0 " +
                    " on s1.p01 = s0.p01" +
                    " left outer join " +
                    "method:SupportJoinMethods.FetchVal(s0.id || 'H0', s0.P00) as h0 " +
                    " on s0.p01 = h0.index ";
            RunAssertionSix(expression);
    
            expression = "select s0.id as s0id, s1.id as s1id, h0.val as valh0 from " +
                    "method:SupportJoinMethods.FetchVal(s0.id || 'H0', s0.P00) as h0 " +
                    " right outer join " +
                    "SupportBeanInt(id like 'E%')#keepall as s0 " +
                    " on s0.p01 = h0.index " +
                    " right outer join " +
                    "SupportBeanInt(id like 'F%')#keepall as s1 " +
                    " on s1.p01 = s0.p01";
            RunAssertionSix(expression);
        }
    
        private void RunAssertionSix(String expression) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(expression);
            listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            String[] fields = "s0id,s1id,valh0".Split(',');
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
    
            SendBeanInt("E1", 1, 1);
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
    
            SendBeanInt("F1", 1, 1);
            Object[][] resultOne = new Object[][] { new Object[] { "E1", "F1", "E1H01" } };
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, resultOne);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, resultOne);
    
            SendBeanInt("F2", 2, 2);
            Object[][] resultTwo = new Object[][] { new Object[] { null, "F2", null } };
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, resultTwo);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, EPAssertionUtil.ConcatenateArray2Dim(resultOne, resultTwo));
    
            SendBeanInt("E2", 2, 2);
            Object[][] resultThree = new Object[][] { new Object[] { "E2", "F2", "E2H02" } };
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, resultThree);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, EPAssertionUtil.ConcatenateArray2Dim(resultOne, resultThree));
    
            SendBeanInt("F3", 3, 3);
            Object[][] resultFour = new Object[][] { new Object[] { null, "F3", null } };
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, resultFour);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, EPAssertionUtil.ConcatenateArray2Dim(resultOne, resultThree, resultFour));
    
            SendBeanInt("E3", 0, 3);
            Object[][] resultFive = new Object[][] { new Object[] { "E3", "F3", null } };
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, resultFive);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, EPAssertionUtil.ConcatenateArray2Dim(resultOne, resultThree, resultFive));
        }
    
        [Test]
        public void Test1Stream2HistStarNoSubordinateLeftRight() {
            String expression;
    
            expression = "select s0.id as s0id, h0.val as valh0, h1.val as valh1 from " +
                    "SupportBeanInt(id like 'E%')#keepall as s0 " +
                    " right outer join " +
                    "method:SupportJoinMethods.FetchVal('H0', 2) as h0 " +
                    " on s0.P00 = h0.index " +
                    " right outer join " +
                    "method:SupportJoinMethods.FetchVal('H1', 2) as h1 " +
                    " on s0.P00 = h1.index";
            RunAssertionSeven(expression);
    
            expression = "select s0.id as s0id, h0.val as valh0, h1.val as valh1 from " +
                    "method:SupportJoinMethods.FetchVal('H1', 2) as h1 " +
                    " left outer join " +
                    "SupportBeanInt(id like 'E%')#keepall as s0 " +
                    " on s0.P00 = h1.index" +
                    " right outer join " +
                    "method:SupportJoinMethods.FetchVal('H0', 2) as h0 " +
                    " on s0.P00 = h0.index ";
            RunAssertionSeven(expression);
        }
    
        private void RunAssertionSeven(String expression) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(expression);
            listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            String[] fields = "s0id,valh0,valh1".Split(',');
            Object[][] resultOne = new Object[][]
                                   {
                                       new Object[] { null, "H01", null }, 
                                       new Object[] { null, "H02", null }, 
                                       new Object[] { null, null, "H11" },
                                       new Object[] { null, null, "H12" }
                                   };
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, resultOne);
    
            SendBeanInt("E1", 0);
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, resultOne);
    
            SendBeanInt("E2", 2);
            Object[][] resultTwo = new Object[][] { new Object[] { "E2", "H02", "H12" } };
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, resultTwo);
            Object[][] resultIt = new Object[][] { new Object[] { null, "H01", null }, new Object[] { null, null, "H11" }, new Object[] { "E2", "H02", "H12" } };
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, resultIt);
    
            SendBeanInt("E3", 1);
            resultTwo = new Object[][] { new Object[] { "E3", "H01", "H11" } };
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, resultTwo);
            resultIt = new Object[][] { new Object[] { "E3", "H01", "H11" }, new Object[] { "E2", "H02", "H12" } };
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, resultIt);
    
            SendBeanInt("E4", 1);
            resultTwo = new Object[][] { new Object[] { "E4", "H01", "H11" } };
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, resultTwo);
            resultIt = new Object[][] { new Object[] { "E3", "H01", "H11" }, new Object[] { "E4", "H01", "H11" }, new Object[] { "E2", "H02", "H12" } };
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, resultIt);
        }
    
        private void TryInvalid(String expression, String text) {
            try {
                epService.EPAdministrator.CreateEPL(expression);
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual(ex.Message, text);
            }
        }
    
        private void SendBeanInt(String id, int p00, int p01, int p02, int p03, int p04, int p05) {
            epService.EPRuntime.SendEvent(new SupportBeanInt(id, p00, p01, p02, p03, p04, p05));
        }
    
        private void SendBeanInt(String id, int p00, int p01, int p02, int p03) {
            SendBeanInt(id, p00, p01, p02, p03, -1, -1);
        }
    
        private void SendBeanInt(String id, int p00, int p01, int p02) {
            SendBeanInt(id, p00, p01, p02, -1);
        }
    
        private void SendBeanInt(String id, int p00, int p01) {
            SendBeanInt(id, p00, p01, -1, -1);
        }
    
        private void SendBeanInt(String id, int p00) {
            SendBeanInt(id, p00, -1, -1, -1);
        }
    }
}
