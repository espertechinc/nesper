///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.supportunit.bean;

using NUnit.Framework;

namespace com.espertech.esper.events.bean
{
    [TestFixture]
    public class TestPropertyListBuilderPublic
    {
        private PropertyListBuilderPublic _builder;
    
        [SetUp]
        public void SetUp() {
            var config = new ConfigurationEventTypeLegacy();
    
            // add 2 explicit properties, also supported
            config.AddFieldProperty("x", "fieldNested");
            config.AddMethodProperty("y", "ReadLegacyBeanVal");
    
            _builder = new PropertyListBuilderPublic(config);
        }
    
        [Test]
        public void TestBuildPropList()
        {
            var descList = _builder.AssessProperties(typeof(SupportLegacyBean));
    
            var expected = new List<InternalEventPropDescriptor>();
            expected.Add(new InternalEventPropDescriptor("fieldLegacyVal", typeof(SupportLegacyBean).GetField("fieldLegacyVal"), EventPropertyType.SIMPLE));
            expected.Add(new InternalEventPropDescriptor("fieldStringArray", typeof(SupportLegacyBean).GetField("fieldStringArray"), EventPropertyType.SIMPLE));
            expected.Add(new InternalEventPropDescriptor("fieldMapped", typeof(SupportLegacyBean).GetField("fieldMapped"), EventPropertyType.SIMPLE));
            expected.Add(new InternalEventPropDescriptor("fieldNested", typeof(SupportLegacyBean).GetField("fieldNested"), EventPropertyType.SIMPLE));
    
            expected.Add(new InternalEventPropDescriptor("ReadLegacyBeanVal", typeof(SupportLegacyBean).GetMethod("ReadLegacyBeanVal"), EventPropertyType.SIMPLE));
            expected.Add(new InternalEventPropDescriptor("ReadStringArray", typeof(SupportLegacyBean).GetMethod("ReadStringArray"), EventPropertyType.SIMPLE));
            expected.Add(new InternalEventPropDescriptor("ReadStringIndexed", typeof(SupportLegacyBean).GetMethod("ReadStringIndexed", new[]{typeof(int)}), EventPropertyType.INDEXED));
            expected.Add(new InternalEventPropDescriptor("ReadMapByKey", typeof(SupportLegacyBean).GetMethod("ReadMapByKey", new[]{typeof(string)}), EventPropertyType.MAPPED));
            expected.Add(new InternalEventPropDescriptor("ReadMap", typeof(SupportLegacyBean).GetMethod("ReadMap"), EventPropertyType.SIMPLE));
            expected.Add(new InternalEventPropDescriptor("ReadLegacyNested", typeof(SupportLegacyBean).GetMethod("ReadLegacyNested"), EventPropertyType.SIMPLE));
    
            expected.Add(new InternalEventPropDescriptor("x", typeof(SupportLegacyBean).GetField("fieldNested"), EventPropertyType.SIMPLE));
            expected.Add(new InternalEventPropDescriptor("y", typeof(SupportLegacyBean).GetMethod("ReadLegacyBeanVal"), EventPropertyType.SIMPLE));
            EPAssertionUtil.AssertEqualsAnyOrder(expected.ToArray(), descList.ToArray());
        }
    }
}
