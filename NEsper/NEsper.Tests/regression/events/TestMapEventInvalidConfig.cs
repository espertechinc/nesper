///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.events
{
    [TestFixture]
	public class TestMapEventInvalidConfig 
	{
        [Test]
	    public void TestInvalidConfig()
	    {
	        var properties = new Properties();
	        properties.Put("astring", "XXXX");

	        var configuration = SupportConfigFactory.GetConfiguration();
	        configuration.AddEventType("MyInvalidEvent", properties);

	        try
	        {
	            var epServiceX = EPServiceProviderManager.GetDefaultProvider(configuration);
	            epServiceX.Initialize();
	            Assert.Fail();
	        }
	        catch (ConfigurationException)
	        {
	            // expected
	        }

	        var epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
	        epService.Initialize();
	    }
	}
} // end of namespace
