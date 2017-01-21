///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.IO;
using System.Net;
using System.Xml;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.support.util;
using com.espertech.esper.events;

using NUnit.Framework;

namespace com.espertech.esper.example.autoid
{
	public class TestRFIDTagsPerSensorStmt
	{
	    private EPServiceProvider epService;
	    private SupportUpdateListener listener;
	
	    [SetUp]
	    public void SetUp()
	    {
	    	Uri url = ResourceManager.ResolveResourceURL("esper.examples.cfg.xml");
	        Configuration config = new Configuration();
	        config.Configure(url);
	
	        epService = EPServiceProviderManager.GetProvider("AutoIdSim", config);
	        epService.Initialize();
	
	        listener = new SupportUpdateListener();
	        var rfidStmt = RFIDTagsPerSensorStmt.Create(epService.EPAdministrator);
	        rfidStmt.Events += listener.Update;
	    }
	
	    [Test]
	    public void TestEvents()
	    {
	    	XmlDocument sensorlDoc = new XmlDocument() ;
	
	        using( Stream stream = ResourceManager.GetResourceAsStream("data/AutoIdSensor1.xml") ) {
	        	sensorlDoc.Load( stream ) ;
	        }
	    	
	        epService.EPRuntime.SendEvent(sensorlDoc);
	        AssertReceived("urn:epc:1:4.16.36", 5);
	    }
	
	    private void AssertReceived(string sensorId, double numTags)
	    {
	        Assert.IsTrue(listener.IsInvoked);
	        Assert.AreEqual(1, listener.LastNewData.Length);
	        EventBean eventBean = listener.LastNewData[0];
	        Assert.AreEqual(sensorId, eventBean["SensorId"]);
	        Assert.AreEqual(numTags, eventBean["NumTagsPerSensor"]);
	        listener.Reset();
	    }
	}
}
