///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using NUnit.Framework;
using com.espertech.esper.supportregression.execution;

namespace com.espertech.esper.regression.expr.datetime
{
    [TestFixture]
    public class TestSuiteDatetime
    {
        [Test]
        public void TestExecDTBetween() {
            RegressionRunner.Run(new ExecDTBetween());
        }
    
        [Test]
        public void TestExecDTDocSamples() {
            RegressionRunner.Run(new ExecDTDocSamples());
        }
    
        [Test]
        public void TestExecDTFormat() {
            RegressionRunner.Run(new ExecDTFormat());
        }
    
        [Test]
        public void TestExecDTGet() {
            RegressionRunner.Run(new ExecDTGet());
        }
    
        [Test]
        public void TestExecDTIntervalOps() {
            RegressionRunner.Run(new ExecDTIntervalOps());
        }
    
        [Test]
        public void TestExecDTIntervalOpsCreateSchema() {
            RegressionRunner.Run(new ExecDTIntervalOpsCreateSchema());
        }
    
        [Test]
        public void TestExecDTIntervalOpsInvalidConfig() {
            RegressionRunner.Run(new ExecDTIntervalOpsInvalidConfig());
        }
    
        [Test]
        public void TestExecDTInvalid() {
            RegressionRunner.Run(new ExecDTInvalid());
        }
    
        [Test]
        public void TestExecDTMicrosecondResolution() {
            RegressionRunner.Run(new ExecDTMicrosecondResolution());
        }
    
        [Test]
        public void TestExecDTNested() {
            RegressionRunner.Run(new ExecDTNested());
        }
    
        [Test]
        public void TestExecDTPerfBetween() {
            RegressionRunner.Run(new ExecDTPerfBetween());
        }
    
        [Test]
        public void TestExecDTPerfIntervalOps() {
            RegressionRunner.Run(new ExecDTPerfIntervalOps());
        }
    
        [Test]
        public void TestExecDTPlusMinus() {
            RegressionRunner.Run(new ExecDTPlusMinus());
        }
    
        [Test]
        public void TestExecDTProperty() {
            RegressionRunner.Run(new ExecDTProperty());
        }
    
        [Test]
        public void TestExecDTRound() {
            RegressionRunner.Run(new ExecDTRound());
        }
    
        [Test]
        public void TestExecDTSet() {
            RegressionRunner.Run(new ExecDTSet());
        }
    
        [Test]
        public void TestExecDTToDateCalMSec() {
            RegressionRunner.Run(new ExecDTToDateCalMSec());
        }
    
        [Test]
        public void TestExecDTWithDate() {
            RegressionRunner.Run(new ExecDTWithDate());
        }
    
        [Test]
        public void TestExecDTWithMax() {
            RegressionRunner.Run(new ExecDTWithMax());
        }
    
        [Test]
        public void TestExecDTWithMin() {
            RegressionRunner.Run(new ExecDTWithMin());
        }
    
        [Test]
        public void TestExecDTWithTime() {
            RegressionRunner.Run(new ExecDTWithTime());
        }
    }
} // end of namespace
