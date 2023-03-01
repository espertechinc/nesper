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
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.suite.@event.bean;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionrun.runner;
using com.espertech.esper.regressionrun.suite.core;

using NUnit.Framework;

using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;

namespace com.espertech.esper.regressionrun.suite.@event
{
    [TestFixture]
    public class TestSuiteEventBeanWConfig : AbstractTestContainer
    {
        [Test, RunInApplicationDomain]
        public void TestEventBeanPropertyResolutionAccessorStyleGlobalPublic()
        {
            using (var session = RegressionRunner.Session(Container)) {
                session.Configuration.Common.EventMeta.DefaultAccessorStyle = AccessorStyle.PUBLIC;
                session.Configuration.Common.AddEventType(typeof(SupportLegacyBean));
                RegressionRunner.Run(session, new EventBeanPropertyResolutionAccessorStyleGlobalPublic());
            }
        }

        [Test, RunInApplicationDomain]
        public void TestEventBeanPropertyResolutionCaseDistinctInsensitive()
        {
            using (var session = RegressionRunner.Session(Container)) {
                session.Configuration.Common.EventMeta.ClassPropertyResolutionStyle = PropertyResolutionStyle.DISTINCT_CASE_INSENSITIVE;
                session.Configuration.Common.AddEventType(typeof(SupportBeanDupProperty));
                RegressionRunner.Run(session, new EventBeanPropertyResolutionCaseDistinctInsensitive());
            }
        }

        [Test, RunInApplicationDomain]
        public void TestEventBeanPropertyResolutionCaseInsensitive()
        {
            using (var session = RegressionRunner.Session(Container)) {
                session.Configuration.Common.EventMeta.ClassPropertyResolutionStyle = PropertyResolutionStyle.CASE_INSENSITIVE;
                session.Configuration.Common.AddEventType(typeof(SupportBeanDupProperty));
                session.Configuration.Common.AddEventType(typeof(SupportBeanComplexProps));
                RegressionRunner.Run(session, new EventBeanPropertyResolutionCaseInsensitive());
            }
        }

        [Test, RunInApplicationDomain]
        public void TestEventBeanPropertyResolutionCaseInsensitiveEngineDefault()
        {
            using (var session = RegressionRunner.Session(Container)) {
                session.Configuration.Common.EventMeta.ClassPropertyResolutionStyle = PropertyResolutionStyle.CASE_INSENSITIVE;
                session.Configuration.Common.AddEventType("BeanWCIED", typeof(SupportBean));
                RegressionRunner.Run(session, new EventBeanPropertyResolutionCaseInsensitiveEngineDefault());
            }
        }

        [Test, RunInApplicationDomain]
        public void TestEventBeanPublicFields()
        {
            using (var session = RegressionRunner.Session(Container)) {

                ConfigurationCommonEventTypeBean legacyDef = new ConfigurationCommonEventTypeBean();
                legacyDef.AccessorStyle = AccessorStyle.PUBLIC;
                session.Configuration.Common.AddEventType("MyLegacyNestedEvent", typeof(SupportLegacyBean.LegacyNested), legacyDef);

                ConfigurationCommonEventTypeBean anotherLegacyEvent = new ConfigurationCommonEventTypeBean();
                anotherLegacyEvent.AccessorStyle = AccessorStyle.PUBLIC;
                anotherLegacyEvent.AddFieldProperty("explicitFSimple", "fieldLegacyVal");
                anotherLegacyEvent.AddFieldProperty("explicitFIndexed", "fieldStringArray");
                anotherLegacyEvent.AddFieldProperty("explicitFNested", "fieldNested");
                anotherLegacyEvent.AddMethodProperty("explicitMSimple", "ReadLegacyBeanVal");
                anotherLegacyEvent.AddMethodProperty("explicitMArray", "ReadStringArray");
                anotherLegacyEvent.AddMethodProperty("explicitMIndexed", "ReadStringIndexed");
                anotherLegacyEvent.AddMethodProperty("explicitMMapped", "ReadMapByKey");
                session.Configuration.Common.AddEventType("AnotherLegacyEvent", typeof(SupportLegacyBean), anotherLegacyEvent);

                RegressionRunner.Run(session, new EventBeanPublicAccessors());
            }
        }

        [Test, RunInApplicationDomain]
        public void TestEventBeanPropertyResolutionCaseInsensitiveConfigureType()
        {
            using (var session = RegressionRunner.Session(Container)) {

                ConfigurationCommonEventTypeBean beanWithCaseInsensitive = new ConfigurationCommonEventTypeBean();
                beanWithCaseInsensitive.PropertyResolutionStyle = PropertyResolutionStyle.CASE_INSENSITIVE;
                session.Configuration.Common.AddEventType("BeanWithCaseInsensitive", typeof(SupportBean), beanWithCaseInsensitive);

                RegressionRunner.Run(session, new EventBeanPropertyResolutionCaseInsensitiveConfigureType());
            }
        }

        [Test, RunInApplicationDomain]
        public void TestEventBeanExplicitOnly()
        {
            using (var session = RegressionRunner.Session(Container)) {

                ConfigurationCommonEventTypeBean legacyDef = new ConfigurationCommonEventTypeBean();
                legacyDef.AccessorStyle = AccessorStyle.EXPLICIT;
                legacyDef.AddFieldProperty("explicitFNested", "fieldNested");
                legacyDef.AddMethodProperty("explicitMNested", "ReadLegacyNested");
                session.Configuration.Common.AddEventType("MyLegacyEvent", typeof(SupportLegacyBean), legacyDef);

                legacyDef = new ConfigurationCommonEventTypeBean();
                legacyDef.AccessorStyle = AccessorStyle.EXPLICIT;
                legacyDef.AddFieldProperty("fieldNestedClassValue", "fieldNestedValue");
                legacyDef.AddMethodProperty("readNestedClassValue", "ReadNestedValue");
                session.Configuration.Common.AddEventType("MyLegacyNestedEvent", typeof(SupportLegacyBean.LegacyNested), legacyDef);

                ConfigurationCommonEventTypeBean mySupportBean = new ConfigurationCommonEventTypeBean();
                mySupportBean.AccessorStyle = AccessorStyle.EXPLICIT;
                session.Configuration.Common.AddEventType("MySupportBean", typeof(SupportBean), mySupportBean);

                RegressionRunner.Run(session, new EventBeanExplicitOnly());
            }
        }
    }
} // end of namespace