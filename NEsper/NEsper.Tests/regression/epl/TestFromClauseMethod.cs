///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.core.service;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.epl;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
	public class TestFromClauseMethod 
	{
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;

        [SetUp]
	    public void SetUp()
	    {
	        var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportBean>();
	        _epService = EPServiceProviderManager.GetDefaultProvider(config);
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);}
	        _listener = new SupportUpdateListener();
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _listener = null;
	    }

        [Test]
	    public void Test2StreamMaxAggregation() {
            var className = typeof(SupportStaticMethodLib).FullName;
	        string stmtText;

	        // ESPER 556
	        stmtText = "select max(col1) as maxcol1 from SupportBean.std:unique(theString), method:" + className + ".FetchResult100() ";

	        var fields = "maxcol1".Split(',');
	        var stmt = (EPStatementSPI) _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);
	        Assert.IsFalse(stmt.StatementContext.IsStatelessSelect);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new object[][]
	        {
	            new object[] { 9 }
	        });

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new object[][]
            {
                new object[]{ 9 }
            });

            stmt.Dispose();
	    }

        [Test]
	    public void Test2JoinHistoricalSubordinateOuterMultiField()
	    {
            var className = typeof(SupportStaticMethodLib).FullName;
	        string stmtText;

	        // fetchBetween must execute first, fetchIdDelimited is dependent on the result of fetchBetween
	        stmtText = "select intPrimitive,intBoxed,col1,col2 from SupportBean.win:keepall() " +
	                   "left outer join " +
	                   "method:" + className + ".FetchResult100() " +
	                   "on intPrimitive = col1 and intBoxed = col2";

	        var fields = "intPrimitive,intBoxed,col1,col2".Split(',');
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
	        stmt.AddListener(_listener);

	        SendSupportBeanEvent(2, 4);
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new object[][]
            {
                new object[]{ 2, 4, 2, 4 }
            });
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]
	        {
	            new object[]{2, 4, 2, 4}
	        });

            stmt.Dispose();
	    }

        [Test]
	    public void Test2JoinHistoricalSubordinateOuter()
	    {
            var className = typeof(SupportStaticMethodLib).FullName;
	        string stmtText;

	        // fetchBetween must execute first, fetchIdDelimited is dependent on the result of fetchBetween
	        stmtText = "select s0.value as valueOne, s1.value as valueTwo from method:" + className + ".FetchResult12(0) as s0 " +
	                   "left outer join " +
	                   "method:" + className + ".FetchResult23(s0.value) as s1 on s0.value = s1.value";
	        AssertJoinHistoricalSubordinateOuter(stmtText);

	        stmtText = "select s0.value as valueOne, s1.value as valueTwo from " +
	                    "method:" + className + ".FetchResult23(s0.value) as s1 " +
	                    "right outer join " +
	                    "method:" + className + ".FetchResult12(0) as s0 on s0.value = s1.value";
	        AssertJoinHistoricalSubordinateOuter(stmtText);

	        stmtText = "select s0.value as valueOne, s1.value as valueTwo from " +
	                    "method:" + className + ".FetchResult23(s0.value) as s1 " +
	                    "full outer join " +
	                    "method:" + className + ".FetchResult12(0) as s0 on s0.value = s1.value";
	        AssertJoinHistoricalSubordinateOuter(stmtText);

	        stmtText = "select s0.value as valueOne, s1.value as valueTwo from " +
	                    "method:" + className + ".FetchResult12(0) as s0 " +
	                    "full outer join " +
	                    "method:" + className + ".FetchResult23(s0.value) as s1 on s0.value = s1.value";
	        AssertJoinHistoricalSubordinateOuter(stmtText);
	    }

        [Test]
	    public void Test2JoinHistoricalIndependentOuter()
	    {
	        var fields = "valueOne,valueTwo".Split(',');
	        var className = typeof(SupportStaticMethodLib).FullName;
	        string stmtText;

	        // fetchBetween must execute first, fetchIdDelimited is dependent on the result of fetchBetween
	        stmtText = "select s0.value as valueOne, s1.value as valueTwo from method:" + className + ".FetchResult12(0) as s0 " +
	                   "left outer join " +
	                   "method:" + className + ".FetchResult23(0) as s1 on s0.value = s1.value";
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]
	        {
	            new object[]{1, null},
                new object[]{2, 2}
	        });
            stmt.Dispose();

	        stmtText = "select s0.value as valueOne, s1.value as valueTwo from " +
	                    "method:" + className + ".FetchResult23(0) as s1 " +
	                    "right outer join " +
	                    "method:" + className + ".FetchResult12(0) as s0 on s0.value = s1.value";
	        stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]
	        {
	            new object[]{1, null}, 
                new object[]{2, 2}
	        });
            stmt.Dispose();

	        stmtText = "select s0.value as valueOne, s1.value as valueTwo from " +
	                    "method:" + className + ".FetchResult23(0) as s1 " +
	                    "full outer join " +
	                    "method:" + className + ".FetchResult12(0) as s0 on s0.value = s1.value";
	        stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]
	        {
	            new object[]{1, null}, 
                new object[]{2, 2},
                new object[]{null, 3}
	        });
            stmt.Dispose();

	        stmtText = "select s0.value as valueOne, s1.value as valueTwo from " +
	                    "method:" + className + ".FetchResult12(0) as s0 " +
	                    "full outer join " +
	                    "method:" + className + ".FetchResult23(0) as s1 on s0.value = s1.value";
	        stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]
	        {
	            new object[]{1, null},
                new object[]{2, 2},
                new object[]{null, 3}
	        });
            stmt.Dispose();
	    }

	    private void AssertJoinHistoricalSubordinateOuter(string expression)
	    {
	        var fields = "valueOne,valueTwo".Split(',');
	        var stmt = _epService.EPAdministrator.CreateEPL(expression);
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]
	        {
	            new object[]{1, null},
                new object[]{2, 2}
	        });
            stmt.Dispose();
	    }

        [Test]
	    public void Test2JoinHistoricalOnlyDependent()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
	        _epService.EPAdministrator.CreateEPL("create variable int lower");
	        _epService.EPAdministrator.CreateEPL("create variable int upper");
	        var setStmt = _epService.EPAdministrator.CreateEPL("on SupportBean set lower=intPrimitive,upper=intBoxed");
	        Assert.AreEqual(StatementType.ON_SET, ((EPStatementSPI) setStmt).StatementMetadata.StatementType);

            var className = typeof(SupportStaticMethodLib).FullName;
	        string stmtText;

	        // fetchBetween must execute first, fetchIdDelimited is dependent on the result of fetchBetween
	        stmtText = "select value,result from method:" + className + ".FetchBetween(lower, upper), " +
	                                        "method:" + className + ".FetchIdDelimited(value)";
	        AssertJoinHistoricalOnlyDependent(stmtText);

	        stmtText = "select value,result from " +
	                                        "method:" + className + ".FetchIdDelimited(value), " +
	                                        "method:" + className + ".FetchBetween(lower, upper)";
	        AssertJoinHistoricalOnlyDependent(stmtText);
	    }

        [Test]
	    public void Test2JoinHistoricalOnlyIndependent()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
	        _epService.EPAdministrator.CreateEPL("create variable int lower");
	        _epService.EPAdministrator.CreateEPL("create variable int upper");
	        _epService.EPAdministrator.CreateEPL("on SupportBean set lower=intPrimitive,upper=intBoxed");

            var className = typeof(SupportStaticMethodLib).FullName;
	        string stmtText;

	        // fetchBetween must execute first, fetchIdDelimited is dependent on the result of fetchBetween
	        stmtText = "select s0.value as valueOne, s1.value as valueTwo from method:" + className + ".FetchBetween(lower, upper) as s0, " +
	                                        "method:" + className + ".FetchBetweenString(lower, upper) as s1";
	        AssertJoinHistoricalOnlyIndependent(stmtText);

	        stmtText = "select s0.value as valueOne, s1.value as valueTwo from " +
	                                        "method:" + className + ".FetchBetweenString(lower, upper) as s1, " +
	                                        "method:" + className + ".FetchBetween(lower, upper) as s0 ";
	        AssertJoinHistoricalOnlyIndependent(stmtText);
	    }

	    private void AssertJoinHistoricalOnlyIndependent(string expression)
	    {
	        var stmt = _epService.EPAdministrator.CreateEPL(expression);
	        _listener = new SupportUpdateListener();
	        stmt.AddListener(_listener);

	        var fields = "valueOne,valueTwo".Split(',');
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);

	        SendSupportBeanEvent(5, 5);
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]
	        {
	            new object[]{5, "5"}
	        });

	        SendSupportBeanEvent(1, 2);
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]
	        {
	            new object[]{1, "1"}, 
                new object[]{1, "2"},
                new object[]{2, "1"}, 
                new object[]{2, "2"}
	        });

	        SendSupportBeanEvent(0, -1);
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);

            stmt.Dispose();
	        SendSupportBeanEvent(0, -1);
	        Assert.IsFalse(_listener.IsInvoked);
	    }

	    private void AssertJoinHistoricalOnlyDependent(string expression)
	    {
	        var stmt = _epService.EPAdministrator.CreateEPL(expression);
	        _listener = new SupportUpdateListener();
	        stmt.AddListener(_listener);

	        var fields = "value,result".Split(',');
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);

	        SendSupportBeanEvent(5, 5);
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]
	        {
	            new object[]{5, "|5|"}
	        });

	        SendSupportBeanEvent(1, 2);
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]
	        {
	            new object[]{1, "|1|"}, 
                new object[]{2, "|2|"}
	        });

	        SendSupportBeanEvent(0, -1);
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);

	        SendSupportBeanEvent(4, 6);
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]
	        {
	            new object[]{4, "|4|"}, 
                new object[]{5, "|5|"}, 
                new object[]{6, "|6|"}
	        });

            stmt.Dispose();
	        SendSupportBeanEvent(0, -1);
	        Assert.IsFalse(_listener.IsInvoked);
	    }

        [Test]
	    public void TestNoJoinIterateVariables()
	    {
	        _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
	        _epService.EPAdministrator.CreateEPL("create variable int lower");
	        _epService.EPAdministrator.CreateEPL("create variable int upper");
	        _epService.EPAdministrator.CreateEPL("on SupportBean set lower=intPrimitive,upper=intBoxed");

	        // Test int and singlerow
	        var className = typeof(SupportStaticMethodLib).FullName;
	        var stmtText = "select value from method:" + className + ".FetchBetween(lower, upper)";
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        _listener = new SupportUpdateListener();
	        stmt.AddListener(_listener);

	        EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), new string[]{"value"}, null);

	        SendSupportBeanEvent(5, 10);
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), new string[]{"value"}, new object[][]
	        {
	            new object[]{5}, 
                new object[]{6}, 
                new object[]{7}, 
                new object[]{8}, 
                new object[]{9}, 
                new object[]{10}
	        });

	        SendSupportBeanEvent(10, 5);
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), new string[]{"value"}, null);

	        SendSupportBeanEvent(4, 4);
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), new string[]{"value"}, new object[][]
	        {
	            new object[]{4}
	        });

	        stmt.Dispose();
	        Assert.IsFalse(_listener.IsInvoked);
	    }

	    private void RunAssertionReturnTypeMultipleRow(string method) {
	        var epl = "select theString, intPrimitive, mapstring, mapint from " +
	                typeof(SupportBean).Name + ".win:keepall() as s1, " +
	                "method:com.espertech.esper.support.epl.SupportStaticMethodLib." + method;
	        var fields = "theString,intPrimitive,mapstring,mapint".Split(',');
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, null);

	        SendBeanEvent("E1", 0);
	        Assert.IsFalse(_listener.IsInvoked);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, null);

	        SendBeanEvent("E2", -1);
	        Assert.IsFalse(_listener.IsInvoked);
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, null);

	        SendBeanEvent("E3", 1);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"E3", 1, "|E3_0|", 100});
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]
	        {
	            new object[]{"E3", 1, "|E3_0|", 100}
	        });

	        SendBeanEvent("E4", 2);
	        EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new object[][]
	        {
	            new object[]{"E4", 2, "|E4_0|", 100},
                new object[]{"E4", 2, "|E4_1|", 101}
	        });
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]
	        {
	            new object[]{"E3", 1, "|E3_0|", 100}, 
                new object[]{"E4", 2, "|E4_0|", 100},
                new object[]{"E4", 2, "|E4_1|", 101}
	        });

	        SendBeanEvent("E5", 3);
	        EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new object[][]
	        {
	            new object[]{"E5", 3, "|E5_0|", 100}, 
                new object[]{"E5", 3, "|E5_1|", 101}, 
                new object[]{"E5", 3, "|E5_2|", 102}
	        });
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]
            {
                new object[]{"E3", 1, "|E3_0|", 100},
	            new object[]{"E4", 2, "|E4_0|", 100},
                new object[]{"E4", 2, "|E4_1|", 101},
	            new object[]{"E5", 3, "|E5_0|", 100},
                new object[]{"E5", 3, "|E5_1|", 101},
                new object[]{"E5", 3, "|E5_2|", 102}
            });

	        _listener.Reset();
	        stmt.Dispose();
	    }

        [Test]
	    public void TestDifferentReturnTypes()
	    {
	        RunAssertionSingleRowFetch("FetchMap(theString, intPrimitive)");
	        RunAssertionSingleRowFetch("FetchMapEventBean(s1, 'theString', 'intPrimitive')");
	        RunAssertionSingleRowFetch("FetchObjectArrayEventBean(theString, intPrimitive)");
	        RunAssertionSingleRowFetch("FetchPOCOArray(theString, intPrimitive)");
	        RunAssertionSingleRowFetch("FetchPOCOCollection(theString, intPrimitive)");
            RunAssertionSingleRowFetch("FetchPOCOIterator(theString, intPrimitive)");
            RunAssertionSingleRowFetch("FetchPOCOIterable(theString, intPrimitive)");

	        RunAssertionReturnTypeMultipleRow("FetchMapArrayMR(theString, intPrimitive)");
	        RunAssertionReturnTypeMultipleRow("FetchOAArrayMR(theString, intPrimitive)");
	        RunAssertionReturnTypeMultipleRow("FetchPOCOArrayMR(theString, intPrimitive)");
	        RunAssertionReturnTypeMultipleRow("FetchMapCollectionMR(theString, intPrimitive)");
	        RunAssertionReturnTypeMultipleRow("FetchOACollectionMR(theString, intPrimitive)");
	        RunAssertionReturnTypeMultipleRow("FetchPOCOCollectionMR(theString, intPrimitive)");
            RunAssertionReturnTypeMultipleRow("FetchMapIteratorMR(theString, intPrimitive)");
            RunAssertionReturnTypeMultipleRow("FetchMapIterableMR(theString, intPrimitive)");
            RunAssertionReturnTypeMultipleRow("FetchOAIteratorMR(theString, intPrimitive)");
            RunAssertionReturnTypeMultipleRow("FetchOAIterableMR(theString, intPrimitive)");
            RunAssertionReturnTypeMultipleRow("FetchPOCOIteratorMR(theString, intPrimitive)");
            RunAssertionReturnTypeMultipleRow("FetchPOCOIterableMR(theString, intPrimitive)");
        }

	    private void RunAssertionSingleRowFetch(string method) {
	        var epl = "select theString, intPrimitive, mapstring, mapint from " +
	                typeof(SupportBean).Name + " as s1, " +
	                "method:com.espertech.esper.support.epl.SupportStaticMethodLib." + method;
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);
	        var fields = new string[] {"theString", "intPrimitive", "mapstring", "mapint"};

	        SendBeanEvent("E1", 1);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1, "|E1|", 2});

	        SendBeanEvent("E2", 3);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", 3, "|E2|", 4});

	        SendBeanEvent("E3", 0);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"E3", 0, null, null});

	        SendBeanEvent("E4", -1);
	        Assert.IsFalse(_listener.IsInvoked);

	        stmt.Dispose();
	    }

        [Test]
	    public void TestArrayNoArg()
	    {
	        var joinStatement = "select id, theString from " +
	                typeof(SupportBean).Name + ".win:length(3) as s1, " +
	                "method:com.espertech.esper.support.epl.SupportStaticMethodLib.FetchArrayNoArg";
	        var stmt = _epService.EPAdministrator.CreateEPL(joinStatement);
	        TryArrayNoArg(stmt);

	        joinStatement = "select id, theString from " +
	                typeof(SupportBean).Name + ".win:length(3) as s1, " +
	                "method:com.espertech.esper.support.epl.SupportStaticMethodLib.FetchArrayNoArg()";
	        stmt = _epService.EPAdministrator.CreateEPL(joinStatement);
	        TryArrayNoArg(stmt);

	        var model = _epService.EPAdministrator.CompileEPL(joinStatement);
	        Assert.AreEqual(joinStatement, model.ToEPL());
	        stmt = _epService.EPAdministrator.Create(model);
	        TryArrayNoArg(stmt);

	        model = new EPStatementObjectModel();
	        model.SelectClause = SelectClause.Create("id", "theString");
	        model.FromClause = FromClause.Create()
	            .Add(FilterStream.Create(typeof(SupportBean).Name, "s1").AddView("win", "length", Expressions.Constant(3)))
                .Add(MethodInvocationStream.Create(typeof(SupportStaticMethodLib).FullName, "FetchArrayNoArg"));
	        stmt = _epService.EPAdministrator.Create(model);
	        Assert.AreEqual(joinStatement, model.ToEPL());

	        TryArrayNoArg(stmt);
	    }

	    private void TryArrayNoArg(EPStatement stmt)
	    {
	        stmt.AddListener(_listener);
	        var fields = new string[] {"id", "theString"};

	        SendBeanEvent("E1");
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"1", "E1"});

	        SendBeanEvent("E2");
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"1", "E2"});

	        stmt.Dispose();
	    }

        [Test]
	    public void TestArrayWithArg()
	    {
	        var joinStatement = "select irstream id, theString from " +
	                typeof(SupportBean).Name + "().win:length(3) as s1, " +
	                " method:com.espertech.esper.support.epl.SupportStaticMethodLib.FetchArrayGen(intPrimitive)";
	        var stmt = _epService.EPAdministrator.CreateEPL(joinStatement);
	        TryArrayWithArg(stmt);

	        joinStatement = "select irstream id, theString from " +
	                "method:com.espertech.esper.support.epl.SupportStaticMethodLib.FetchArrayGen(intPrimitive) as s0, " +
	                typeof(SupportBean).Name + ".win:length(3)";
	        stmt = _epService.EPAdministrator.CreateEPL(joinStatement);
	        TryArrayWithArg(stmt);

	        var model = _epService.EPAdministrator.CompileEPL(joinStatement);
	        Assert.AreEqual(joinStatement, model.ToEPL());
	        stmt = _epService.EPAdministrator.Create(model);
	        TryArrayWithArg(stmt);

	        model = new EPStatementObjectModel();
	        model.SelectClause = SelectClause.Create("id", "theString").SetStreamSelector(StreamSelector.RSTREAM_ISTREAM_BOTH);
	        model.FromClause = FromClause.Create()
                .Add(MethodInvocationStream.Create(typeof(SupportStaticMethodLib).FullName, "FetchArrayGen", "s0")
	                .AddParameter(Expressions.Property("intPrimitive")))
                    .Add(FilterStream.Create(typeof(SupportBean).Name).AddView("win", "length", Expressions.Constant(3)));

	        stmt = _epService.EPAdministrator.Create(model);
	        Assert.AreEqual(joinStatement, model.ToEPL());

	        TryArrayWithArg(stmt);
	    }

	    private void TryArrayWithArg(EPStatement stmt)
	    {
	        stmt.AddListener(_listener);
	        var fields = new string[] {"id", "theString"};

	        SendBeanEvent("E1", -1);
	        Assert.IsFalse(_listener.IsInvoked);

	        SendBeanEvent("E2", 0);
	        Assert.IsFalse(_listener.IsInvoked);

	        SendBeanEvent("E3", 1);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"A", "E3"});

	        SendBeanEvent("E4", 2);
	        EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new object[][]
	        {
	            new object[]{"A", "E4"},
                new object[]{"B", "E4"}
	        });
	        Assert.IsNull(_listener.LastOldData);
	        _listener.Reset();

	        SendBeanEvent("E5", 3);
	        EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new object[][]
	        {
	            new object[]{"A", "E5"}, 
                new object[]{"B", "E5"}, 
                new object[]{"C", "E5"}
	        });
	        Assert.IsNull(_listener.LastOldData);
	        _listener.Reset();

	        SendBeanEvent("E6", 1);
	        EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new object[][]
	        {
	            new object[]{"A", "E6"}
	        });
	        EPAssertionUtil.AssertPropsPerRow(_listener.LastOldData, fields, new object[][]
	        {
	            new object[]{"A", "E3"}
	        });
	        _listener.Reset();

	        SendBeanEvent("E7", 1);
	        EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new object[][]
	        {
	            new object[]{"A", "E7"}
	        });
	        EPAssertionUtil.AssertPropsPerRow(_listener.LastOldData, fields, new object[][]
	        {
	            new object[]{"A", "E4"},
                new object[]{"B", "E4"}
	        });
	        _listener.Reset();

	        stmt.Dispose();
	    }

        [Test]
	    public void TestObjectNoArg()
	    {
	        var joinStatement = "select id, theString from " +
	                typeof(SupportBean).Name + "().win:length(3) as s1, " +
	                " method:com.espertech.esper.support.epl.SupportStaticMethodLib.FetchObjectNoArg()";

	        var stmt = _epService.EPAdministrator.CreateEPL(joinStatement);
	        stmt.AddListener(_listener);
	        var fields = new string[] {"id", "theString"};

	        SendBeanEvent("E1");
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"2", "E1"});

	        SendBeanEvent("E2");
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"2", "E2"});
	    }

        [Test]
	    public void TestObjectWithArg()
	    {
	        var joinStatement = "select id, theString from " +
	                typeof(SupportBean).Name + "().win:length(3) as s1, " +
	                " method:com.espertech.esper.support.epl.SupportStaticMethodLib.FetchObject(theString)";

	        var stmt = _epService.EPAdministrator.CreateEPL(joinStatement);
	        stmt.AddListener(_listener);
	        var fields = new string[] {"id", "theString"};

	        SendBeanEvent("E1");
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"|E1|", "E1"});

	        SendBeanEvent(null);
	        Assert.IsFalse(_listener.IsInvoked);

	        SendBeanEvent("E2");
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"|E2|", "E2"});
	    }

        [Test]
	    public void TestInvocationTargetEx()
	    {
	        var joinStatement = "select s1.theString from " +
	                typeof(SupportBean).Name + "().win:length(3) as s1, " +
	                " method:com.espertech.esper.support.epl.SupportStaticMethodLib.ThrowExceptionBeanReturn()";

	        _epService.EPAdministrator.CreateEPL(joinStatement);

	        try {
	            SendBeanEvent("E1");
	            Assert.Fail(); // default test configuration rethrows this exception
	        }
	        catch (EPException) {
	            // fine
	        }
	    }

        [Test]
	    public void TestInvalid()
	    {
	        TryInvalid("select * from SupportBean, method:com.espertech.esper.support.epl.SupportStaticMethodLib.FetchArrayGen()",
                        "Error starting statement: Method footprint does not match the number or type of expression parameters, expecting no parameters in method: Could not find static method named 'FetchArrayGen' in class 'com.espertech.esper.support.epl.SupportStaticMethodLib' taking no parameters (nearest match found was 'FetchArrayGen' taking type(s) 'System.Int32') [select * from SupportBean, method:com.espertech.esper.support.epl.SupportStaticMethodLib.FetchArrayGen()]");

	        TryInvalid("select * from SupportBean, method:.abc where 1=2",
	                   "Incorrect syntax near '.' at line 1 column 34, please check the method invocation join within the from clause [select * from SupportBean, method:.abc where 1=2]");

	        TryInvalid("select * from SupportBean, method:com.espertech.esper.support.epl.SupportStaticMethodLib.FetchObjectAndSleep(1)",
                       "Error starting statement: Method footprint does not match the number or type of expression parameters, expecting a method where parameters are typed '" + Name.Clean<int>() + "': Could not find static method named 'FetchObjectAndSleep' in class 'com.espertech.esper.support.epl.SupportStaticMethodLib' with matching parameter number and expected parameter type(s) '" + Name.Clean<int>() + "' (nearest match found was 'FetchObjectAndSleep' taking type(s) 'System.String, System.Int32, System.Int64') [select * from SupportBean, method:com.espertech.esper.support.epl.SupportStaticMethodLib.FetchObjectAndSleep(1)]");

	        TryInvalid("select * from SupportBean, method:com.espertech.esper.support.epl.SupportStaticMethodLib.Sleep(100) where 1=2",
	                   "Error starting statement: Invalid return type for static method 'Sleep' of class 'com.espertech.esper.support.epl.SupportStaticMethodLib', expecting a class [select * from SupportBean, method:com.espertech.esper.support.epl.SupportStaticMethodLib.Sleep(100) where 1=2]");

	        TryInvalid("select * from SupportBean, method:AClass. where 1=2",
	                   "Incorrect syntax near 'where' (a reserved keyword) expecting an identifier but found 'where' at line 1 column 42, please check the view specifications within the from clause [select * from SupportBean, method:AClass. where 1=2]");

	        TryInvalid("select * from SupportBean, method:Dummy.abc where 1=2",
	                   "Error starting statement: Could not load class by name 'Dummy', please check imports [select * from SupportBean, method:Dummy.abc where 1=2]");

	        TryInvalid("select * from SupportBean, method:Math where 1=2",
	                   "No method name specified for method-based join [select * from SupportBean, method:Math where 1=2]");

	        TryInvalid("select * from SupportBean, method:Dummy.dummy().win:length(100) where 1=2",
	                   "Error starting statement: Method data joins do not allow views onto the data, view 'win:length' is not valid in this context [select * from SupportBean, method:Dummy.dummy().win:length(100) where 1=2]");

	        TryInvalid("select * from SupportBean, method:com.espertech.esper.support.epl.SupportStaticMethodLib.Dummy where 1=2",
                       "Error starting statement: Could not find public static method named 'Dummy' in class 'com.espertech.esper.support.epl.SupportStaticMethodLib' [select * from SupportBean, method:com.espertech.esper.support.epl.SupportStaticMethodLib.Dummy where 1=2]");

	        TryInvalid("select * from SupportBean, method:com.espertech.esper.support.epl.SupportStaticMethodLib.MinusOne() where 1=2",
                       "Error starting statement: Invalid return type for static method 'MinusOne' of class 'com.espertech.esper.support.epl.SupportStaticMethodLib', expecting a class [select * from SupportBean, method:com.espertech.esper.support.epl.SupportStaticMethodLib.MinusOne() where 1=2]");

	        TryInvalid("select * from SupportBean, xyz:com.espertech.esper.support.epl.SupportStaticMethodLib.FetchArrayNoArg() where 1=2",
	                   "Expecting keyword 'method', found 'xyz' [select * from SupportBean, xyz:com.espertech.esper.support.epl.SupportStaticMethodLib.FetchArrayNoArg() where 1=2]");

	        TryInvalid("select * from method:com.espertech.esper.support.epl.SupportStaticMethodLib.FetchBetween(s1.value, s1.value) as s0, method:com.espertech.esper.support.epl.SupportStaticMethodLib.FetchBetween(s0.value, s0.value) as s1",
	                   "Error starting statement: Circular dependency detected between historical streams [select * from method:com.espertech.esper.support.epl.SupportStaticMethodLib.FetchBetween(s1.value, s1.value) as s0, method:com.espertech.esper.support.epl.SupportStaticMethodLib.FetchBetween(s0.value, s0.value) as s1]");

	        TryInvalid("select * from method:com.espertech.esper.support.epl.SupportStaticMethodLib.FetchBetween(s0.value, s0.value) as s0, method:com.espertech.esper.support.epl.SupportStaticMethodLib.FetchBetween(s0.value, s0.value) as s1",
	                   "Error starting statement: Parameters for historical stream 0 indicate that the stream is subordinate to itself as stream parameters originate in the same stream [select * from method:com.espertech.esper.support.epl.SupportStaticMethodLib.FetchBetween(s0.value, s0.value) as s0, method:com.espertech.esper.support.epl.SupportStaticMethodLib.FetchBetween(s0.value, s0.value) as s1]");

	        TryInvalid("select * from method:com.espertech.esper.support.epl.SupportStaticMethodLib.FetchBetween(s0.value, s0.value) as s0",
	                   "Error starting statement: Parameters for historical stream 0 indicate that the stream is subordinate to itself as stream parameters originate in the same stream [select * from method:com.espertech.esper.support.epl.SupportStaticMethodLib.FetchBetween(s0.value, s0.value) as s0]");

	        _epService.EPAdministrator.Configuration.AddImport(typeof(SupportMethodInvocationJoinInvalid));
	        TryInvalid("select * from method:SupportMethodInvocationJoinInvalid.ReadRowNoMetadata()",
                    "Error starting statement: Could not find getter method for method invocation, expected a method by name 'ReadRowNoMetadataMetadata' accepting no parameters [select * from method:SupportMethodInvocationJoinInvalid.ReadRowNoMetadata()]");

	        TryInvalid("select * from method:SupportMethodInvocationJoinInvalid.ReadRowWrongMetadata()",
                    "Error starting statement: Getter method 'ReadRowWrongMetadataMetadata' does not return " + Name.Of<IDictionary<string, object>>() + " [select * from method:SupportMethodInvocationJoinInvalid.ReadRowWrongMetadata()]");
	    }

	    private void TryInvalid(string stmt, string message)
	    {
	        try
	        {
	            _epService.EPAdministrator.CreateEPL(stmt);
	            Assert.Fail();
	        }
	        catch (EPStatementException ex)
	        {
	            Assert.AreEqual(message, ex.Message);
	        }
	    }

	    private void SendBeanEvent(string theString)
	    {
	        var bean = new SupportBean();
	        bean.TheString = theString;
	        _epService.EPRuntime.SendEvent(bean);
	    }

	    private void SendBeanEvent(string theString, int intPrimitive)
	    {
	        var bean = new SupportBean();
	        bean.TheString = theString;
	        bean.IntPrimitive = intPrimitive;
	        _epService.EPRuntime.SendEvent(bean);
	    }

	    private void SendSupportBeanEvent(int intPrimitive, int intBoxed)
	    {
	        var bean = new SupportBean();
	        bean.IntPrimitive = intPrimitive;
	        bean.IntBoxed = intBoxed;
	        _epService.EPRuntime.SendEvent(bean);
	    }
	}
} // end of namespace
