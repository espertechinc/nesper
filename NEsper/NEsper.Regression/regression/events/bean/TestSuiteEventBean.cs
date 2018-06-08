///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using NUnit.Framework;

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.execution;

namespace com.espertech.esper.regression.events.bean
{
    [TestFixture]
    public class TestSuiteEventBean
    {
        [Test]
        public void TestExecEventBeanEventPropertyDynamicPerformance() {
            RegressionRunner.Run(new ExecEventBeanEventPropertyDynamicPerformance());
        }
    
        [Test]
        public void TestExecEventBeanAddRemoveType() {
            RegressionRunner.Run(new ExecEventBeanAddRemoveType());
        }
    
        [Test]
        public void TestExecEventBeanPublicAccessors() {
            RegressionRunner.Run(new ExecEventBeanPublicAccessors(true));
            RegressionRunner.Run(new ExecEventBeanPublicAccessors(false));
        }
    
        [Test]
        public void TestExecEventBeanExplicitOnly() {
            RegressionRunner.Run(new ExecEventBeanExplicitOnly(true));
            RegressionRunner.Run(new ExecEventBeanExplicitOnly(false));
        }
    
        [Test]
        public void TestExecEventBeanJavaBeanAccessor() {
            RegressionRunner.Run(new ExecEventBeanAccessor(true));
            RegressionRunner.Run(new ExecEventBeanAccessor(false));
        }
    
        [Test]
        public void TestExecEventBeanFinalClass() {
            RegressionRunner.Run(new ExecEventBeanFinalClass(true));
            RegressionRunner.Run(new ExecEventBeanFinalClass(false));
        }
    
        [Test]
        public void TestExecEventBeanMappedIndexedPropertyExpression() {
            RegressionRunner.Run(new ExecEventBeanMappedIndexedPropertyExpression());
        }
    
        [Test]
        public void TestExecEventBeanPropertyResolutionWDefaults() {
            RegressionRunner.Run(new ExecEventBeanPropertyResolutionWDefaults());
        }
    
        [Test]
        public void TestExecEventBeanPropertyResolutionCaseInsensitive() {
            RegressionRunner.Run(new ExecEventBeanPropertyResolutionCaseInsensitive());
        }
    
        [Test]
        public void TestExecEventBeanPropertyResolutionAccessorStyleGlobalPublic() {
            RegressionRunner.Run(new ExecEventBeanPropertyResolutionAccessorStyleGlobalPublic());
        }
    
        [Test]
        public void TestExecEventBeanPropertyResolutionCaseDistinctInsensitive() {
            RegressionRunner.Run(new ExecEventBeanPropertyResolutionCaseDistinctInsensitive());
        }
    
        [Test]
        public void TestExecEventBeanPropertyResolutionCaseInsensitiveEngineDefault() {
            RegressionRunner.Run(new ExecEventBeanPropertyResolutionCaseInsensitiveEngineDefault());
        }
    
        [Test]
        public void TestExecEventBeanPropertyResolutionCaseInsensitiveConfigureType() {
            RegressionRunner.Run(new ExecEventBeanPropertyResolutionCaseInsensitiveConfigureType());
        }
    
        [Test]
        public void TestExecEventBeanPropertyResolutionFragment() {
            RegressionRunner.Run(new ExecEventBeanPropertyResolutionFragment());
        }
    
        [Test]
        public void TestExecEventBeanPropertyIterableMapList() {
            RegressionRunner.Run(new ExecEventBeanPropertyIterableMapList());
        }
    
        [Test]
        public void TestInvalidConfig() {
            var properties = new Properties();
            properties.Put("astring", "XXXX");
    
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddEventType("MyInvalidEvent", properties);
    
            try {
                EPServiceProvider epServiceInner = EPServiceProviderManager.GetDefaultProvider(configuration);
                epServiceInner.Initialize();
                Assert.Fail();
            } catch (ConfigurationException) {
                // expected
            }
    
            EPServiceProvider epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            epService.Initialize();
        }
    }
} // end of namespace
