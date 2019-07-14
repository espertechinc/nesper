///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Linq;
using System.Xml;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.xml
{
    public class EventXMLSchemaEventTransposeXPathGetter : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            // Note that XPath Node results when transposed must be queried by XPath that is also absolute.
            // For example: "nested1" => "/n0:simpleEvent/n0:nested1" results in a Node.
            // That result Node's "prop1" =>  "/n0:simpleEvent/n0:nested1/n0:prop1" and "/n0:nested1/n0:prop1" does NOT result in a value.
            // Therefore property transposal is disabled for Property-XPath expressions.

            // note class not a fragment
            env.CompileDeploy(
                "@Name('s0') insert into MyNestedStream select nested1 from TestXMLSchemaTypeWithSS#lastevent");
            EPAssertionUtil.AssertEqualsAnyOrder(
                new object[] {
                    new EventPropertyDescriptor("nested1", typeof(XmlNode), null, false, false, false, false, false)
                },
                env.Statement("s0").EventType.PropertyDescriptors);
            SupportEventTypeAssertionUtil.AssertConsistency(env.Statement("s0").EventType);

            var type = env.Runtime.EventTypeService.GetEventTypePreconfigured("TestXMLSchemaTypeWithSS");
            SupportEventTypeAssertionUtil.AssertConsistency(type);
            Assert.IsNull(type.GetFragmentType("nested1"));
            Assert.IsNull(type.GetFragmentType("nested1.nested2"));

            SupportXML.SendDefaultEvent(env.EventService, "ABC", "TestXMLSchemaTypeWithSS");
            SupportEventTypeAssertionUtil.AssertConsistency(env.Statement("s0").First());

            env.UndeployAll();
        }
    }
} // end of namespace