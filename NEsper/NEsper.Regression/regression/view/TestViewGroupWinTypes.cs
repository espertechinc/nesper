///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
	public class TestViewGroupWinTypes  {
	    private EPServiceProvider _epService;

        [SetUp]
	    public void SetUp() {
	        _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);
	        }
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.EndTest();
	        }
	    }

        [Test]
	    public void TestType() {
	        string viewStmt = "select * from " + Name.Of<SupportBean>() +
	                          "#groupwin(intPrimitive)#length(4)#groupwin(longBoxed)#uni(doubleBoxed)";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(viewStmt);

	        Assert.AreEqual(typeof(int), stmt.EventType.GetPropertyType("intPrimitive"));
	        Assert.AreEqual(typeof(long?), stmt.EventType.GetPropertyType("longBoxed"));
	        Assert.AreEqual(typeof(double?), stmt.EventType.GetPropertyType("stddev"));
	        Assert.AreEqual(8, stmt.EventType.PropertyNames.Length);
	    }
	}
} // end of namespace
