///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.resultset
{
    [TestFixture]
	public class TestAggregateExtInvalid
    {

	    private EPServiceProvider _epService;

        [Test]
	    public void TestInvalid()
	    {
	        Configuration config = SupportConfigFactory.GetConfiguration();
	        config.EngineDefaults.Expression.IsExtendedAggregation = false;
	        _epService = EPServiceProviderManager.GetDefaultProvider(config);
	        _epService.Initialize();
	        _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();

	        TryInvalid("select rate(10) from SupportBean",
	                "Error starting statement: Failed to validate select-clause expression 'rate(10)': Unknown single-row function, aggregation function or mapped or indexed property named 'rate' could not be resolved [select rate(10) from SupportBean]");
	    }

	    private void TryInvalid(string epl, string message) {
	        try
	        {
	            _epService.EPAdministrator.CreateEPL(epl);
	            Assert.Fail();
	        }
	        catch (EPStatementException ex) {
	            Assert.AreEqual(message, ex.Message);
	        }
	    }
	}
} // end of namespace
