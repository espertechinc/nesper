///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Xml;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.container;

using NUnit.Framework;

namespace NEsper.Examples.AutoId
{
	public class TestRFIDTagsPerSensorStmt
	{
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;
	
	    [SetUp]
	    public void SetUp()
	    {
	        var container = ContainerExtensions.CreateDefaultContainer()
	            .InitializeDefaultServices()
	            .InitializeDatabaseDrivers();

            var url = container.ResourceManager().ResolveResourceURL("esper.examples.cfg.xml");
	        var config = new Configuration(container);
	        config.Configure(url);
	
	        _epService = EPServiceProviderManager.GetProvider(container, "AutoIdSim", config);
	        _epService.Initialize();
	
	        _listener = new SupportUpdateListener();
	        var rfidStmt = RFIDTagsPerSensorStmt.Create(_epService.EPAdministrator);
	        rfidStmt.Events += _listener.Update;
	    }
	
	    [Test]
	    public void TestEvents()
	    {
	    	var sensorlDoc = new XmlDocument() ;
	
	        using(var stream = _epService.Container.ResourceManager().GetResourceAsStream("data/AutoIdSensor1.xml")) {
	        	sensorlDoc.Load( stream ) ;
	        }
	    	
	        _epService.EPRuntime.SendEvent(sensorlDoc);
	        AssertReceived("urn:epc:1:4.16.36", 5);
	    }
	
	    private void AssertReceived(string sensorId, double numTags)
	    {
	        Assert.IsTrue(_listener.IsInvoked);
	        Assert.AreEqual(1, _listener.LastNewData.Length);
	        var eventBean = _listener.LastNewData[0];
	        Assert.AreEqual(sensorId, eventBean["SensorId"]);
	        Assert.AreEqual(numTags, eventBean["NumTagsPerSensor"]);
	        _listener.Reset();
	    }
	}
}
