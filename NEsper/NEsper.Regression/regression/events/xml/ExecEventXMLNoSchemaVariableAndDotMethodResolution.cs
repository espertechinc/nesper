///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Xml.XPath;
using com.espertech.esper.client;
using com.espertech.esper.supportregression.execution;

namespace com.espertech.esper.regression.events.xml
{
    public class ExecEventXMLNoSchemaVariableAndDotMethodResolution : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.AddVariable("var", typeof(int), 0);
    
            var xmlDOMEventTypeDesc = new ConfigurationEventTypeXMLDOM();
            xmlDOMEventTypeDesc.RootElementName = "myevent";
            xmlDOMEventTypeDesc.AddXPathProperty("xpathAttrNum", "/myevent/@attrnum", XPathResultType.String, "long");
            xmlDOMEventTypeDesc.AddXPathProperty("xpathAttrNumTwo", "/myevent/@attrnumtwo", XPathResultType.String, "long");
            configuration.AddEventType("TestXMLNoSchemaType", xmlDOMEventTypeDesc);
        }
    
        public override void Run(EPServiceProvider epService) {
            string stmtTextOne = "select var, xpathAttrNum.after(xpathAttrNumTwo) from TestXMLNoSchemaType#length(100)";
            epService.EPAdministrator.CreateEPL(stmtTextOne);
        }
    }
} // end of namespace
