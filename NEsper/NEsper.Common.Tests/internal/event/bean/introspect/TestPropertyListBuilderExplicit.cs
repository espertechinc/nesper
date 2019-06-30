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
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.supportunit.bean;
using com.espertech.esper.compat.logging;
using NUnit.Framework;

namespace com.espertech.esper.common.@internal.@event.bean.introspect
{
    [TestFixture]
    public class TestPropertyListBuilderExplicit : CommonTest
    {
        private PropertyListBuilderExplicit builder;

        [SetUp]
        public void SetUp()
        {
            ConfigurationCommonEventTypeBean config = new ConfigurationCommonEventTypeBean();
            config.AddFieldProperty("f_legVal", "fieldLegacyVal");
            config.AddFieldProperty("f_strArr", "fieldStringArray");
            config.AddFieldProperty("f_strMap", "fieldMapped");
            config.AddFieldProperty("f_legNested", "fieldNested");

            config.AddMethodProperty("m_legVal", "readLegacyBeanVal");
            config.AddMethodProperty("m_strArr", "readStringArray");
            config.AddMethodProperty("m_strInd", "readStringIndexed");
            config.AddMethodProperty("m_strMapKeyed", "readMapByKey");
            config.AddMethodProperty("m_strMap", "readMap");
            config.AddMethodProperty("m_legNested", "readLegacyNested");

            builder = new PropertyListBuilderExplicit(config);
        }

        [Test]
        public void TestBuildPropList()
        {
            IList<PropertyStem> descList = builder.AssessProperties(typeof(SupportLegacyBean));

            IList<PropertyStem> expected = new List<PropertyStem>();
            expected.Add(new PropertyStem("f_legVal", typeof(SupportLegacyBean).GetField("fieldLegacyVal"), EventPropertyType.SIMPLE));
            expected.Add(new PropertyStem("f_strArr", typeof(SupportLegacyBean).GetField("fieldStringArray"), EventPropertyType.SIMPLE));
            expected.Add(new PropertyStem("f_strMap", typeof(SupportLegacyBean).GetField("fieldMapped"), EventPropertyType.SIMPLE));
            expected.Add(new PropertyStem("f_legNested", typeof(SupportLegacyBean).GetField("fieldNested"), EventPropertyType.SIMPLE));

            expected.Add(new PropertyStem("m_legVal", typeof(SupportLegacyBean).GetMethod("readLegacyBeanVal"), EventPropertyType.SIMPLE));
            expected.Add(new PropertyStem("m_strArr", typeof(SupportLegacyBean).GetMethod("readStringArray"), EventPropertyType.SIMPLE));
            expected.Add(new PropertyStem("m_strInd", typeof(SupportLegacyBean).GetMethod("readStringIndexed", new Type[] { typeof(int) }), EventPropertyType.INDEXED));
            expected.Add(new PropertyStem("m_strMapKeyed", typeof(SupportLegacyBean).GetMethod("readMapByKey", new Type[] { typeof(string) }), EventPropertyType.MAPPED));
            expected.Add(new PropertyStem("m_strMap", typeof(SupportLegacyBean).GetMethod("readMap"), EventPropertyType.SIMPLE));
            expected.Add(new PropertyStem("m_legNested", typeof(SupportLegacyBean).GetMethod("readLegacyNested"), EventPropertyType.SIMPLE));

            EPAssertionUtil.AssertEqualsAnyOrder(expected.ToArray(), descList.ToArray());
        }

        [Test]
        public void TestInvalid()
        {
            TryInvalidField("x", typeof(SupportBean));
            TryInvalidField("intPrimitive", typeof(SupportBean));

            TryInvalidMethod("x", typeof(SupportBean));
            TryInvalidMethod("intPrimitive", typeof(SupportBean));
        }

        private void TryInvalidMethod(string methodName, Type clazz)
        {
            ConfigurationCommonEventTypeBean config = new ConfigurationCommonEventTypeBean();
            config.AddMethodProperty("name", methodName);
            builder = new PropertyListBuilderExplicit(config);

            try
            {
                builder.AssessProperties(clazz);
            }
            catch (ConfigurationException ex)
            {
                // expected
                log.Debug(ex.Message);
            }
        }

        private void TryInvalidField(string fieldName, Type clazz)
        {
            ConfigurationCommonEventTypeBean config = new ConfigurationCommonEventTypeBean();
            config.AddFieldProperty("name", fieldName);
            builder = new PropertyListBuilderExplicit(config);

            try
            {
                builder.AssessProperties(clazz);
            }
            catch (ConfigurationException ex)
            {
                // expected
                log.Debug(ex.Message);
            }
        }

        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace