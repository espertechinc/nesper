///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl.variable
{
    [TestFixture]
    public class TestSuiteVariable
    {
        [Test]
        public void TestExecVariables() {
            RegressionRunner.Run(new ExecVariables());
        }
    
        [Test]
        public void TestExecVariablesCreate() {
            RegressionRunner.Run(new ExecVariablesCreate());
        }
    
        [Test]
        public void TestExecVariablesDestroy() {
            RegressionRunner.Run(new ExecVariablesDestroy());
        }
    
        [Test]
        public void TestExecVariablesEventTyped() {
            RegressionRunner.Run(new ExecVariablesEventTyped());
        }
    
        [Test]
        public void TestExecVariablesOutputRate() {
            RegressionRunner.Run(new ExecVariablesOutputRate());
        }
    
        [Test]
        public void TestExecVariablesPerf() {
            RegressionRunner.Run(new ExecVariablesPerf());
        }
    
        [Test]
        public void TestExecVariablesTimer() {
            RegressionRunner.Run(new ExecVariablesTimer());
        }
    }
} // end of namespace
