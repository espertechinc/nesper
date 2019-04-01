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
using com.espertech.esper.supportunit.bean;

using NUnit.Framework;

namespace com.espertech.esper.events.bean
{
    [TestFixture]
    public class TestPropertyListBuilderNative 
    {
        private PropertyListBuilderNative _builder;
    
        [SetUp]
        public void SetUp()
        {
            ConfigurationEventTypeLegacy config = new ConfigurationEventTypeLegacy();
    
            // add 2 explicit properties, also supported
            config.AddFieldProperty("x", "fieldNested");
            config.AddMethodProperty("y", "ReadLegacyBeanVal");

            _builder = new PropertyListBuilderNative(config);
        }
    
        [Test]
        public void TestBuildPropList()
        {
            IList<InternalEventPropDescriptor> descList = _builder.AssessProperties(typeof(SupportLegacyBean));
    
            IList<InternalEventPropDescriptor> expected = new List<InternalEventPropDescriptor>();
            expected.Add(new InternalEventPropDescriptor("x", typeof(SupportLegacyBean).GetField("fieldNested"), EventPropertyType.SIMPLE));
            expected.Add(new InternalEventPropDescriptor("y", typeof(SupportLegacyBean).GetMethod("ReadLegacyBeanVal"), EventPropertyType.SIMPLE));
            EPAssertionUtil.AssertEqualsAnyOrder(expected, descList);
        }
    }
}
