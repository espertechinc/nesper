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

using NUnit.Framework;

namespace com.espertech.esper.regressionrun.suite.rowrecog
{
    [TestFixture]
    public class TestSuiteRowRecogWConfig
    {
        [Test]
        public void TestRowRecogIntervalMicrosecondResolution()
        {
            RegressionSession session = RegressionRunner.Session();
            session.Configuration.Common.AddEventType(typeof(SupportBean));
            session.Configuration.Common.TimeSource.TimeUnit = TimeUnit.MICROSECONDS;
            RegressionRunner.Run(session, new RowRecogIntervalResolution(10000000));
            session.Destroy();
        }

        [Test]
        public void TestRowRecogMaxStatesEngineWideNoPreventStart()
        {
            RegressionSession session = RegressionRunner.Session();
            Configure(session.Configuration);
            session.Configuration.Runtime.MatchRecognize.MaxStates = 3L;
            session.Configuration.Runtime.MatchRecognize.IsMaxStatesPreventStart = false;
            RegressionRunner.Run(session, new RowRecogMaxStatesEngineWideNoPreventStart());
            session.Destroy();
        }

        [Test]
        public void TestRowRecogMaxStatesEngineWide3Instance()
        {
            RegressionSession session = RegressionRunner.Session();
            Configure(session.Configuration);
            session.Configuration.Runtime.MatchRecognize.MaxStates = 3L;
            session.Configuration.Runtime.MatchRecognize.IsMaxStatesPreventStart = true;
            RegressionRunner.Run(session, new RowRecogMaxStatesEngineWide3Instance());
            session.Destroy();
        }

        [Test]
        public void TestRowRecogMaxStatesEngineWide4Instance()
        {
            RegressionSession session = RegressionRunner.Session();
            Configure(session.Configuration);
            session.Configuration.Runtime.MatchRecognize.MaxStates = 4L;
            session.Configuration.Runtime.MatchRecognize.IsMaxStatesPreventStart = true;
            RegressionRunner.Run(session, new RowRecogMaxStatesEngineWide4Instance());
            session.Destroy();
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