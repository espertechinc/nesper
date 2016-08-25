///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Numerics;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.epl;

using NUnit.Framework;

namespace com.espertech.esper.regression.expr
{
    [TestFixture]
	public class TestBigNumberSupport 
	{
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;

        [SetUp]
	    public void SetUp()
	    {
	        _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);}
	        _listener = new SupportUpdateListener();
	        _epService.EPAdministrator.Configuration.AddEventType("SupportBeanNumeric", typeof(SupportBeanNumeric));
	        _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _listener = null;
	    }

        [Test]
	    public void TestEquals()
	    {
	        // test equals BigDecimal
	        var stmt = _epService.EPAdministrator.CreateEPL("select * from SupportBeanNumeric where DecimalOne = 1 or DecimalOne = intOne or DecimalOne = doubleOne");
	        stmt.AddListener(_listener);

	        SendBigNumEvent(-1, 1);
	        Assert.IsTrue(_listener.GetAndClearIsInvoked());
	        SendBigNumEvent(-1, 2);
	        Assert.IsFalse(_listener.GetAndClearIsInvoked());

	        _epService.EPRuntime.SendEvent(new SupportBeanNumeric(2, 0, null, 2m, 0, 0));
	        Assert.IsTrue(_listener.GetAndClearIsInvoked());
	        _epService.EPRuntime.SendEvent(new SupportBeanNumeric(3, 0, null, 2m, 0, 0));
	        Assert.IsFalse(_listener.GetAndClearIsInvoked());

	        _epService.EPRuntime.SendEvent(new SupportBeanNumeric(0, 0, null, 3m, 3d, 0));
	        Assert.IsTrue(_listener.GetAndClearIsInvoked());
	        _epService.EPRuntime.SendEvent(new SupportBeanNumeric(0, 0, null, 3.9999m, 4d, 0));
	        Assert.IsFalse(_listener.GetAndClearIsInvoked());

	        // test equals BigInteger
	        stmt = _epService.EPAdministrator.CreateEPL("select * from SupportBeanNumeric where DecimalOne = BigInt or BigInt = intOne or BigInt = 1");
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBeanNumeric(0, 0, new BigInteger(2), 2m, 0, 0));
	        Assert.IsTrue(_listener.GetAndClearIsInvoked());
	        _epService.EPRuntime.SendEvent(new SupportBeanNumeric(0, 0, new BigInteger(3), 2m, 0, 0));
	        Assert.IsFalse(_listener.GetAndClearIsInvoked());

	        _epService.EPRuntime.SendEvent(new SupportBeanNumeric(2, 0, new BigInteger(2), null, 0, 0));
	        Assert.IsTrue(_listener.GetAndClearIsInvoked());
	        _epService.EPRuntime.SendEvent(new SupportBeanNumeric(3, 0, new BigInteger(2), null, 0, 0));
	        Assert.IsFalse(_listener.GetAndClearIsInvoked());

	        _epService.EPRuntime.SendEvent(new SupportBeanNumeric(0, 0, new BigInteger(1), null, 0, 0));
	        Assert.IsTrue(_listener.GetAndClearIsInvoked());
	        _epService.EPRuntime.SendEvent(new SupportBeanNumeric(0, 0, new BigInteger(4), null, 0, 0));
	        Assert.IsFalse(_listener.GetAndClearIsInvoked());
	    }

        [Test]
	    public void TestRelOp()
	    {
	        // relational op tests handled by relational op unit test
	        var stmt = _epService.EPAdministrator.CreateEPL("select * from SupportBeanNumeric where DecimalOne < 10 and BigInt > 10");
	        stmt.AddListener(_listener);

	        SendBigNumEvent(10, 10);
	        Assert.IsFalse(_listener.GetAndClearIsInvoked());

	        SendBigNumEvent(11, 9);
	        Assert.IsTrue(_listener.GetAndClearIsInvoked());
	        stmt.Dispose();

	        stmt = _epService.EPAdministrator.CreateEPL("select * from SupportBeanNumeric where DecimalOne < 10.0");
	        stmt.AddListener(_listener);

	        SendBigNumEvent(0, 11);
	        Assert.IsFalse(_listener.GetAndClearIsInvoked());

	        _epService.EPRuntime.SendEvent(new SupportBeanNumeric(null, 9.999m));
	        Assert.IsTrue(_listener.GetAndClearIsInvoked());
	        stmt.Dispose();

	        // test float
	        stmt = _epService.EPAdministrator.CreateEPL("select * from SupportBeanNumeric where floatOne < 10f and floatTwo > 10f");
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBeanNumeric(true, 1f, 20f));
	        Assert.IsTrue(_listener.GetAndClearIsInvoked());
	        _epService.EPRuntime.SendEvent(new SupportBeanNumeric(true, 20f, 1f));
	        Assert.IsFalse(_listener.GetAndClearIsInvoked());
	    }

        [Test]
	    public void TestBetween()
	    {
	        var stmt = _epService.EPAdministrator.CreateEPL("select * from SupportBeanNumeric where DecimalOne between 10 and 20 or BigInt between 100 and 200");
	        stmt.AddListener(_listener);

	        SendBigNumEvent(0, 9);
	        Assert.IsFalse(_listener.GetAndClearIsInvoked());

	        SendBigNumEvent(0, 10);
	        Assert.IsTrue(_listener.GetAndClearIsInvoked());

	        SendBigNumEvent(99, 0);
	        Assert.IsFalse(_listener.GetAndClearIsInvoked());

	        SendBigNumEvent(100, 0);
	        Assert.IsTrue(_listener.GetAndClearIsInvoked());
	        stmt.Dispose();
	    }

        [Test]
	    public void TestIn()
	    {
	        var stmt = _epService.EPAdministrator.CreateEPL("select * from SupportBeanNumeric where DecimalOne in (10, 20d) or BigInt in (0x02, 3)");
	        stmt.AddListener(_listener);

	        SendBigNumEvent(0, 9);
	        Assert.IsFalse(_listener.GetAndClearIsInvoked());

	        SendBigNumEvent(0, 10);
	        Assert.IsTrue(_listener.GetAndClearIsInvoked());

	        _epService.EPRuntime.SendEvent(new SupportBeanNumeric(null, 20m));
	        Assert.IsTrue(_listener.GetAndClearIsInvoked());

	        SendBigNumEvent(99, 0);
	        Assert.IsFalse(_listener.GetAndClearIsInvoked());

	        SendBigNumEvent(2, 0);
	        Assert.IsTrue(_listener.GetAndClearIsInvoked());

	        SendBigNumEvent(3, 0);
	        Assert.IsTrue(_listener.GetAndClearIsInvoked());
	        stmt.Dispose();
	    }

        [Test]
	    public void TestMath()
	    {
	        var stmt = _epService.EPAdministrator.CreateEPL("select * from SupportBeanNumeric " +
	                        "where DecimalOne+BigInt=100 or DecimalOne+1=2 or DecimalOne+2d=5.0 or BigInt+5L=8 or BigInt+5d=9.0");
	        stmt.AddListener(_listener);

	        SendBigNumEvent(50, 49);
	        Assert.IsFalse(_listener.GetAndClearIsInvoked());

	        SendBigNumEvent(50, 50);
	        Assert.IsTrue(_listener.GetAndClearIsInvoked());

	        SendBigNumEvent(0, 1);
	        Assert.IsTrue(_listener.GetAndClearIsInvoked());

	        SendBigNumEvent(0, 2);
	        Assert.IsFalse(_listener.GetAndClearIsInvoked());

	        SendBigNumEvent(0, 3);
	        Assert.IsTrue(_listener.GetAndClearIsInvoked());

	        SendBigNumEvent(0, 0);
	        Assert.IsFalse(_listener.GetAndClearIsInvoked());

	        SendBigNumEvent(3, 0);
	        Assert.IsTrue(_listener.GetAndClearIsInvoked());

	        SendBigNumEvent(4, 0);
	        Assert.IsTrue(_listener.GetAndClearIsInvoked());
	        stmt.Dispose();

	        stmt = _epService.EPAdministrator.CreateEPL(
	                "select DecimalOne+BigInt as v1, DecimalOne+2 as v2, DecimalOne+3d as v3, BigInt+5L as v4, BigInt+5d as v5 " +
	                " from SupportBeanNumeric");
	        stmt.AddListener(_listener);
	        _listener.Reset();

	        Assert.AreEqual(typeof(decimal?), stmt.EventType.GetPropertyType("v1"));
            Assert.AreEqual(typeof(decimal?), stmt.EventType.GetPropertyType("v2"));
            Assert.AreEqual(typeof(decimal?), stmt.EventType.GetPropertyType("v3"));
            Assert.AreEqual(typeof(BigInteger?), stmt.EventType.GetPropertyType("v4"));
            Assert.AreEqual(typeof(decimal?), stmt.EventType.GetPropertyType("v5"));

	        SendBigNumEvent(1, 2);
	        var theEvent = _listener.AssertOneGetNewAndReset();
	        EPAssertionUtil.AssertProps(theEvent, "v1,v2,v3,v4,v5".Split(','),
	                new object[]{3m, 4m, 5m, new BigInteger(6), 6m});

	        // test aggregation-sum, multiplication and division all together; test for ESPER-340
	        stmt.Dispose();
	        stmt = _epService.EPAdministrator.CreateEPL(
	                "select (sum(DecimalTwo * DecimalOne)/sum(DecimalOne)) as avgRate from SupportBeanNumeric");
	        stmt.AddListener(_listener);
	        _listener.Reset();
            Assert.AreEqual(typeof(decimal?), stmt.EventType.GetPropertyType("avgRate"));
	        SendBigNumEvent(0, 5);
	        var avgRate = _listener.AssertOneGetNewAndReset().Get("avgRate");
	        Assert.IsTrue(avgRate is decimal);
	        Assert.AreEqual(5m, avgRate);
	    }

        [Test]
	    public void TestAggregation()
	    {
	        var fields = "sum(BigInt),sum(DecimalOne)," +
	                "avg(BigInt),avg(DecimalOne)," +
	                "median(BigInt),median(DecimalOne)," +
	                "stddev(BigInt),stddev(DecimalOne)," +
	                "avedev(BigInt),avedev(DecimalOne)," +
	                "min(BigInt),min(DecimalOne)";
	        var stmt = _epService.EPAdministrator.CreateEPL(
	                "select " + fields + " from SupportBeanNumeric");
	        stmt.AddListener(_listener);
	        _listener.Reset();

	        var fieldList = fields.Split(',');
	        SendBigNumEvent(1, 2);
	        var theEvent = _listener.AssertOneGetNewAndReset();
	        EPAssertionUtil.AssertProps(theEvent, fieldList, new object[]
            {
                new BigInteger(1), 2m,        // sum
	            new BigInteger(1), 2m,               // avg
	            1d, 2d,               // median
	            null, null,
	            0.0, 0.0,
	            new BigInteger(1), 2m,
	        });
	    }

        [Test]
	    public void TestMinMax()
	    {
	        var stmt = _epService.EPAdministrator.CreateEPL(
	                "select min(BigInt, 10) as v1, min(10, BigInt) as v2, " +
	                "max(DecimalOne, 10) as v3, max(10, 100d, BigInt, DecimalOne) as v4 from SupportBeanNumeric");
	        stmt.AddListener(_listener);
	        _listener.Reset();

	        var fieldList = "v1,v2,v3,v4".Split(',');

	        SendBigNumEvent(1, 2);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldList,
	                new object[]{new BigInteger(1), new BigInteger(1), 10m, 100m});

	        SendBigNumEvent(40, 300);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldList,
	                new object[]{new BigInteger(10), new BigInteger(10), 300m, 300m});

	        SendBigNumEvent(250, 200);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldList,
	                new object[]{new BigInteger(10), new BigInteger(10), 200m, 250m});
	    }

        [Test]
	    public void TestFilterEquals()
	    {
	        var fieldList = "DecimalOne".Split(',');

	        var stmt = _epService.EPAdministrator.CreateEPL("select DecimalOne from SupportBeanNumeric(DecimalOne = 4)");
	        stmt.AddListener(_listener);

	        SendBigNumEvent(0, 2);
	        Assert.IsFalse(_listener.IsInvoked);

	        SendBigNumEvent(0, 4);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldList, new object[]{4m});

	        stmt.Dispose();
	        stmt = _epService.EPAdministrator.CreateEPL("select DecimalOne from SupportBeanNumeric(DecimalOne = 4d)");
	        stmt.AddListener(_listener);

	        SendBigNumEvent(0, 4);
	        Assert.IsTrue(_listener.IsInvoked);
	        _listener.Reset();

	        _epService.EPRuntime.SendEvent(new SupportBeanNumeric(new BigInteger(0), 4m));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldList, new object[]{4m});

	        stmt.Dispose();
	        stmt = _epService.EPAdministrator.CreateEPL("select DecimalOne from SupportBeanNumeric(BigInt = 4)");
	        stmt.AddListener(_listener);

	        SendBigNumEvent(3, 4);
	        Assert.IsFalse(_listener.IsInvoked);

	        SendBigNumEvent(4, 3);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldList, new object[]{3m});
	    }

        [Test]
	    public void TestJoin()
	    {
            var fieldList = "BigInt,DecimalOne".Split(',');
	        var stmt = _epService.EPAdministrator.CreateEPL("select BigInt,DecimalOne from SupportBeanNumeric.win:keepall(), SupportBean.win:keepall() " +
	                "where IntPrimitive = BigInt and DoublePrimitive = DecimalOne");
	        stmt.AddListener(_listener);

	        SendSupportBean(2, 3);
	        SendBigNumEvent(0, 2);
	        SendBigNumEvent(2, 0);
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBeanNumeric(new BigInteger(2), 3m));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldList, new object[]{new BigInteger(2), 3m});
	    }

        [Test]
	    public void TestCastAndUDF()
	    {
	        _epService.EPAdministrator.Configuration.AddImport(typeof(SupportStaticMethodLib).FullName);
	        var stmt = _epService.EPAdministrator.CreateEPL(
                    "select SupportStaticMethodLib.MyBigIntFunc(cast(2, BigInteger)) as v1, SupportStaticMethodLib.MyDecimalFunc(cast(3d, decimal)) as v2 from SupportBeanNumeric");
	        stmt.AddListener(_listener);

	        var fieldList = "v1,v2".Split(',');
	        SendBigNumEvent(0, 2);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldList, new object[]{new BigInteger(2), 3.0m});
	    }

	    private void SendBigNumEvent(int bigInt, double bigDec)
	    {
	        var bean = new SupportBeanNumeric(new BigInteger(bigInt), (decimal) bigDec);
	        bean.DecimalTwo = (decimal) bigDec;
	        _epService.EPRuntime.SendEvent(bean);
	    }

	    private void SendSupportBean(int intPrimitive, double doublePrimitive)
	    {
	        var bean = new SupportBean();
	        bean.IntPrimitive = intPrimitive;
	        bean.DoublePrimitive = doublePrimitive;
	        _epService.EPRuntime.SendEvent(bean);
	    }
	}
} // end of namespace
