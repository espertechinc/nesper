///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.supportunit.bean;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using NUnit.Framework;

namespace com.espertech.esper.common.@internal.@event.bean.introspect
{
    [TestFixture]
    public class TestPropertyListBuilderPublic : AbstractCommonTest
    {
        private PropertyListBuilderPublic builder;

        [SetUp]
        public void SetUp()
        {
            ConfigurationCommonEventTypeBean config = new ConfigurationCommonEventTypeBean();

            // add 2 explicit properties, also supported
            config.AddFieldProperty("x", "fieldNested");
            config.AddMethodProperty("y", "ReadLegacyBeanVal");

            builder = new PropertyListBuilderPublic(config);
        }

        [Test]
        public void TestBuildPropList()
        {
            IList<PropertyStem> descList = builder.AssessProperties(typeof(SupportLegacyBean));

            IList<PropertyStem> expected = new List<PropertyStem>();
            expected.Add(new PropertyStem("fieldLegacyVal", typeof(SupportLegacyBean).GetField("fieldLegacyVal"), PropertyType.SIMPLE | PropertyType.INDEXED));
            expected.Add(new PropertyStem("fieldStringArray", typeof(SupportLegacyBean).GetField("fieldStringArray"), PropertyType.SIMPLE | PropertyType.INDEXED));
            expected.Add(new PropertyStem("fieldMapped", typeof(SupportLegacyBean).GetField("fieldMapped"), PropertyType.SIMPLE | PropertyType.MAPPED));
            expected.Add(new PropertyStem("fieldNested", typeof(SupportLegacyBean).GetField("fieldNested"), PropertyType.SIMPLE));

            expected.Add(new PropertyStem("ReadLegacyBeanVal", typeof(SupportLegacyBean).GetMethod("ReadLegacyBeanVal"), PropertyType.SIMPLE | PropertyType.INDEXED));
            expected.Add(new PropertyStem("ReadStringArray", typeof(SupportLegacyBean).GetMethod("ReadStringArray"), PropertyType.SIMPLE | PropertyType.INDEXED));
            expected.Add(new PropertyStem("ReadStringIndexed", typeof(SupportLegacyBean).GetMethod("ReadStringIndexed", new Type[] { typeof(int) }), PropertyType.INDEXED));
            expected.Add(new PropertyStem("ReadMapByKey", typeof(SupportLegacyBean).GetMethod("ReadMapByKey", new Type[] { typeof(string) }), PropertyType.MAPPED));
            expected.Add(new PropertyStem("ReadMap", typeof(SupportLegacyBean).GetMethod("ReadMap"), PropertyType.SIMPLE | PropertyType.MAPPED));
            expected.Add(new PropertyStem("ReadLegacyNested", typeof(SupportLegacyBean).GetMethod("ReadLegacyNested"), PropertyType.SIMPLE));

            expected.Add(new PropertyStem("x", typeof(SupportLegacyBean).GetField("fieldNested"), PropertyType.SIMPLE));
            expected.Add(new PropertyStem("y", typeof(SupportLegacyBean).GetMethod("ReadLegacyBeanVal"), PropertyType.SIMPLE | PropertyType.INDEXED));
            
            CollectionAssert.AreEquivalent(expected, descList);
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
