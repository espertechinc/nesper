///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using NUnit.Framework;

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.execution;

namespace com.espertech.esper.regression.events.infra
{
    [TestFixture]
    public class TestSuiteEventInfra
    {
        [Test]
        public void TestExecEventInfraPropertyUnderlyingSimple() {
            RegressionRunner.Run(new ExecEventInfraPropertyUnderlyingSimple());
        }
    
        [Test]
        public void TestExecEventInfraPropertyMappedIndexed() {
            RegressionRunner.Run(new ExecEventInfraPropertyMappedIndexed());
        }
    
        [Test]
        public void TestExecEventInfraPropertyDynamicNonSimple() {
            RegressionRunner.Run(new ExecEventInfraPropertyDynamicNonSimple());
        }
    
        [Test]
        public void TestExecEventInfraPropertyDynamicSimple() {
            RegressionRunner.Run(new ExecEventInfraPropertyDynamicSimple());
        }
    
        [Test]
        public void TestExecEventInfraPropertyNestedSimple() {
            RegressionRunner.Run(new ExecEventInfraPropertyNestedSimple());
        }
    
        [Test]
        public void TestExecEventInfraPropertyNestedIndexed() {
            RegressionRunner.Run(new ExecEventInfraPropertyNestedIndexed());
        }
    
        [Test]
        public void TestExecEventInfraPropertyNestedDynamic() {
            RegressionRunner.Run(new ExecEventInfraPropertyNestedDynamic());
        }
    
        [Test]
        public void TestExecEventInfraPropertyNestedDynamicRootedSimple() {
            RegressionRunner.Run(new ExecEventInfraPropertyNestedDynamicRootedSimple());
        }
    
        [Test]
        public void TestExecEventInfraPropertyNestedDynamicDeep() {
            RegressionRunner.Run(new ExecEventInfraPropertyNestedDynamicDeep());
        }
    
        [Test]
        public void TestExecEventInfraPropertyNestedDynamicRootedNonSimple() {
            RegressionRunner.Run(new ExecEventInfraPropertyNestedDynamicRootedNonSimple());
        }
    
        [Test]
        public void TestExecEventInfraEventRenderer() {
            RegressionRunner.Run(new ExecEventInfraEventRenderer());
        }
    
        [Test]
        public void TestExecEventInfraEventSender() {
            RegressionRunner.Run(new ExecEventInfraEventSender());
        }
    
        [Test]
        public void TestExecEventInfraSuperType() {
            RegressionRunner.Run(new ExecEventInfraSuperType());
        }
    
        [Test]
        public void TestExecEventInfraStaticConfiguration() {
            RegressionRunner.Run(new ExecEventInfraStaticConfiguration());
        }
    
        [Test]
        public void TestExecEventInfraPropertyAccessPerformance() {
            RegressionRunner.Run(new ExecEventInfraPropertyAccessPerformance());
        }
    }
} // end of namespace
