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
using com.espertech.esper.compat;
using com.espertech.esper.core.service;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
	public class TestAggregateFiltered 
	{
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;

        [SetUp]
	    public void SetUp()
	    {
	        _listener = new SupportUpdateListener();
	        _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().Name);}

	        _epService.EPAdministrator.Configuration.AddEventType(typeof(BlackWhiteEvent));
	        _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean));
	        _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBeanNumeric));
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _listener = null;
	    }

        [Test]
	    public void TestBlackWhitePercent()
	    {
	        var fields = "cb,cnb,c,pct".Split(',');
	        var epl = "select count(*,IsBlack) as cb, count(*,not IsBlack) as cnb, count(*) as c, count(*,IsBlack)/count(*) as pct from BlackWhiteEvent.win:length(3)";
	        var stmt = (EPStatementSPI) _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);
	        Assert.IsFalse(stmt.StatementContext.IsStatelessSelect);

	        _epService.EPRuntime.SendEvent(new BlackWhiteEvent(true));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{1L, 0L, 1L, 1d});

	        _epService.EPRuntime.SendEvent(new BlackWhiteEvent(false));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{1L, 1L, 2L, 0.5d});

	        _epService.EPRuntime.SendEvent(new BlackWhiteEvent(false));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{1L, 2L, 3L, 1 / 3d});

	        _epService.EPRuntime.SendEvent(new BlackWhiteEvent(false));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{0L, 3L, 3L, 0d});

	        SupportModelHelper.CompileCreate(_epService, epl);
	        SupportModelHelper.CompileCreate(_epService, "select count(distinct IsBlack,not IsBlack), count(IsBlack,IsBlack) from BlackWhiteEvent");
	    }

        [Test]
	    public void TestCountVariations()
	    {
	        var fields = "c1,c2".Split(',');
	        var stmt = _epService.EPAdministrator.CreateEPL("select " +
	                "count(IntBoxed, BoolPrimitive) as c1," +
	                "count(distinct IntBoxed, BoolPrimitive) as c2 " +
	                "from SupportBean.win:length(3)");
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(MakeBean(100, true));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{1L, 1L});

	        _epService.EPRuntime.SendEvent(MakeBean(100, true));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{2L, 1L});

	        _epService.EPRuntime.SendEvent(MakeBean(101, false));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{2L, 1L});

	        _epService.EPRuntime.SendEvent(MakeBean(102, true));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{2L, 2L});

	        _epService.EPRuntime.SendEvent(MakeBean(103, false));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{1L, 1L});

	        _epService.EPRuntime.SendEvent(MakeBean(104, false));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{1L, 1L});

	        _epService.EPRuntime.SendEvent(MakeBean(105, false));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{0L, 0L});
	    }

        [Test]
	    public void TestAllAggFunctions()
        {
	        var fields = "cavedev,cavg,cmax,cmedian,cmin,cstddev,csum,cfmaxever,cfminever".Split(',');
	        var stmt = (EPStatementSPI) _epService.EPAdministrator.CreateEPL("select " +
	                "avedev(IntBoxed, BoolPrimitive) as cavedev," +
	                "avg(IntBoxed, BoolPrimitive) as cavg, " +
	                "fmax(IntBoxed, BoolPrimitive) as cmax, " +
	                "median(IntBoxed, BoolPrimitive) as cmedian, " +
	                "fmin(IntBoxed, BoolPrimitive) as cmin, " +
	                "stddev(IntBoxed, BoolPrimitive) as cstddev, " +
	                "sum(IntBoxed, BoolPrimitive) as csum," +
	                "fmaxever(IntBoxed, BoolPrimitive) as cfmaxever, " +
	                "fminever(IntBoxed, BoolPrimitive) as cfminever " +
	                "from SupportBean.win:length(3)");
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(MakeBean(100, false));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{null, null, null, null, null, null, null, null, null});

	        _epService.EPRuntime.SendEvent(MakeBean(10, true));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{0.0d, 10.0, 10, 10.0, 10, null, 10, 10, 10});

	        _epService.EPRuntime.SendEvent(MakeBean(11, false));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{0.0d, 10.0, 10, 10.0, 10, null, 10, 10, 10});

	        _epService.EPRuntime.SendEvent(MakeBean(20, true));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{5.0d, 15.0, 20, 15.0, 10, 7.0710678118654755, 30, 20, 10});

	        _epService.EPRuntime.SendEvent(MakeBean(30, true));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{5.0d, 25.0, 30, 25.0, 20, 7.0710678118654755, 50, 30, 10});

	        // Test all remaining types of "sum"
	        stmt.Dispose();
	        fields = "c1,c2,c3,c4".Split(',');
	        stmt = (EPStatementSPI) _epService.EPAdministrator.CreateEPL("select " +
	                "sum(FloatPrimitive, BoolPrimitive) as c1," +
	                "sum(DoublePrimitive, BoolPrimitive) as c2, " +
	                "sum(LongPrimitive, BoolPrimitive) as c3, " +
	                "sum(ShortPrimitive, BoolPrimitive) as c4 " +
	                "from SupportBean.win:length(2)");
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(MakeBean(2f, 3d, 4L, (short) 5, false));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{null, null, null, null});

	        _epService.EPRuntime.SendEvent(MakeBean(3f, 4d, 5L, (short) 6, true));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{3f, 4d, 5L, 6});

	        _epService.EPRuntime.SendEvent(MakeBean(4f, 5d, 6L, (short) 7, true));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{7f, 9d, 11L, 13});

	        _epService.EPRuntime.SendEvent(MakeBean(1f, 1d, 1L, (short) 1, true));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{5f, 6d, 7L, 8});

	        // Test min/max-ever
	        stmt.Dispose();
	        fields = "c1,c2".Split(',');
	        stmt = (EPStatementSPI) _epService.EPAdministrator.CreateEPL("select " +
	                "fmax(IntBoxed, BoolPrimitive) as c1," +
	                "fmin(IntBoxed, BoolPrimitive) as c2 " +
	                "from SupportBean");
	        stmt.AddListener(_listener);
	        Assert.IsFalse(stmt.StatementContext.IsStatelessSelect);

	        _epService.EPRuntime.SendEvent(MakeBean(10, true));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{10, 10});

	        _epService.EPRuntime.SendEvent(MakeBean(20, true));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{20, 10});

	        _epService.EPRuntime.SendEvent(MakeBean(8, false));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{20, 10});

	        _epService.EPRuntime.SendEvent(MakeBean(7, true));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{20, 7});

	        _epService.EPRuntime.SendEvent(MakeBean(30, false));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{20, 7});

	        _epService.EPRuntime.SendEvent(MakeBean(40, true));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{40, 7});

	        // test decimal
	        stmt.Dispose();
	        fields = "c1,c2,c3".Split(',');
            stmt = (EPStatementSPI) _epService.EPAdministrator.CreateEPL(
                "select " +
                "avg(DecimalOne, IntOne < 100) as c1," +
                "sum(DecimalOne, IntOne < 100) as c2, " +
                "sum(IntOne, IntOne < 100) as c3 " +
                "from SupportBeanNumeric.win:length(2)");
            stmt.AddListener(_listener);

            _epService.EPRuntime.SendEvent(new SupportBeanNumeric(10, 20.0m));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { 20.0m, 20.0m, 10 });

            _epService.EPRuntime.SendEvent(new SupportBeanNumeric(101, 101.0m));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { 20.0m, 20.0m, 10 });

            _epService.EPRuntime.SendEvent(new SupportBeanNumeric(20, 40.0m));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { 40.0m, 40.0m, 20 });

            _epService.EPRuntime.SendEvent(new SupportBeanNumeric(30, 50.0m));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { 45.0m, 90.0m, 50 });
    
            stmt.Dispose();
	        var epl = "select " +
	                "avedev(distinct IntBoxed,BoolPrimitive) as cavedev, " +
	                "avg(distinct IntBoxed,BoolPrimitive) as cavg, " +
	                "fmax(distinct IntBoxed,BoolPrimitive) as cmax, " +
	                "median(distinct IntBoxed,BoolPrimitive) as cmedian, " +
	                "fmin(distinct IntBoxed,BoolPrimitive) as cmin, " +
	                "stddev(distinct IntBoxed,BoolPrimitive) as cstddev, " +
	                "sum(distinct IntBoxed,BoolPrimitive) as csum " +
	                "from SupportBean.win:length(3)";
	        stmt = (EPStatementSPI) _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        RunAssertionDistinct();

	        // test SODA
            stmt.Dispose();
	        var model = _epService.EPAdministrator.CompileEPL(epl);
	        Assert.AreEqual(epl, model.ToEPL());
	        stmt = (EPStatementSPI) _epService.EPAdministrator.Create(model);
	        stmt.AddListener(_listener);
	        Assert.AreEqual(epl, stmt.Text);

	        RunAssertionDistinct();

	        // test math context for big decimal and average divide
	        var config = SupportConfigFactory.GetConfiguration();
	        config.EngineDefaults.ExpressionConfig.MathContext = new MathContext(MidpointRounding.ToEven, 2);
	        config.AddEventType(typeof(SupportBeanNumeric));
	        var engineMathCtx = EPServiceProviderManager.GetProvider(typeof(TestAggregateFiltered).Name, config);

            engineMathCtx.EPAdministrator.CreateEPL("select avg(DecimalOne) as c0 from SupportBeanNumeric").Events += _listener.Update;
            engineMathCtx.EPRuntime.SendEvent(new SupportBeanNumeric(null, MakeDecimal(0, 2, MidpointRounding.AwayFromZero)));
            engineMathCtx.EPRuntime.SendEvent(new SupportBeanNumeric(null, MakeDecimal(0, 2, MidpointRounding.AwayFromZero)));
            engineMathCtx.EPRuntime.SendEvent(new SupportBeanNumeric(null, MakeDecimal(1, 2, MidpointRounding.AwayFromZero)));
            Assert.AreEqual(0.33d, _listener.GetAndResetLastNewData()[0].Get("c0").AsDouble());
  
            engineMathCtx.Dispose();
	    }

	    private void RunAssertionDistinct()
        {

	        var fields = "cavedev,cavg,cmax,cmedian,cmin,cstddev,csum".Split(',');
	        _epService.EPRuntime.SendEvent(MakeBean(100, true));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{0d, 100d, 100, 100d, 100, null, 100});

	        _epService.EPRuntime.SendEvent(MakeBean(100, true));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{0d, 100d, 100, 100d, 100, null, 100});

	        _epService.EPRuntime.SendEvent(MakeBean(200, true));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{50d, 150d, 200, 150d, 100, 70.71067811865476, 300});

	        _epService.EPRuntime.SendEvent(MakeBean(200, true));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{50d, 150d, 200, 150d, 100, 70.71067811865476, 300});

	        _epService.EPRuntime.SendEvent(MakeBean(200, true));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{0d, 200d, 200, 200d, 200, null, 200});
	    }

        [Test]
	    public void TestFirstLastEver()
        {
	        RunAssertionFirstLastEver(true);
	        RunAssertionFirstLastEver(false);
	    }

	    private void RunAssertionFirstLastEver(bool soda)
        {
	        var fields = "c1,c2,c3".Split(',');
	        var epl = "select " +
	                "firstever(IntBoxed,BoolPrimitive) as c1, " +
	                "lastever(IntBoxed,BoolPrimitive) as c2, " +
	                "countever(*,BoolPrimitive) as c3 " +
	                "from SupportBean.win:length(3)";
	        var stmt = SupportModelHelper.CreateByCompileOrParse(_epService, soda, epl);
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(MakeBean(100, false));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{null, null, 0L});

	        _epService.EPRuntime.SendEvent(MakeBean(100, true));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{100, 100, 1L});

	        _epService.EPRuntime.SendEvent(MakeBean(200, true));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{100, 200, 2L});

	        _epService.EPRuntime.SendEvent(MakeBean(201, false));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{100, 200, 2L});

	        stmt.Dispose();
	    }

        [Test]
	    public void TestInvalid()
        {
	        TryInvalid("select count(*, IntPrimitive) from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'count(*,IntPrimitive)': Invalid filter expression parameter to the aggregation function 'count' is expected to return a boolean value but returns " + Name.Of<int>() + (" [select count(*, IntPrimitive) from SupportBean]"));

	        TryInvalid("select fmin(IntPrimitive) from SupportBean",
	                "Error starting statement: Failed to validate select-clause expression 'min(IntPrimitive)': MIN-filtered aggregation function must have a filter expression as a second parameter [select fmin(IntPrimitive) from SupportBean]");
	    }

	    private void TryInvalid(string epl, string message)
        {
	        try {
	            _epService.EPAdministrator.CreateEPL(epl);
	            Assert.Fail();
	        }
	        catch (EPStatementException ex) {
	            Assert.AreEqual(message, ex.Message);
	        }
	    }

	    private SupportBean MakeBean(float floatPrimitive, double doublePrimitive, long longPrimitive, short shortPrimitive, bool boolPrimitive)
        {
	        var sb = new SupportBean();
	        sb.FloatPrimitive = floatPrimitive;
	        sb.DoublePrimitive = doublePrimitive;
	        sb.LongPrimitive = longPrimitive;
	        sb.ShortPrimitive = shortPrimitive;
	        sb.BoolPrimitive = boolPrimitive;
	        return sb;
	    }

	    private SupportBean MakeBean(int? intBoxed, bool boolPrimitive)
        {
	        var sb = new SupportBean();
	        sb.IntBoxed = intBoxed;
	        sb.BoolPrimitive = boolPrimitive;
	        return sb;
	    }

        private decimal MakeDecimal(int value, int scale, MidpointRounding rounding)
        {
            return Math.Round((decimal)value, scale, rounding);
        }

	    public class BlackWhiteEvent
        {
	        public BlackWhiteEvent(bool black)
            {
	            IsBlack = black;
	        }

	        public bool IsBlack { get; private set; }
        }
	}
} // end of namespace
