///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;

using NUnit.Framework;

namespace com.espertech.esper.regression.events
{
    [TestFixture]
    public class TestEventTypeStaticConfig 
    {
        [Test]
        public void TestStaticConfig() 
        {
            var xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                    "<esper-configuration>\t\n" +
                    "\t<event-type name=\"MyMapEvent\">\n" +
                    "\t\t<map>\n" +
                    "\t  \t\t<map-property name=\"myStringArray\" class=\"string[]\"/>\n" +
                    "\t  \t</map>\n" +
                    "\t</event-type>\n" +
                    "\t\n" +
                    "\t<event-type name=\"MyObjectArrayEvent\">\n" +
                    "\t\t<objectarray>\n" +
                    "\t  \t\t<objectarray-property name=\"myStringArray\" class=\"string[]\"/>\n" +
                    "\t  \t</objectarray>\n" +
                    "\t</event-type>\n" +
                    "</esper-configuration>\n";
    
            var config = new Configuration();
            config.Configure(SupportXML.GetDocument(xml));
    
            // add a map-type and then clear the map to test copy of type definition for preventing accidental overwrite
            var typeMyEventIsCopyDef = new Dictionary<string, object>();
            typeMyEventIsCopyDef.Put("prop1", typeof(string));
            config.AddEventType("MyEventIsCopyDef", typeMyEventIsCopyDef);
            typeMyEventIsCopyDef.Clear();
    
            // obtain engine
            var epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
    
            // ensure cleared type is available (type information was copied to prevent accidental overwrite)
            epService.EPAdministrator.CreateEPL("select prop1 from MyEventIsCopyDef");
    
            // assert array types
            foreach (var name in new string[] {"MyObjectArrayEvent", "MyMapEvent"}) {
                EPAssertionUtil.AssertEqualsAnyOrder(
                    new EventPropertyDescriptor[]
                    {
                        new EventPropertyDescriptor("myStringArray", typeof(string[]), typeof(string), false, false, true, false, false),
                    },
                    epService.EPAdministrator.Configuration.GetEventType(name).PropertyDescriptors);
            }
        }
    }
}
