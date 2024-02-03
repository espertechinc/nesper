///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Xml;

using com.espertech.esper.container;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;
using NUnit.Framework.Legacy;
using Configuration = com.espertech.esper.common.client.configuration.Configuration;

namespace NEsper.Examples.AutoId
{
	public class TestRFIDTagsPerSensorStmt
	{
		private IContainer _container;
	    private EPRuntime _runtime;
	    private SupportUpdateListener _listener;
	
	    [SetUp]
	    public void SetUp()
	    {
	        _container = ContainerExtensions.CreateDefaultContainer()
	            .InitializeDefaultServices()
	            .InitializeDatabaseDrivers();

            var url = _container.ResourceManager().ResolveResourceURL("esper.examples.cfg.xml");
	        var config = new Configuration(_container);
	        config.Configure(url);
	
	        _runtime = EPRuntimeProvider.GetRuntime("AutoIdSim", config);
	        _runtime.Initialize();
	
	        _listener = new SupportUpdateListener();
	        var rfidStmt = RFIDTagsPerSensorStmt.Create(_runtime);
	        rfidStmt.Events += _listener.Update;
	    }
	
	    [Test]
	    public void TestEvents()
	    {
	    	var sensorlDoc = new XmlDocument() ;
	
	        using(var stream = _container
		        .ResourceManager()
		        .GetResourceAsStream("data/AutoIdSensor1.xml")) {
	        	sensorlDoc.Load( stream ) ;
	        }
	    	
	        _runtime.EventService.SendEventXMLDOM(sensorlDoc, "AutoIdSim");
	        AssertReceived("urn:epc:1:4.16.36", 5);
	    }
	
	    private void AssertReceived(string sensorId, double numTags)
	    {
	        ClassicAssert.IsTrue(_listener.IsInvoked);
	        ClassicAssert.AreEqual(1, _listener.LastNewData.Length);
	        var eventBean = _listener.LastNewData[0];
	        ClassicAssert.AreEqual(sensorId, eventBean["SensorId"]);
	        ClassicAssert.AreEqual(numTags, eventBean["NumTagsPerSensor"]);
	        _listener.Reset();
	    }
	}
}
