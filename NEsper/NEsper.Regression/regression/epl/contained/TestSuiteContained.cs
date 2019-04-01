///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using NUnit.Framework;
using com.espertech.esper.supportregression.execution;

namespace com.espertech.esper.regression.epl.contained
{
    [TestFixture]
    public class TestSuiteContained
    {
        [Test]
        public void TestExecContainedEventArray() {
            RegressionRunner.Run(new ExecContainedEventArray());
        }
    
        [Test]
        public void TestExecContainedEventExample() {
            RegressionRunner.Run(new ExecContainedEventExample());
        }
    
        [Test]
        public void TestExecContainedEventNested() {
            RegressionRunner.Run(new ExecContainedEventNested());
        }
    
        [Test]
        public void TestExecContainedEventSimple() {
            RegressionRunner.Run(new ExecContainedEventSimple());
        }
    
        [Test]
        public void TestExecContainedEventSplitExpr() {
            RegressionRunner.Run(new ExecContainedEventSplitExpr());
        }
    }
} // end of namespace
