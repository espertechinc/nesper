///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.suite.epl.script;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.script;
using com.espertech.esper.regressionrun.Runner;
using com.espertech.esper.regressionrun.suite.core;

using NUnit.Framework;

namespace com.espertech.esper.regressionrun.suite.epl
{
    [TestFixture]
    public class TestSuiteEPLScript
    {
        [SetUp]
        public void SetUp()
        {
            session = RegressionRunner.Session();
            Configure(session.Configuration);
        }

        [TearDown]
        public void TearDown()
        {
            session.Destroy();
            session = null;
        }

        private RegressionSession session;

        private static void Configure(Configuration configuration)
        {
            foreach (var clazz in new[] {
                typeof(SupportBean),
                typeof(SupportBean_S0),
                typeof(SupportColorEvent),
                typeof(SupportRFIDSimpleEvent)
            }) {
                configuration.Common.AddEventType(clazz);
            }

            configuration.Common.AddImportType(typeof(MyImportedClass));
        }

        /// <summary>
        /// Auto-test(s): EPLScriptExpression
        /// <code>
        /// RegressionRunner.Run(_session, EPLScriptExpression.Executions());
        /// </code>
        /// </summary>

        public class TestEPLScriptExpression : AbstractTestBase
        {
            public TestEPLScriptExpression() : base(Configure) { }

            [Test, RunInApplicationDomain]
            public void WithReturnNullWhenNumeric() => RegressionRunner.Run(_session, EPLScriptExpression.WithReturnNullWhenNumeric());

            [Test, RunInApplicationDomain]
            public void WithSubqueryParam() => RegressionRunner.Run(_session, EPLScriptExpression.WithSubqueryParam());

            [Test, RunInApplicationDomain]
            public void WithJavaScriptStatelessReturnPassArgs() => RegressionRunner.Run(_session, EPLScriptExpression.WithJavaScriptStatelessReturnPassArgs());

            [Test, RunInApplicationDomain]
            public void WithParserMVELSelectNoArgConstant() => RegressionRunner.Run(_session, EPLScriptExpression.WithParserMVELSelectNoArgConstant());

            [Test, RunInApplicationDomain]
            public void WithInvalidScriptJS() => RegressionRunner.Run(_session, EPLScriptExpression.WithInvalidScriptJS());

            [Test, RunInApplicationDomain]
            public void WithInvalidRegardlessDialect() => RegressionRunner.Run(_session, EPLScriptExpression.WithInvalidRegardlessDialect());

            [Test, RunInApplicationDomain]
            public void WithDocSamples() => RegressionRunner.Run(_session, EPLScriptExpression.WithDocSamples());

            [Test, RunInApplicationDomain]
            public void WithScriptReturningEvents() => RegressionRunner.Run(_session, EPLScriptExpression.WithScriptReturningEvents());

            [Test, RunInApplicationDomain]
            public void WithQuoteEscape() => RegressionRunner.Run(_session, EPLScriptExpression.WithQuoteEscape());

            [Test, RunInApplicationDomain]
            public void WithScripts() => RegressionRunner.Run(_session, EPLScriptExpression.WithScripts());  
        }
    }
} // end of namespace