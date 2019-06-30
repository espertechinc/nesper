///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.supportunit.bean;
using com.espertech.esper.compat.logging;
using NUnit.Framework;

namespace com.espertech.esper.common.@internal.@event.bean.introspect
{
    [TestFixture]
    public class TestPropertyListBuilderPublic : CommonTest
    {
        private PropertyListBuilderPublic builder;

        [SetUp]
        public void SetUp()
        {
            ConfigurationCommonEventTypeBean config = new ConfigurationCommonEventTypeBean();

            // add 2 explicit properties, also supported
            config.AddFieldProperty("x", "fieldNested");
            config.AddMethodProperty("y", "readLegacyBeanVal");

            builder = new PropertyListBuilderPublic(config);
        }

        [Test]
        public void TestBuildPropList()
        {
            IList<PropertyStem> descList = builder.AssessProperties(typeof(SupportLegacyBean));

            IList<PropertyStem> expected = new List<PropertyStem>();
            expected.Add(new PropertyStem("fieldLegacyVal", typeof(SupportLegacyBean).GetField("fieldLegacyVal"), EventPropertyType.SIMPLE));
            expected.Add(new PropertyStem("fieldStringArray", typeof(SupportLegacyBean).GetField("fieldStringArray"), EventPropertyType.SIMPLE));
            expected.Add(new PropertyStem("fieldMapped", typeof(SupportLegacyBean).GetField("fieldMapped"), EventPropertyType.SIMPLE));
            expected.Add(new PropertyStem("fieldNested", typeof(SupportLegacyBean).GetField("fieldNested"), EventPropertyType.SIMPLE));

            expected.Add(new PropertyStem("readLegacyBeanVal", typeof(SupportLegacyBean).GetMethod("readLegacyBeanVal"), EventPropertyType.SIMPLE));
            expected.Add(new PropertyStem("readStringArray", typeof(SupportLegacyBean).GetMethod("readStringArray"), EventPropertyType.SIMPLE));
            expected.Add(new PropertyStem("readStringIndexed", typeof(SupportLegacyBean).GetMethod("readStringIndexed", new Type[] { typeof(int) }), EventPropertyType.INDEXED));
            expected.Add(new PropertyStem("readMapByKey", typeof(SupportLegacyBean).GetMethod("readMapByKey", new Type[] { typeof(string) }), EventPropertyType.MAPPED));
            expected.Add(new PropertyStem("readMap", typeof(SupportLegacyBean).GetMethod("readMap"), EventPropertyType.SIMPLE));
            expected.Add(new PropertyStem("readLegacyNested", typeof(SupportLegacyBean).GetMethod("readLegacyNested"), EventPropertyType.SIMPLE));

            expected.Add(new PropertyStem("x", typeof(SupportLegacyBean).GetField("fieldNested"), EventPropertyType.SIMPLE));
            expected.Add(new PropertyStem("y", typeof(SupportLegacyBean).GetMethod("readLegacyBeanVal"), EventPropertyType.SIMPLE));
            EPAssertionUtil.AssertEqualsAnyOrder(expected.ToArray(), descList.ToArray());
        }

        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace