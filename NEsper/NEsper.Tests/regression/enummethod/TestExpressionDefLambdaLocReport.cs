///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean.lrreport;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.enummethod
{
    [TestFixture]
	public class TestExpressionDefLambdaLocReport  {

	    private EPServiceProvider epService;
	    private SupportUpdateListener listener;

        [SetUp]
	    public void SetUp() {

	        Configuration config = SupportConfigFactory.GetConfiguration();
	        config.AddEventType("LocationReport", typeof(LocationReport));
	        config.AddImport(typeof(LRUtil));

	        epService = EPServiceProviderManager.GetDefaultProvider(config);
	        epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName);}
	        listener = new SupportUpdateListener();
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        listener = null;
	    }

        [Test]
	    public void TestMissingLuggage()
        {
	        // Regular algorithm to find separated luggage and new owner.
	        LocationReport theEvent = LocationReportFactory.MakeLarge();
	        IList<Item> separatedLuggage = LocationReportFactory.FindSeparatedLuggage(theEvent);

	        foreach (Item item in separatedLuggage) {
	            //log.info("Luggage that are separated (dist>20): " + item);
	            Item newOwner = LocationReportFactory.FindPotentialNewOwner(theEvent, item);
	            //log.info("Found new owner " + newOwner);
	        }

	        string eplFragment = "" +
	                "expression lostLuggage {" +
	                "  lr => lr.items.where(l => l.type='L' and " +
	                "    lr.items.anyof(p => p.type='P' and p.assetId=l.assetIdPassenger and LRUtil.Distance(l.location.x, l.location.y, p.location.x, p.location.y) > 20))" +
	                "}" +
	                "expression passengers {" +
	                "  lr => lr.items.where(l => l.type='P')" +
	                "}" +
	                "" +
	                "expression nearestOwner {" +
	                "  lr => lostLuggage(lr).toMap(key => key.assetId, " +
	                "     value => passengers(lr).minBy(p => LRUtil.Distance(value.location.x, value.location.y, p.location.x, p.location.y)))" +
	                "}" +
	                "" +
	                "select lostLuggage(lr) as val1, nearestOwner(lr) as val2 from LocationReport lr";
	        EPStatement stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
	        stmtFragment.AddListener(listener);

	        LocationReport bean = LocationReportFactory.MakeLarge();
	        epService.EPRuntime.SendEvent(bean);

	        Item[] val1 = ItemArray(listener.AssertOneGetNew().Get("val1").Unwrap<Item>());
	        Assert.AreEqual(3, val1.Length);
	        Assert.AreEqual("L00000", val1[0].AssetId);
	        Assert.AreEqual("L00007", val1[1].AssetId);
	        Assert.AreEqual("L00008", val1[2].AssetId);

	        var val2 = (IDictionary<object, object>) listener.AssertOneGetNewAndReset().Get("val2");
	        Assert.AreEqual(3, val2.Count);
	        Assert.AreEqual("P00008", ((Item) val2.Get("L00000")).AssetId);
	        Assert.AreEqual("P00001", ((Item) val2.Get("L00007")).AssetId);
	        Assert.AreEqual("P00001", ((Item) val2.Get("L00008")).AssetId);
	    }

	    private Item[] ItemArray(ICollection<Item> it) {
	        return it.ToArray();
	    }
	}
} // end of namespace
