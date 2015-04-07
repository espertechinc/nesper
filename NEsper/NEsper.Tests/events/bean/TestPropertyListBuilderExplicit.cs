///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.logging;
using com.espertech.esper.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.events.bean
{
    [TestFixture]
    public class TestPropertyListBuilderExplicit
    {
        private PropertyListBuilderExplicit _builder;
    
        [SetUp]
        public void SetUp() {
            var config = new ConfigurationEventTypeLegacy();
            config.AddFieldProperty("f_legVal", "fieldLegacyVal");
            config.AddFieldProperty("f_strArr", "fieldStringArray");
            config.AddFieldProperty("f_strMap", "fieldMapped");
            config.AddFieldProperty("f_legNested", "fieldNested");
    
            config.AddMethodProperty("m_legVal", "ReadLegacyBeanVal");
            config.AddMethodProperty("m_strArr", "ReadStringArray");
            config.AddMethodProperty("m_strInd", "ReadStringIndexed");
            config.AddMethodProperty("m_strMapKeyed", "ReadMapByKey");
            config.AddMethodProperty("m_strMap", "ReadMap");
            config.AddMethodProperty("m_legNested", "ReadLegacyNested");
    
            _builder = new PropertyListBuilderExplicit(config);
        }
    
        [Test]
        public void TestBuildPropList()
        {
            var descList = _builder.AssessProperties(typeof(SupportLegacyBean));
    
            var expected = new List<InternalEventPropDescriptor>();
            expected.Add(new InternalEventPropDescriptor("f_legVal", typeof(SupportLegacyBean).GetField("fieldLegacyVal"), EventPropertyType.SIMPLE));
            expected.Add(new InternalEventPropDescriptor("f_strArr", typeof(SupportLegacyBean).GetField("fieldStringArray"), EventPropertyType.SIMPLE));
            expected.Add(new InternalEventPropDescriptor("f_strMap", typeof(SupportLegacyBean).GetField("fieldMapped"), EventPropertyType.SIMPLE));
            expected.Add(new InternalEventPropDescriptor("f_legNested", typeof(SupportLegacyBean).GetField("fieldNested"), EventPropertyType.SIMPLE));
    
            expected.Add(new InternalEventPropDescriptor("m_legVal", typeof(SupportLegacyBean).GetMethod("ReadLegacyBeanVal"), EventPropertyType.SIMPLE));
            expected.Add(new InternalEventPropDescriptor("m_strArr", typeof(SupportLegacyBean).GetMethod("ReadStringArray"), EventPropertyType.SIMPLE));
            expected.Add(new InternalEventPropDescriptor("m_strInd", typeof(SupportLegacyBean).GetMethod("ReadStringIndexed", new[]{typeof(int)}), EventPropertyType.INDEXED));
            expected.Add(new InternalEventPropDescriptor("m_strMapKeyed", typeof(SupportLegacyBean).GetMethod("ReadMapByKey", new[]{typeof(string)}), EventPropertyType.MAPPED));
            expected.Add(new InternalEventPropDescriptor("m_strMap", typeof(SupportLegacyBean).GetMethod("ReadMap"), EventPropertyType.SIMPLE));
            expected.Add(new InternalEventPropDescriptor("m_legNested", typeof(SupportLegacyBean).GetMethod("ReadLegacyNested"), EventPropertyType.SIMPLE));
    
            EPAssertionUtil.AssertEqualsAnyOrder(expected, descList);
        }
    
        [Test]
        public void TestInvalid()
        {
            TryInvalidField("x", typeof(SupportBean));
            TryInvalidField("IntPrimitive", typeof(SupportBean));
    
            TryInvalidMethod("x", typeof(SupportBean));
            TryInvalidMethod("IntPrimitive", typeof(SupportBean));
        }
    
        private void TryInvalidMethod(String methodName, Type clazz)
        {
            var config = new ConfigurationEventTypeLegacy();
            config.AddMethodProperty("name", methodName);
            _builder = new PropertyListBuilderExplicit(config);
    
            try {
                _builder.AssessProperties(clazz);
            } catch (ConfigurationException ex) {
                // expected
                Log.Debug(ex.Message, ex);
            }
        }

        private void TryInvalidField(String fieldName, Type clazz)
        {
            var config = new ConfigurationEventTypeLegacy();
            config.AddFieldProperty("name", fieldName);
            _builder = new PropertyListBuilderExplicit(config);
    
            try {
                _builder.AssessProperties(clazz);
            } catch (ConfigurationException ex) {
                // expected
                Log.Debug(ex.Message, ex);
            }
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
