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
using com.espertech.esper.support.epl;

using NUnit.Framework;


namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestFromClauseMethodNStream
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp() {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.LoggingConfig.IsEnableQueryPlan = true;
            config.AddEventType(typeof(SupportBeanInt));
            config.AddImport(typeof(SupportJoinMethods).FullName);
            config.AddVariable("var1", typeof(int?), 0);
            config.AddVariable("var2", typeof(int?), 0);
            config.AddVariable("var3", typeof(int?), 0);
            config.AddVariable("var4", typeof(int?), 0);
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _listener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }
    
        [Test]
        public void Test1Stream2HistStarSubordinateCartesianLast() {
            String expression;
    
            expression = "select s0.Id as id, h0.val as valh0, h1.val as valh1 " +
                    "from SupportBeanInt.std:lastevent() as s0, " +
                    "method:SupportJoinMethods.FetchVal('H0', p00) as h0, " +
                    "method:SupportJoinMethods.FetchVal('H1', p01) as h1 " +
                    "order by h0.val, h1.val";
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(expression);
            _listener = new SupportUpdateListener();
            stmt.Events += _listener.Update;
    
            String[] fields = "id,valh0,valh1".Split(',');
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
    
            SendBeanInt("E1", 1, 1);
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new Object[][]{new Object[] {"E1", "H01", "H11"}});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new Object[][]{new Object[] {"E1", "H01", "H11"}});
    
            SendBeanInt("E2", 2, 0);
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, null);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
    
            SendBeanInt("E3", 0, 1);
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, null);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
    
            SendBeanInt("E3", 2, 2);
            Object[][] result = new Object[][]{new Object[] {"E3", "H01", "H11"}, new Object[] {"E3", "H01", "H12"}, new Object[] {"E3", "H02", "H11"}, new Object[] {"E3", "H02", "H12"}};
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, result);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, result);
    
            SendBeanInt("E4", 2, 1);
            result = new Object[][]{new Object[] {"E4", "H01", "H11"}, new Object[] {"E4", "H02", "H11"}};
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, result);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, result);
        }
    
        [Test]
        public void Test1Stream2HistStarSubordinateJoinedKeepall() {
            String expression;
    
            expression = "select s0.Id as id, h0.val as valh0, h1.val as valh1 " +
                    "from SupportBeanInt.win:keepall() as s0, " +
                    "method:SupportJoinMethods.FetchVal('H0', p00) as h0, " +
                    "method:SupportJoinMethods.FetchVal('H1', p01) as h1 " +
                    "where h0.index = h1.index and h0.index = p02";
            RunAssertionOne(expression);
    
            expression = "select s0.Id as id, h0.val as valh0, h1.val as valh1   from " +
                    "method:SupportJoinMethods.FetchVal('H1', p01) as h1, " +
                    "method:SupportJoinMethods.FetchVal('H0', p00) as h0, " +
                    "SupportBeanInt.win:keepall() as s0 " +
                    "where h0.index = h1.index and h0.index = p02";
            RunAssertionOne(expression);
        }
    
        private void RunAssertionOne(String expression) {
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(expression);
            _listener = new SupportUpdateListener();
            stmt.Events += _listener.Update;
    
            String[] fields = "id,valh0,valh1".Split(',');
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
    
            SendBeanInt("E1", 20, 20, 3);
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new Object[][]{new Object[] {"E1", "H03", "H13"}});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new Object[][]{new Object[] {"E1", "H03", "H13"}});
    
            SendBeanInt("E2", 20, 20, 21);
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, null);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new Object[][]{new Object[] {"E1", "H03", "H13"}});
    
            SendBeanInt("E3", 4, 4, 2);
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new Object[][]{new Object[] {"E3", "H02", "H12"}});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new Object[][]{new Object[] {"E1", "H03", "H13"}, new Object[] {"E3", "H02", "H12"}});
    
            stmt.Dispose();
        }
    
        [Test]
        public void Test1Stream2HistForwardSubordinate() {
            String expression;
    
            expression = "select s0.Id as id, h0.val as valh0, h1.val as valh1 " +
                    "from SupportBeanInt.win:keepall() as s0, " +
                    "method:SupportJoinMethods.FetchVal('H0', p00) as h0, " +
                    "method:SupportJoinMethods.FetchVal(h0.val, p01) as h1 " +
                    "order by h0.val, h1.val";
            RunAssertionTwo(expression);
    
            expression = "select s0.Id as id, h0.val as valh0, h1.val as valh1 from " +
                    "method:SupportJoinMethods.FetchVal(h0.val, p01) as h1, " +
                    "SupportBeanInt.win:keepall() as s0, " +
                    "method:SupportJoinMethods.FetchVal('H0', p00) as h0 " +
                    "order by h0.val, h1.val";
            RunAssertionTwo(expression);
        }
    
        private void RunAssertionTwo(String expression) {
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(expression);
            _listener = new SupportUpdateListener();
            stmt.Events += _listener.Update;
    
            String[] fields = "id,valh0,valh1".Split(',');
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
    
            SendBeanInt("E1", 1, 1);
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new Object[][]{new Object[] {"E1", "H01", "H011"}});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new Object[][]{new Object[] {"E1", "H01", "H011"}});
    
            SendBeanInt("E2", 0, 1);
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, null);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new Object[][]{new Object[] {"E1", "H01", "H011"}});
    
            SendBeanInt("E3", 1, 0);
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, null);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new Object[][]{new Object[] {"E1", "H01", "H011"}});
    
            SendBeanInt("E4", 2, 2);
            Object[][] result = { new Object[] { "E4", "H01", "H011" }, new Object[] { "E4", "H01", "H012" }, new Object[] { "E4", "H02", "H021" }, new Object[] { "E4", "H02", "H022" } };
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, result);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, EPAssertionUtil.ConcatenateArray2Dim(result, new Object[][]{new Object[] {"E1", "H01", "H011"}}));
        }
    
        [Test]
        public void Test1Stream3HistStarSubordinateCartesianLast() {
            String expression;
    
            expression = "select s0.Id as id, h0.val as valh0, h1.val as valh1, h2.val as valh2 " +
                    "from SupportBeanInt.std:lastevent() as s0, " +
                    "method:SupportJoinMethods.FetchVal('H0', p00) as h0, " +
                    "method:SupportJoinMethods.FetchVal('H1', p01) as h1, " +
                    "method:SupportJoinMethods.FetchVal('H2', p02) as h2 " +
                    "order by h0.val, h1.val, h2.val";
            RunAssertionThree(expression);
    
            expression = "select s0.Id as id, h0.val as valh0, h1.val as valh1, h2.val as valh2 from " +
                    "method:SupportJoinMethods.FetchVal('H2', p02) as h2, " +
                    "method:SupportJoinMethods.FetchVal('H1', p01) as h1, " +
                    "method:SupportJoinMethods.FetchVal('H0', p00) as h0, " +
                    "SupportBeanInt.std:lastevent() as s0 " +
                    "order by h0.val, h1.val, h2.val";
            RunAssertionThree(expression);
        }
    
        private void RunAssertionThree(String expression) {
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(expression);
            _listener = new SupportUpdateListener();
            stmt.Events += _listener.Update;
    
            String[] fields = "id,valh0,valh1,valh2".Split(',');
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
    
            SendBeanInt("E1", 1, 1, 1);
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new Object[][]{new Object[] {"E1", "H01", "H11", "H21"}});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new Object[][]{new Object[] {"E1", "H01", "H11", "H21"}});
    
            SendBeanInt("E2", 1, 1, 2);
            Object[][] result = new Object[][]{new Object[] {"E2", "H01", "H11", "H21"}, new Object[] {"E2", "H01", "H11", "H22"}};
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, result);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, result);
        }
    
        [Test]
        public void Test1Stream3HistForwardSubordinate() {
            String expression;
    
            expression = "select s0.Id as id, h0.val as valh0, h1.val as valh1, h2.val as valh2 " +
                    "from SupportBeanInt.win:keepall() as s0, " +
                    "method:SupportJoinMethods.FetchVal('H0', p00) as h0, " +
                    "method:SupportJoinMethods.FetchVal('H1', p01) as h1, " +
                    "method:SupportJoinMethods.FetchVal(h0.val||'H2', p02) as h2 " +
                    " where h0.index = h1.index and h1.index = h2.index and h2.index = p03";
            RunAssertionFour(expression);
    
            expression = "select s0.Id as id, h0.val as valh0, h1.val as valh1, h2.val as valh2 from " +
                    "method:SupportJoinMethods.FetchVal(h0.val||'H2', p02) as h2, " +
                    "method:SupportJoinMethods.FetchVal('H0', p00) as h0, " +
                    "SupportBeanInt.win:keepall() as s0, " +
                    "method:SupportJoinMethods.FetchVal('H1', p01) as h1 " +
                    " where h0.index = h1.index and h1.index = h2.index and h2.index = p03";
            RunAssertionFour(expression);
        }
    
        private void RunAssertionFour(String expression) {
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(expression);
            _listener = new SupportUpdateListener();
            stmt.Events += _listener.Update;
    
            String[] fields = "id,valh0,valh1,valh2".Split(',');
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
    
            SendBeanInt("E1", 2, 2, 2, 1);
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new Object[][]{new Object[] {"E1", "H01", "H11", "H01H21"}});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new Object[][]{new Object[] {"E1", "H01", "H11", "H01H21"}});
    
            SendBeanInt("E2", 4, 4, 4, 3);
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new Object[][]{new Object[] {"E2", "H03", "H13", "H03H23"}});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new Object[][]{new Object[] {"E1", "H01", "H11", "H01H21"}, new Object[] {"E2", "H03", "H13", "H03H23"}});
        }
    
        [Test]
        public void Test1Stream3HistChainSubordinate() {
            String expression;
    
            expression = "select s0.Id as id, h0.val as valh0, h1.val as valh1, h2.val as valh2 " +
                    "from SupportBeanInt.win:keepall() as s0, " +
                    "method:SupportJoinMethods.FetchVal('H0', p00) as h0, " +
                    "method:SupportJoinMethods.FetchVal(h0.val||'H1', p01) as h1, " +
                    "method:SupportJoinMethods.FetchVal(h1.val||'H2', p02) as h2 " +
                    " where h0.index = h1.index and h1.index = h2.index and h2.index = p03";
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(expression);
            _listener = new SupportUpdateListener();
            stmt.Events += _listener.Update;
    
            String[] fields = "id,valh0,valh1,valh2".Split(',');
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
    
            SendBeanInt("E2", 4, 4, 4, 3);
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new Object[][]{new Object[] {"E2", "H03", "H03H13", "H03H13H23"}});
    
            SendBeanInt("E2", 4, 4, 4, 5);
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, null);
    
            SendBeanInt("E2", 4, 4, 0, 1);
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, null);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new Object[][]{new Object[] {"E2", "H03", "H03H13", "H03H13H23"}});
        }
    
        [Test]
        public void Test2Stream2HistStarSubordinate() {
            String expression;
    
            expression = "select s0.Id as ids0, s1.Id as ids1, h0.val as valh0, h1.val as valh1 " +
                    "from SupportBeanInt(id like 'S0%').win:keepall() as s0, " +
                    "SupportBeanInt(id like 'S1%').std:lastevent() as s1, " +
                    "method:SupportJoinMethods.FetchVal(s0.Id||'H1', s0.P00) as h0, " +
                    "method:SupportJoinMethods.FetchVal(s1.Id||'H2', s1.P00) as h1 " +
                    "order by s0.Id asc";
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(expression);
            _listener = new SupportUpdateListener();
            stmt.Events += _listener.Update;
    
            String[] fields = "ids0,ids1,valh0,valh1".Split(',');
            SendBeanInt("S00", 1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
            Assert.IsFalse(_listener.IsInvoked);
    
            SendBeanInt("S10", 1);
            Object[][] resultOne = new Object[][]{new Object[] {"S00", "S10", "S00H11", "S10H21"}};
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, resultOne);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, resultOne);
    
            SendBeanInt("S01", 1);
            Object[][] resultTwo = new Object[][]{new Object[] {"S01", "S10", "S01H11", "S10H21"}};
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, resultTwo);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, EPAssertionUtil.ConcatenateArray2Dim(resultOne, resultTwo));
    
            SendBeanInt("S11", 1);
            Object[][] resultThree = new Object[][]{new Object[] {"S00", "S11", "S00H11", "S11H21"}, new Object[] {"S01", "S11", "S01H11", "S11H21"}};
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, resultThree);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, EPAssertionUtil.ConcatenateArray2Dim(resultThree));
        }
    
        [Test]
        public void Test3Stream1HistSubordinate() {
            String expression;
    
            expression = "select s0.Id as ids0, s1.Id as ids1, s2.Id as ids2, h0.val as valh0 " +
                    "from SupportBeanInt(id like 'S0%').win:keepall() as s0, " +
                    "SupportBeanInt(id like 'S1%').std:lastevent() as s1, " +
                    "SupportBeanInt(id like 'S2%').std:lastevent() as s2, " +
                    "method:SupportJoinMethods.FetchVal(s1.Id||s2.Id||'H1', s0.P00) as h0 " +
                    "order by s0.Id, s1.Id, s2.Id, h0.val";
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(expression);
            _listener = new SupportUpdateListener();
            stmt.Events += _listener.Update;
    
            String[] fields = "ids0,ids1,ids2,valh0".Split(',');
            SendBeanInt("S00", 2);
            SendBeanInt("S10", 1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
            Assert.IsFalse(_listener.IsInvoked);
    
            SendBeanInt("S20", 1);
            Object[][] resultOne = new Object[][]{new Object[] {"S00", "S10", "S20", "S10S20H11"}, new Object[] {"S00", "S10", "S20", "S10S20H12"}};
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, resultOne);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, resultOne);
    
            SendBeanInt("S01", 1);
            Object[][] resultTwo = new Object[][]{new Object[] {"S01", "S10", "S20", "S10S20H11"}};
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, resultTwo);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, EPAssertionUtil.ConcatenateArray2Dim(resultOne, resultTwo));
    
            SendBeanInt("S21", 1);
            Object[][] resultThree = new Object[][]{new Object[] {"S00", "S10", "S21", "S10S21H11"}, new Object[] {"S00", "S10", "S21", "S10S21H12"}, new Object[] {"S01", "S10", "S21", "S10S21H11"}};
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, resultThree);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, EPAssertionUtil.ConcatenateArray2Dim(resultThree));
        }
    
        [Test]
        public void Test3HistPureNoSubordinate() {
            _epService.EPAdministrator.CreateEPL("on SupportBeanInt set var1=p00, var2=p01, var3=p02, var4=p03");
    
            String expression;
            expression = "select h0.val as valh0, h1.val as valh1, h2.val as valh2 from " +
                    "method:SupportJoinMethods.FetchVal('H0', var1) as h0," +
                    "method:SupportJoinMethods.FetchVal('H1', var2) as h1," +
                    "method:SupportJoinMethods.FetchVal('H2', var3) as h2";
            RunAssertionFive(expression);
    
            expression = "select h0.val as valh0, h1.val as valh1, h2.val as valh2 from " +
                    "method:SupportJoinMethods.FetchVal('H2', var3) as h2," +
                    "method:SupportJoinMethods.FetchVal('H1', var2) as h1," +
                    "method:SupportJoinMethods.FetchVal('H0', var1) as h0";
            RunAssertionFive(expression);
        }
    
        private void RunAssertionFive(String expression) {
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(expression);
            _listener = new SupportUpdateListener();
            stmt.Events += _listener.Update;
            String[] fields = "valh0,valh1,valh2".Split(',');
    
            SendBeanInt("S00", 1, 1, 1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new Object[][]{new Object[] {"H01", "H11", "H21"}});
    
            SendBeanInt("S01", 0, 1, 1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
    
            SendBeanInt("S02", 1, 1, 0);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
    
            SendBeanInt("S03", 1, 1, 2);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new Object[][]{new Object[] {"H01", "H11", "H21"}, new Object[] {"H01", "H11", "H22"}});
    
            SendBeanInt("S04", 2, 2, 1);
            Object[][] result = new Object[][]{new Object[] {"H01", "H11", "H21"}, new Object[] {"H02", "H11", "H21"}, new Object[] {"H01", "H12", "H21"}, new Object[] {"H02", "H12", "H21"}};
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, result);
        }
    
        [Test]
        public void Test3Hist1Subordinate() {
            _epService.EPAdministrator.CreateEPL("on SupportBeanInt set var1=p00, var2=p01, var3=p02, var4=p03");
    
            String expression;
            expression = "select h0.val as valh0, h1.val as valh1, h2.val as valh2 from " +
                    "method:SupportJoinMethods.FetchVal('H0', var1) as h0," +
                    "method:SupportJoinMethods.FetchVal('H1', var2) as h1," +
                    "method:SupportJoinMethods.FetchVal(h0.val||'-H2', var3) as h2";
            RunAssertionSix(expression);
    
            expression = "select h0.val as valh0, h1.val as valh1, h2.val as valh2 from " +
                    "method:SupportJoinMethods.FetchVal(h0.val||'-H2', var3) as h2," +
                    "method:SupportJoinMethods.FetchVal('H1', var2) as h1," +
                    "method:SupportJoinMethods.FetchVal('H0', var1) as h0";
            RunAssertionSix(expression);
        }
    
        private void RunAssertionSix(String expression) {
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(expression);
            _listener = new SupportUpdateListener();
            stmt.Events += _listener.Update;
            String[] fields = "valh0,valh1,valh2".Split(',');
    
            SendBeanInt("S00", 1, 1, 1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new Object[][]{new Object[] {"H01", "H11", "H01-H21"}});
    
            SendBeanInt("S01", 0, 1, 1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
    
            SendBeanInt("S02", 1, 1, 0);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
    
            SendBeanInt("S03", 1, 1, 2);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new Object[][]{new Object[] {"H01", "H11", "H01-H21"}, new Object[] {"H01", "H11", "H01-H22"}});
    
            SendBeanInt("S04", 2, 2, 1);
            Object[][] result = new Object[][]{new Object[] {"H01", "H11", "H01-H21"}, new Object[] {"H02", "H11", "H02-H21"}, new Object[] {"H01", "H12", "H01-H21"}, new Object[] {"H02", "H12", "H02-H21"}};
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, result);
        }
    
        [Test]
        public void Test3Hist2SubordinateChain() {
            _epService.EPAdministrator.CreateEPL("on SupportBeanInt set var1=p00, var2=p01, var3=p02, var4=p03");
    
            String expression;
            expression = "select h0.val as valh0, h1.val as valh1, h2.val as valh2 from " +
                    "method:SupportJoinMethods.FetchVal('H0', var1) as h0," +
                    "method:SupportJoinMethods.FetchVal(h0.val||'-H1', var2) as h1," +
                    "method:SupportJoinMethods.FetchVal(h1.val||'-H2', var3) as h2";
            RunAssertionSeven(expression);
    
            expression = "select h0.val as valh0, h1.val as valh1, h2.val as valh2 from " +
                    "method:SupportJoinMethods.FetchVal(h1.val||'-H2', var3) as h2," +
                    "method:SupportJoinMethods.FetchVal(h0.val||'-H1', var2) as h1," +
                    "method:SupportJoinMethods.FetchVal('H0', var1) as h0";
            RunAssertionSeven(expression);
        }
    
        private void RunAssertionSeven(String expression) {
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(expression);
            _listener = new SupportUpdateListener();
            stmt.Events += _listener.Update;
            String[] fields = "valh0,valh1,valh2".Split(',');
    
            SendBeanInt("S00", 1, 1, 1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new Object[][]{new Object[] {"H01", "H01-H11", "H01-H11-H21"}});
    
            SendBeanInt("S01", 0, 1, 1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
    
            SendBeanInt("S02", 1, 1, 0);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
    
            SendBeanInt("S03", 1, 1, 2);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new Object[][]{new Object[] {"H01", "H01-H11", "H01-H11-H21"}, new Object[] {"H01", "H01-H11", "H01-H11-H22"}});
    
            SendBeanInt("S04", 2, 2, 1);
            Object[][] result = new Object[][]{new Object[] {"H01", "H01-H11", "H01-H11-H21"}, new Object[] {"H02", "H02-H11", "H02-H11-H21"}, new Object[] {"H01", "H01-H12", "H01-H12-H21"}, new Object[] {"H02", "H02-H12", "H02-H12-H21"}};
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, result);
        }
    
        private void SendBeanInt(String id, int p00, int p01, int p02, int p03) {
            _epService.EPRuntime.SendEvent(new SupportBeanInt(id, p00, p01, p02, p03, -1, -1));
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
