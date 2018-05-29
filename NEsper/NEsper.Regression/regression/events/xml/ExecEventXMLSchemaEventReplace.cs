///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Xml;
using System.Xml.XPath;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.container;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util.support;

namespace com.espertech.esper.regression.events.xml
{
    public class ExecEventXMLSchemaEventReplace : RegressionExecution
    {
        public static readonly string CLASSLOADER_SCHEMA_URI = "regression/simpleSchema.xsd";
        public static readonly string CLASSLOADER_SCHEMA_VERSION2_URI = "regression/simpleSchema_version2.xsd";
    
        private ConfigurationEventTypeXMLDOM _eventTypeMeta;
    
        public override void Configure(Configuration configuration) {
            _eventTypeMeta = new ConfigurationEventTypeXMLDOM();
            _eventTypeMeta.RootElementName = "simpleEvent";

            _eventTypeMeta.SchemaResource = SupportContainer.Instance.ResourceManager().ResolveResourceURL(CLASSLOADER_SCHEMA_URI).ToString();
            _eventTypeMeta.AddNamespacePrefix("ss", "samples:schemas:simpleSchema");
            _eventTypeMeta.AddXPathProperty("customProp", "count(/ss:simpleEvent/ss:nested3/ss:nested4)", XPathResultType.Number);
            configuration.AddEventType("TestXMLSchemaType", _eventTypeMeta);
        }
    
        public override void Run(EPServiceProvider epService) {
            if (SupportConfigFactory.SkipTest(typeof(ExecEventXMLSchemaEventReplace))) {
                return;
            }
    
            string stmtSelectWild = "select * from TestXMLSchemaType";
            EPStatement wildStmt = epService.EPAdministrator.CreateEPL(stmtSelectWild);
            EventType type = wildStmt.EventType;
            SupportEventTypeAssertionUtil.AssertConsistency(type);
    
            EPAssertionUtil.AssertEqualsAnyOrder(new EventPropertyDescriptor[]{
                    new EventPropertyDescriptor("nested1", typeof(XmlNode), null, false, false, false, false, true),
                    new EventPropertyDescriptor("prop4", typeof(string), typeof(char), false, false, true, false, false),
                    new EventPropertyDescriptor("nested3", typeof(XmlNode), null, false, false, false, false, true),
                    new EventPropertyDescriptor("customProp", typeof(double?), null, false, false, false, false, false),
            }, type.PropertyDescriptors);

            // update type and replace
            _eventTypeMeta.SchemaResource = SupportContainer.Instance.ResourceManager().ResolveResourceURL(CLASSLOADER_SCHEMA_VERSION2_URI).ToString();
            _eventTypeMeta.AddXPathProperty("countProp", "count(/ss:simpleEvent/ss:nested3/ss:nested4)", XPathResultType.Number);
            epService.EPAdministrator.Configuration.ReplaceXMLEventType("TestXMLSchemaType", _eventTypeMeta);
    
            wildStmt = epService.EPAdministrator.CreateEPL(stmtSelectWild);
            type = wildStmt.EventType;
            SupportEventTypeAssertionUtil.AssertConsistency(type);
    
            EPAssertionUtil.AssertEqualsAnyOrder(new EventPropertyDescriptor[]{
                    new EventPropertyDescriptor("nested1", typeof(XmlNode), null, false, false, false, false, true),
                    new EventPropertyDescriptor("prop4", typeof(string), typeof(char), false, false, true, false, false),
                    new EventPropertyDescriptor("prop5", typeof(string), typeof(char), false, false, true, false, false),
                    new EventPropertyDescriptor("nested3", typeof(XmlNode), null, false, false, false, false, true),
                    new EventPropertyDescriptor("customProp", typeof(double?), null, false, false, false, false, false),
                    new EventPropertyDescriptor("countProp", typeof(double?), null, false, false, false, false, false),
            }, type.PropertyDescriptors);
        }
    }
} // end of namespace
