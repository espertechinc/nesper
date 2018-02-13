///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Xml;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.supportregression.events;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util.support;

// using static org.junit.Assert.assertNull;

using NUnit.Framework;

namespace com.espertech.esper.regression.events.xml
{
    public class ExecEventXMLNoSchemaEventTransposeXPathGetter : RegressionExecution {
        private static readonly string CLASSLOADER_SCHEMA_URI = "regression/simpleSchema.xsd";
    
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.ViewResources.IterableUnbound = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            var eventTypeMeta = new ConfigurationEventTypeXMLDOM();
            eventTypeMeta.RootElementName = "simpleEvent";
            string schemaUri = typeof(ExecEventXMLNoSchemaEventTransposeXPathGetter).ClassLoader.GetResource(CLASSLOADER_SCHEMA_URI).ToString();
            eventTypeMeta.SchemaResource = schemaUri;
            eventTypeMeta.XPathPropertyExpr = true;       // <== note this
            eventTypeMeta.AddNamespacePrefix("ss", "samples:schemas:simpleSchema");
            epService.EPAdministrator.Configuration.AddEventType("TestXMLSchemaType", eventTypeMeta);
    
            // note class not a fragment
            EPStatement stmtInsert = epService.EPAdministrator.CreateEPL("insert into MyNestedStream select nested1 from TestXMLSchemaType");
            EPAssertionUtil.AssertEqualsAnyOrder(new Object[]{
                    new EventPropertyDescriptor("nested1", typeof(XmlNode), null, false, false, false, false, false),
            }, stmtInsert.EventType.PropertyDescriptors);
            SupportEventTypeAssertionUtil.AssertConsistency(stmtInsert.EventType);
    
            EventType type = ((EPServiceProviderSPI) epService).EventAdapterService.GetEventTypeByName("TestXMLSchemaType");
            SupportEventTypeAssertionUtil.AssertConsistency(type);
            Assert.IsNull(type.GetFragmentType("nested1"));
            Assert.IsNull(type.GetFragmentType("nested1.nested2"));
    
            SupportXML.SendDefaultEvent(epService.EPRuntime, "ABC");
            SupportEventTypeAssertionUtil.AssertConsistency(stmtInsert.First());
        }
    }
} // end of namespace
