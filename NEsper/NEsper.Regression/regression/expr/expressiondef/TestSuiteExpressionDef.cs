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

namespace com.espertech.esper.regression.expr.expressiondef
{
    [TestFixture]
    public class TestSuiteExpressionDef
    {
        [Test]
        public void TestExecExpressionDef() {
            RegressionRunner.Run(new ExecExpressionDef());
        }
    
        [Test]
        public void TestExecExpressionDefAliasFor() {
            RegressionRunner.Run(new ExecExpressionDefAliasFor());
        }
    
        [Test]
        public void TestExecExpressionDefLambdaLocReport() {
            RegressionRunner.Run(new ExecExpressionDefLambdaLocReport());
        }
    
        [Test]
        public void TestExecExpressionDefConfigurations() {
            RegressionRunner.Run(new ExecExpressionDefConfigurations(null, 4));
            RegressionRunner.Run(new ExecExpressionDefConfigurations(0, 4));
            RegressionRunner.Run(new ExecExpressionDefConfigurations(1, 4));
            RegressionRunner.Run(new ExecExpressionDefConfigurations(2, 2));
        }
    }
} // end of namespace
