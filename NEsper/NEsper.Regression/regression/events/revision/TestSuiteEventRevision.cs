///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using NUnit.Framework;
using com.espertech.esper.supportregression.execution;

namespace com.espertech.esper.regression.events.revision
{
    [TestFixture]
    public class TestSuiteEventRevision
    {
        [Test]
        public void TestExecEventRevisionDeclared() {
            RegressionRunner.Run(new ExecEventRevisionDeclared());
        }
    
        [Test]
        public void TestExecEventRevisionMerge() {
            RegressionRunner.Run(new ExecEventRevisionMerge());
        }
    
        [Test]
        public void TestExecEventRevisionWindowed() {
            RegressionRunner.Run(new ExecEventRevisionWindowed());
        }
    
        [Test]
        public void TestExecEventRevisionWindowedTime() {
            RegressionRunner.Run(new ExecEventRevisionWindowedTime());
        }
    }
} // end of namespace
