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

namespace com.espertech.esper.regression.events.render
{
    [TestFixture]
    public class TestSuiteEventRender
    {
        [Test]
        public void TestExecEventRender() {
            RegressionRunner.Run(new ExecEventRender());
        }
    
        [Test]
        public void TestExecEventRenderJSON() {
            RegressionRunner.Run(new ExecEventRenderJSON());
        }
    
        [Test]
        public void TestExecEventRenderXML() {
            RegressionRunner.Run(new ExecEventRenderXML());
        }
    }
} // end of namespace
