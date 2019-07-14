///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Xml;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.xml
{
    public class EventXMLNoSchemaEventTransposeXPathGetter : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            // note class not a fragment
            env.CompileDeploy("@Name('s0') insert into MyNestedStream select nested1 from TestXMLSchemaTypeTXG");
            EPAssertionUtil.AssertEqualsAnyOrder(
                new object[] {
                    new EventPropertyDescriptor("nested1", typeof(XmlNode), null, false, false, false, false, false)
                },
                env.Statement("s0").EventType.PropertyDescriptors);
            SupportEventTypeAssertionUtil.AssertConsistency(env.Statement("s0").EventType);

            var type = env.Runtime.EventTypeService.GetEventTypePreconfigured("TestXMLSchemaTypeTXG");
            SupportEventTypeAssertionUtil.AssertConsistency(type);
            Assert.IsNull(type.GetFragmentType("nested1"));
            Assert.IsNull(type.GetFragmentType("nested1.nested2"));

            SupportXML.SendDefaultEvent(env.EventService, "ABC", "TestXMLSchemaTypeTXG");
            SupportEventTypeAssertionUtil.AssertConsistency(env.GetEnumerator("s0").Advance());

            env.UndeployAll();
        }
    }
} // end of namespace