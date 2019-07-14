///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.suite.@event.bean;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionrun.runner;

using NUnit.Framework;

using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;

namespace com.espertech.esper.regressionrun.suite.@event
{
    [TestFixture]
    public class TestSuiteEventBeanWConfig
    {
        [Test]
        public void TestEventBeanPropertyResolutionAccessorStyleGlobalPublic()
        {
            RegressionSession session = RegressionRunner.Session();
            session.Configuration.Common.EventMeta.DefaultAccessorStyle = AccessorStyle.PUBLIC;
            session.Configuration.Common.AddEventType(typeof(SupportLegacyBean));
            RegressionRunner.Run(session, new EventBeanPropertyResolutionAccessorStyleGlobalPublic());
            session.Destroy();
        }

        [Test]
        public void TestEventBeanPropertyResolutionCaseDistinctInsensitive()
        {
            RegressionSession session = RegressionRunner.Session();
            session.Configuration.Common.EventMeta.ClassPropertyResolutionStyle = PropertyResolutionStyle.DISTINCT_CASE_INSENSITIVE;
            session.Configuration.Common.AddEventType(typeof(SupportBeanDupProperty));
            RegressionRunner.Run(session, new EventBeanPropertyResolutionCaseDistinctInsensitive());
            session.Destroy();
        }

        [Test]
        public void TestEventBeanPropertyResolutionCaseInsensitive()
        {
            RegressionSession session = RegressionRunner.Session();
            session.Configuration.Common.EventMeta.ClassPropertyResolutionStyle = PropertyResolutionStyle.CASE_INSENSITIVE;
            session.Configuration.Common.AddEventType(typeof(SupportBeanDupProperty));
            session.Configuration.Common.AddEventType(typeof(SupportBeanComplexProps));
            RegressionRunner.Run(session, new EventBeanPropertyResolutionCaseInsensitive());
            session.Destroy();
        }

        [Test]
        public void TestEventBeanPropertyResolutionCaseInsensitiveEngineDefault()
        {
            RegressionSession session = RegressionRunner.Session();
            session.Configuration.Common.EventMeta.ClassPropertyResolutionStyle = PropertyResolutionStyle.CASE_INSENSITIVE;
            session.Configuration.Common.AddEventType("BeanWCIED", typeof(SupportBean));
            RegressionRunner.Run(session, new EventBeanPropertyResolutionCaseInsensitiveEngineDefault());
            session.Destroy();
        }

        [Test]
        public void TestEventBeanPublicAccessors()
        {
            RegressionSession session = RegressionRunner.Session();

            ConfigurationCommonEventTypeBean legacyDef = new ConfigurationCommonEventTypeBean();
            legacyDef.AccessorStyle = AccessorStyle.PUBLIC;
            session.Configuration.Common.AddEventType("MyLegacyNestedEvent", typeof(SupportLegacyBean.LegacyNested), legacyDef);

            ConfigurationCommonEventTypeBean anotherLegacyEvent = new ConfigurationCommonEventTypeBean();
            anotherLegacyEvent.AccessorStyle = AccessorStyle.PUBLIC;
            anotherLegacyEvent.AddFieldProperty("explicitFSimple", "fieldLegacyVal");
            anotherLegacyEvent.AddFieldProperty("explicitFIndexed", "fieldStringArray");
            anotherLegacyEvent.AddFieldProperty("explicitFNested", "fieldNested");
            anotherLegacyEvent.AddMethodProperty("explicitMSimple", "readLegacyBeanVal");
            anotherLegacyEvent.AddMethodProperty("explicitMArray", "readStringArray");
            anotherLegacyEvent.AddMethodProperty("explicitMIndexed", "readStringIndexed");
            anotherLegacyEvent.AddMethodProperty("explicitMMapped", "readMapByKey");
            session.Configuration.Common.AddEventType("AnotherLegacyEvent", typeof(SupportLegacyBean), anotherLegacyEvent);

            RegressionRunner.Run(session, new EventBeanPublicAccessors());
            session.Destroy();
        }

        [Test]
        public void TestEventBeanPropertyResolutionCaseInsensitiveConfigureType()
        {
            RegressionSession session = RegressionRunner.Session();

            ConfigurationCommonEventTypeBean beanWithCaseInsensitive = new ConfigurationCommonEventTypeBean();
            beanWithCaseInsensitive.PropertyResolutionStyle = PropertyResolutionStyle.CASE_INSENSITIVE;
            session.Configuration.Common.AddEventType("BeanWithCaseInsensitive", typeof(SupportBean), beanWithCaseInsensitive);

            RegressionRunner.Run(session, new EventBeanPropertyResolutionCaseInsensitiveConfigureType());
            session.Destroy();
        }

        [Test]
        public void TestEventBeanExplicitOnly()
        {
            RegressionSession session = RegressionRunner.Session();

            ConfigurationCommonEventTypeBean legacyDef = new ConfigurationCommonEventTypeBean();
            legacyDef.AccessorStyle = AccessorStyle.EXPLICIT;
            legacyDef.AddFieldProperty("explicitFNested", "fieldNested");
            legacyDef.AddMethodProperty("explicitMNested", "readLegacyNested");
            session.Configuration.Common.AddEventType("MyLegacyEvent", typeof(SupportLegacyBean), legacyDef);

            legacyDef = new ConfigurationCommonEventTypeBean();
            legacyDef.AccessorStyle = AccessorStyle.EXPLICIT;
            legacyDef.AddFieldProperty("fieldNestedClassValue", "fieldNestedValue");
            legacyDef.AddMethodProperty("readNestedClassValue", "readNestedValue");
            session.Configuration.Common.AddEventType("MyLegacyNestedEvent", typeof(SupportLegacyBean.LegacyNested), legacyDef);

            ConfigurationCommonEventTypeBean mySupportBean = new ConfigurationCommonEventTypeBean();
            mySupportBean.AccessorStyle = AccessorStyle.EXPLICIT;
            session.Configuration.Common.AddEventType("MySupportBean", typeof(SupportBean), mySupportBean);

            RegressionRunner.Run(session, new EventBeanExplicitOnly());
            session.Destroy();
        }
    }
} // end of namespace