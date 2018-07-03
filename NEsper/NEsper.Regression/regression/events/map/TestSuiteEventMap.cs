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

namespace com.espertech.esper.regression.events.map
{
    [TestFixture]
    public class TestSuiteEventMap
    {
        [Test]
        public void TestExecEventMap() {
            RegressionRunner.Run(new ExecEventMap());
        }
    
        [Test]
        public void TestExecEventMapPropertyConfig() {
            RegressionRunner.Run(new ExecEventMapPropertyConfig());
        }
    
        [Test]
        public void TestExecEventMapPropertyDynamic() {
            RegressionRunner.Run(new ExecEventMapPropertyDynamic());
        }
    
        [Test]
        public void TestExecEventMapObjectArrayInterUse() {
            RegressionRunner.Run(new ExecEventMapObjectArrayInterUse());
        }
    
        [Test]
        public void TestExecEventMapUpdate() {
            RegressionRunner.Run(new ExecEventMapUpdate());
        }
    
        [Test]
        public void TestExecEventMapInheritanceInitTime() {
            RegressionRunner.Run(new ExecEventMapInheritanceInitTime());
        }
    
        [Test]
        public void TestExecEventMapInheritanceRuntime() {
            RegressionRunner.Run(new ExecEventMapInheritanceRuntime());
        }
    
        [Test]
        public void TestExecEventMapNestedEscapeDot() {
            RegressionRunner.Run(new ExecEventMapNestedEscapeDot());
        }
    
        [Test]
        public void TestExecEventMapNestedConfigRuntime() {
            RegressionRunner.Run(new ExecEventMapNestedConfigRuntime());
        }
    
        [Test]
        public void TestExecEventMapNestedConfigStatic() {
            RegressionRunner.Run(new ExecEventMapNestedConfigStatic());
        }
    
        [Test]
        public void TestExecEventMapNested() {
            RegressionRunner.Run(new ExecEventMapNested());
        }
    
        [Test]
        public void TestExecEventMapAddIdenticalMapTypes() {
            RegressionRunner.Run(new ExecEventMapAddIdenticalMapTypes());
        }
    
        [Test]
        public void TestExecEventMapInvalidType() {
            RegressionRunner.Run(new ExecEventMapInvalidType());
        }
    
        [Test]
        public void TestExecEventMapProperties() {
            RegressionRunner.Run(new ExecEventMapProperties());
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
