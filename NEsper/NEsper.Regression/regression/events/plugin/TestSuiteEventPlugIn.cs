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

namespace com.espertech.esper.regression.events.plugin
{
    [TestFixture]
    public class TestSuiteEventPlugIn
    {
        [Test]
        public void TestExecEventPlugInConfigStaticTypeResolution() {
            RegressionRunner.Run(new ExecEventPlugInConfigStaticTypeResolution());
        }
    
        [Test]
        public void TestExecEventPlugInConfigRuntimeTypeResolution() {
            RegressionRunner.Run(new ExecEventPlugInConfigRuntimeTypeResolution());
        }
    
        [Test]
        public void TestExecEventPlugInInvalid() {
            RegressionRunner.Run(new ExecEventPlugInInvalid());
        }
    
        [Test]
        public void TestExecEventPlugInContextContent() {
            RegressionRunner.Run(new ExecEventPlugInContextContent());
        }
    
        [Test]
        public void TestExecEventPlugInRuntimeConfigDynamicTypeResolution() {
            RegressionRunner.Run(new ExecEventPlugInRuntimeConfigDynamicTypeResolution());
        }
    
        [Test]
        public void TestExecEventPlugInStaticConfigDynamicTypeResolution() {
            RegressionRunner.Run(new ExecEventPlugInStaticConfigDynamicTypeResolution());
        }
    }
} // end of namespace
