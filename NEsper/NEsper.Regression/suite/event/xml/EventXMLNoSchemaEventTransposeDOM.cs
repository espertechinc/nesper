///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.xml
{
    public class EventXMLNoSchemaEventTransposeDOM : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            env.CompileDeploy("@Name('insert') insert into MyNestedStream select nested1 from TestXMLSchemaType");
            EPAssertionUtil.AssertEqualsAnyOrder(
                new EventPropertyDescriptor[] {
                    new EventPropertyDescriptor("nested1", typeof(string), null, false, false, false, false, false)
                },
                env.Statement("insert").EventType.PropertyDescriptors);
            SupportEventTypeAssertionUtil.AssertConsistency(env.Statement("insert").EventType);

            env.CompileDeploy("@Name('s0') select * from TestXMLSchemaType");
            EPAssertionUtil.AssertEqualsAnyOrder(new EventPropertyDescriptor[0], env.Statement("s0").EventType.PropertyDescriptors);
            SupportEventTypeAssertionUtil.AssertConsistency(env.Statement("s0").EventType);

            SupportXML.SendDefaultEvent(env.EventService, "test", "TestXMLSchemaType");
            var stmtInsertWildcardBean = env.GetEnumerator("insert").Advance();
            var stmtSelectWildcardBean = env.GetEnumerator("s0").Advance();
            Assert.IsNotNull(stmtInsertWildcardBean.Get("nested1"));
            SupportEventTypeAssertionUtil.AssertConsistency(stmtSelectWildcardBean);
            SupportEventTypeAssertionUtil.AssertConsistency(env.GetEnumerator("insert").Advance());

            Assert.AreEqual(0, stmtSelectWildcardBean.EventType.PropertyNames.Length);

            env.UndeployAll();
        }
    }
} // end of namespace