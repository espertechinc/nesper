///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.suite.epl.script;
using com.espertech.esper.regressionrun.runner;
using com.espertech.esper.regressionrun.suite.core;

using NUnit.Framework;

namespace com.espertech.esper.regressionrun.suite.epl
{
    [TestFixture]
    public class TestSuiteEPLScriptWConfig : AbstractTestContainer
    {
        [Test, RunInApplicationDomain]
        public void TestEPLScriptExpressionConfiguration()
        {
            using (var session = RegressionRunner.Session(Container)) {
                session.Configuration.Common.AddEventType(typeof(SupportBean));
                session.Configuration.Compiler.Scripts.DefaultDialect = "dummy";
                RegressionRunner.Run(session, new EPLScriptExpressionConfiguration());
            }
        }
        
        [Test, RunInApplicationDomain]
        public void testEPLScriptExpressionDisable()
        {
            using (var session = RegressionRunner.Session(Container)) {
                session.Configuration.Common.AddEventType<SupportBean>();
                session.Configuration.Compiler.Scripts.IsEnabled = false;
                RegressionRunner.Run(session, new EPLScriptExpressionDisable());
            }
        }
    }
} // end of namespace