///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.suite.rowrecog;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.regressionrun.runner;
using com.espertech.esper.regressionrun.suite.core;

using NUnit.Framework;

namespace com.espertech.esper.regressionrun.suite.rowrecog
{
    [TestFixture]
    public class TestSuiteRowRecogWConfig : AbstractTestContainer
    {
        [Test, RunInApplicationDomain]
        public void TestRowRecogIntervalMicrosecondResolution()
        {
            using var session = RegressionRunner.Session(Container);
            session.Configuration.Common.AddEventType(typeof(SupportBean));
            session.Configuration.Common.TimeSource.TimeUnit = TimeUnit.MICROSECONDS;
            RegressionRunner.Run(session, new RowRecogIntervalResolution(10000000));
        }

        [Test, RunInApplicationDomain]
        public void TestRowRecogMaxStatesEngineWideNoPreventStart()
        {
            using var session = RegressionRunner.Session(Container, true);
            Configure(session.Configuration);
            session.Configuration.Runtime.MatchRecognize.MaxStates = 3L;
            session.Configuration.Runtime.MatchRecognize.IsMaxStatesPreventStart = false;
            RegressionRunner.Run(session, new RowRecogMaxStatesEngineWideNoPreventStart());
        }

        [Test, RunInApplicationDomain]
        public void TestRowRecogMaxStatesEngineWide3Instance()
        {
            using var session = RegressionRunner.Session(Container);
            Configure(session.Configuration);
            session.Configuration.Runtime.MatchRecognize.MaxStates = 3L;
            session.Configuration.Runtime.MatchRecognize.IsMaxStatesPreventStart = true;
            RegressionRunner.Run(session, new RowRecogMaxStatesEngineWide3Instance());
        }

        [Test, RunInApplicationDomain]
        public void TestRowRecogMaxStatesEngineWide4Instance()
        {
            using var session = RegressionRunner.Session(Container);
            Configure(session.Configuration);
            session.Configuration.Runtime.MatchRecognize.MaxStates = 4L;
            session.Configuration.Runtime.MatchRecognize.IsMaxStatesPreventStart = true;
            RegressionRunner.Run(session, new RowRecogMaxStatesEngineWide4Instance());
        }

        private void Configure(Configuration configuration)
        {
            configuration.Common.AddEventType(typeof(SupportBean));
            configuration.Common.AddEventType(typeof(SupportBean_S0));
            configuration.Common.AddEventType(typeof(SupportBean_S1));
            configuration.Runtime.ConditionHandling.AddClass(typeof(SupportConditionHandlerFactory));
        }
    }
} // end of namespace