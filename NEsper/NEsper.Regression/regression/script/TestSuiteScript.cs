///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.script
{
    [TestFixture]
    public class TestSuiteScript
    {
        [Test]
        public void TestExecScriptExpression() {
            RegressionRunner.Run(new ExecScriptExpression());
        }
    
        [Test]
        public void TestExecScriptExpressionConfiguration() {
            RegressionRunner.Run(new ExecScriptExpressionConfiguration());
        }
    }
} // end of namespace
