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

namespace com.espertech.esper.regression.nwtable.infra
{
    [TestFixture]
    public class TestSuiteInfra
    {
        [Test]
        public void TestExecNWTableInfraComparative() {
            RegressionRunner.Run(new ExecNWTableInfraComparative());
        }
    
        [Test]
        public void TestExecNWTableInfraContext() {
            RegressionRunner.Run(new ExecNWTableInfraContext());
        }
    
        [Test]
        public void TestExecNWTableInfraCreateIndex() {
            RegressionRunner.Run(new ExecNWTableInfraCreateIndex());
        }
    
        [Test]
        public void TestExecNWTableInfraEventType() {
            RegressionRunner.Run(new ExecNWTableInfraEventType());
        }
    
        [Test]
        public void TestExecNWTableInfraExecuteQuery() {
            RegressionRunner.Run(new ExecNWTableInfraExecuteQuery());
        }
    
        [Test]
        public void TestExecNWTableInfraIndexFAF() {
            RegressionRunner.Run(new ExecNWTableInfraIndexFAF());
        }
    
        [Test]
        public void TestExecNWTableInfraIndexFAFPerf() {
            RegressionRunner.Run(new ExecNWTableInfraIndexFAFPerf());
        }
    
        [Test]
        public void TestExecNWTableInfraCreateIndexAdvancedSyntax() {
            RegressionRunner.Run(new ExecNWTableInfraCreateIndexAdvancedSyntax());
        }
    
        [Test]
        public void TestExecNWTableInfraOnDelete() {
            RegressionRunner.Run(new ExecNWTableInfraOnDelete());
        }
    
        [Test]
        public void TestExecNWTableInfraOnMerge() {
            RegressionRunner.Run(new ExecNWTableInfraOnMerge());
        }
    
        [Test]
        public void TestExecNWTableInfraOnMergePerf() {
            RegressionRunner.Run(new ExecNWTableInfraOnMergePerf());
        }
    
        [Test]
        public void TestExecNWTableInfraOnSelect() {
            RegressionRunner.Run(new ExecNWTableInfraOnSelect());
        }
    
        [Test]
        public void TestExecNWTableOnSelectWDelete() {
            RegressionRunner.Run(new ExecNWTableOnSelectWDelete());
        }
    
        [Test]
        public void TestExecNWTableInfraOnUpdate() {
            RegressionRunner.Run(new ExecNWTableInfraOnUpdate());
        }
    
        [Test]
        public void TestExecNWTableInfraStartStop() {
            RegressionRunner.Run(new ExecNWTableInfraStartStop());
        }
    
        [Test]
        public void TestExecNWTableInfraSubqCorrelCoerce() {
            RegressionRunner.Run(new ExecNWTableInfraSubqCorrelCoerce());
        }
    
        [Test]
        public void TestExecNWTableInfraSubqCorrelIndex() {
            RegressionRunner.Run(new ExecNWTableInfraSubqCorrelIndex());
        }
    
        [Test]
        public void TestExecNWTableInfraSubqCorrelJoin() {
            RegressionRunner.Run(new ExecNWTableInfraSubqCorrelJoin());
        }
    
        [Test]
        public void TestExecNWTableInfraSubquery() {
            RegressionRunner.Run(new ExecNWTableInfraSubquery());
        }
    
        [Test]
        public void TestExecNWTableInfraSubqueryAtEventBean() {
            RegressionRunner.Run(new ExecNWTableInfraSubqueryAtEventBean());
        }
    
        [Test]
        public void TestExecNWTableInfraSubqUncorrel() {
            RegressionRunner.Run(new ExecNWTableInfraSubqUncorrel());
        }
    
    }
} // end of namespace
