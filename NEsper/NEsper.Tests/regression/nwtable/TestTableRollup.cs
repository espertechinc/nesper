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

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
	public class TestTableRollup
    {
	    private EPServiceProvider _epService;

        [SetUp]
	    public void SetUp()
        {
	        _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
	        _epService.Initialize();
	        foreach (var clazz in new Type[] {typeof(SupportBean), typeof(SupportBean_S0), typeof(SupportBean_S1)}) {
	            _epService.EPAdministrator.Configuration.AddEventType(clazz);
	        }
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);}
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	    }

        [Test]
	    public void TestRollupOneDim() {
	        var listenerQuery = new SupportUpdateListener();
	        var listenerOut = new SupportUpdateListener();
	        var fieldsOut = "theString,total".Split(',');

	        _epService.EPAdministrator.CreateEPL("create table MyTable(pk string primary key, total sum(int))");
	        _epService.EPAdministrator.CreateEPL("into table MyTable insert into MyStream select theString, sum(intPrimitive) as total from SupportBean.win:length(4) group by rollup(theString)").AddListener(listenerOut);
	        _epService.EPAdministrator.CreateEPL("select MyTable[p00].total as c0 from SupportBean_S0").AddListener(listenerQuery);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            AssertValuesListener(listenerQuery, new object[][] { new object[] { null, 10 }, new object[] { "E1", 10 }, new object[] { "E2", null } });
            EPAssertionUtil.AssertPropsPerRow(listenerOut.GetAndResetLastNewData(), fieldsOut, new object[][] { new object[] { "E1", 10 }, new object[] { null, 10 } });

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 200));
            AssertValuesListener(listenerQuery, new object[][] { new object[] { null, 210 }, new object[] { "E1", 10 }, new object[] { "E2", 200 } });
            EPAssertionUtil.AssertPropsPerRow(listenerOut.GetAndResetLastNewData(), fieldsOut, new object[][] { new object[] { "E2", 200 }, new object[] { null, 210 } });

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 11));
            AssertValuesListener(listenerQuery, new object[][] { new object[] { null, 221 }, new object[] { "E1", 21 }, new object[] { "E2", 200 } });
            EPAssertionUtil.AssertPropsPerRow(listenerOut.GetAndResetLastNewData(), fieldsOut, new object[][] { new object[] { "E1", 21 }, new object[] { null, 221 } });

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 201));
            AssertValuesListener(listenerQuery, new object[][] { new object[] { null, 422 }, new object[] { "E1", 21 }, new object[] { "E2", 401 } });
            EPAssertionUtil.AssertPropsPerRow(listenerOut.GetAndResetLastNewData(), fieldsOut, new object[][] { new object[] { "E2", 401 }, new object[] { null, 422 } });

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 12)); // {"E1", 10} leaving window
            AssertValuesListener(listenerQuery, new object[][] { new object[] { null, 424 }, new object[] { "E1", 23 }, new object[] { "E2", 401 } });
            EPAssertionUtil.AssertPropsPerRow(listenerOut.GetAndResetLastNewData(), fieldsOut, new object[][] { new object[] { "E1", 23 }, new object[] { null, 424 } });
	    }

        [Test]
	    public void TestRollupTwoDim()
        {
            var fields = "k0,k1,total".Split(',');
	        _epService.EPAdministrator.CreateEPL("create objectarray schema MyEvent(k0 int, k1 int, col int)");
	        _epService.EPAdministrator.CreateEPL("create table MyTable(k0 int primary key, k1 int primary key, total sum(int))");
	        _epService.EPAdministrator.CreateEPL("into table MyTable insert into MyStream select sum(col) as total from MyEvent.win:length(3) group by rollup(k0,k1)");

	        _epService.EPRuntime.SendEvent(new object[] {1, 10, 100}, "MyEvent");
	        _epService.EPRuntime.SendEvent(new object[] {2, 10, 200}, "MyEvent");
	        _epService.EPRuntime.SendEvent(new object[] {1, 20, 300}, "MyEvent");

            AssertValuesIterate(fields, new object[][]{new object[]{null, null, 600}, new object[]{1, null, 400}, new object[]{2, null, 200},
	                new object[]{1, 10, 100}, new object[]{2, 10, 200}, new object[]{1, 20, 300}});

	        _epService.EPRuntime.SendEvent(new object[] {1, 10, 400}, "MyEvent"); // expires {1, 10, 100}

            AssertValuesIterate(fields, new object[][]{new object[]{null, null, 900}, new object[]{1, null, 700}, new object[]{2, null, 200},
	                new object[]{1, 10, 400}, new object[]{2, 10, 200}, new object[]{1, 20, 300}});
	    }

        [Test]
	    public void TestGroupingSetThreeDim() {
	        _epService.EPAdministrator.CreateEPL("create objectarray schema MyEvent(k0 string, k1 string, k2 string, col int)");
	        _epService.EPAdministrator.CreateEPL("create table MyTable(k0 string primary key, k1 string primary key, k2 string primary key, total sum(int))");
	        _epService.EPAdministrator.CreateEPL("into table MyTable insert into MyStream select sum(col) as total from MyEvent.win:length(3) group by grouping sets(k0,k1,k2)");

	        var fields = "k0,k1,k2,total".Split(',');
	        _epService.EPRuntime.SendEvent(new object[] {1, 10, 100, 1000}, "MyEvent");
	        _epService.EPRuntime.SendEvent(new object[] {2, 10, 200, 2000}, "MyEvent");
	        _epService.EPRuntime.SendEvent(new object[] {1, 20, 300, 3000}, "MyEvent");

	        AssertValuesIterate(fields, new object[][]{
	                new object[]{1, null, null, 4000}, new object[]{2, null, null, 2000},
	                new object[]{null, 10, null, 3000}, new object[]{null, 20, null, 3000},
	                new object[]{null, null, 100, 1000}, new object[]{null, null, 200, 2000}, new object[]{null, null, 300, 3000}});

	        _epService.EPRuntime.SendEvent(new object[] {1, 10, 400, 4000}, "MyEvent"); // expires {1, 10, 100, 1000}

	        AssertValuesIterate(fields, new object[][]{
	                new object[]{1, null, null, 7000}, new object[]{2, null, null, 2000},
	                new object[]{null, 10, null, 6000}, new object[]{null, 20, null, 3000},
	                new object[]{null, null, 100, null}, new object[]{null, null, 400, 4000}, new object[]{null, null, 200, 2000}, new object[]{null, null, 300, 3000}});
	    }

	    private void AssertValuesIterate(string[] fields, object[][] objects) {
	        var result = _epService.EPRuntime.ExecuteQuery("select * from MyTable");
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(result.Array, fields, objects);
	    }

	    private void AssertValuesListener(SupportUpdateListener listenerQuery, object[][] objects) {
	        for (var i = 0; i < objects.Length; i++) {
	            var p00 = (string) objects[i][0];
	            var expected = (int?) objects[i][1];
	            _epService.EPRuntime.SendEvent(new SupportBean_S0(0, p00));
                Assert.AreEqual(expected, listenerQuery.AssertOneGetNewAndReset().Get("c0"), "Failed at " + i + " for key " + p00);
	        }
	    }
	}
} // end of namespace
