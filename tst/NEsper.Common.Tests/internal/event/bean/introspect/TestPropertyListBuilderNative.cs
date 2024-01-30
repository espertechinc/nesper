///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.supportunit.bean;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using NUnit.Framework;

namespace com.espertech.esper.common.@internal.@event.bean.introspect
{
    [TestFixture]
    public class TestPropertyListBuilderNative : AbstractCommonTest
    {
        [SetUp]
        public void SetUp()
        {
            var config = new ConfigurationCommonEventTypeBean();

            // add 2 explicit properties, also supported
            config.AddFieldProperty("x", "fieldNested");
            config.AddMethodProperty("y", "ReadLegacyBeanVal");

            builder = new PropertyListBuilderNative(config);
        }

        private PropertyListBuilderNative builder;

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void TestBuildPropList()
        {
            var descList = builder.AssessProperties(typeof(SupportLegacyBean));

            IList<PropertyStem> expected = new List<PropertyStem>();
            expected.Add(new PropertyStem("x", typeof(SupportLegacyBean).GetField("fieldNested"), PropertyType.SIMPLE));
            expected.Add(new PropertyStem("y", typeof(SupportLegacyBean).GetMethod("ReadLegacyBeanVal"), PropertyType.SIMPLE | PropertyType.INDEXED));
            CollectionAssert.AreEquivalent(expected, descList);
        }
    }
} // end of namespace
