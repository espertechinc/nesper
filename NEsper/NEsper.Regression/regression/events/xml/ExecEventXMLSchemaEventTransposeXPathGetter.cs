///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Linq;
using System.Xml;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.container;
using com.espertech.esper.core.service;
using com.espertech.esper.supportregression.events;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util.support;


using NUnit.Framework;

namespace com.espertech.esper.regression.events.xml
{
    public class ExecEventXMLSchemaEventTransposeXPathGetter : RegressionExecution {
        private const string CLASSLOADER_SCHEMA_URI = "regression/simpleSchema.xsd";
    
        public override void Run(EPServiceProvider epService) {
    
            // Note that XPath Node results when transposed must be queried by XPath that is also absolute.
            // For example: "nested1" => "/n0:simpleEvent/n0:nested1" results in a Node.
            // That result Node's "prop1" =>  "/n0:simpleEvent/n0:nested1/n0:prop1" and "/n0:nested1/n0:prop1" does NOT result in a value.
            // Therefore property transposal is disabled for Property-XPath expressions.
            var eventTypeMeta = new ConfigurationEventTypeXMLDOM();
            eventTypeMeta.RootElementName = "simpleEvent";
            string schemaUri = SupportContainer.Instance.ResourceManager().ResolveResourceURL(CLASSLOADER_SCHEMA_URI).ToString();
            eventTypeMeta.SchemaResource = schemaUri;
            eventTypeMeta.IsXPathPropertyExpr = true;       // <== note this
            eventTypeMeta.AddNamespacePrefix("ss", "samples:schemas:simpleSchema");
            epService.EPAdministrator.Configuration.AddEventType("TestXMLSchemaType", eventTypeMeta);
    
            // note class not a fragment
            EPStatement stmtInsert = epService.EPAdministrator.CreateEPL("insert into MyNestedStream select nested1 from TestXMLSchemaType#lastevent");
            EPAssertionUtil.AssertEqualsAnyOrder(new EventPropertyDescriptor[]{
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
