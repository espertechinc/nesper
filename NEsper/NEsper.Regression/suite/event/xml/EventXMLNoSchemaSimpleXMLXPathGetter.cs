///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.regressionlib.framework;

using static com.espertech.esper.regressionlib.suite.@event.xml.EventXMLNoSchemaSimpleXMLDOMGetter;
using static com.espertech.esper.regressionlib.suite.@event.xml.EventXMLNoSchemaSimpleXMLXPathProperties;

namespace com.espertech.esper.regressionlib.suite.@event.xml
{
    public class EventXMLNoSchemaSimpleXMLXPathGetter : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var stmt = "@Name('s0') select " +
                       "element1, " +
                       "invalidelement, " +
                       "element4.element41 as nestedElement," +
                       "element2.element21('e21_2') as mappedElement," +
                       "element2.element21[1] as indexedElement," +
                       "element3.myattribute as invalidattribute " +
                       "from TestXMLNoSchemaTypeWXPathPropTrue#length(100)";
            env.CompileDeploy(stmt).AddListener("s0");

            // Generate document with the specified in element1 to confirm we have independent events
            SendEvent(env, "EventA", "TestXMLNoSchemaTypeWXPathPropTrue");
            AssertDataGetter(env, "EventA", true);

            SendEvent(env, "EventB", "TestXMLNoSchemaTypeWXPathPropTrue");
            AssertDataGetter(env, "EventB", true);

            env.UndeployAll();
        }
    }
} // end of namespace