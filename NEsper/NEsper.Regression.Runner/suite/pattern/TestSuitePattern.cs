///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.suite.pattern;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.regressionlib.support.epl;
using com.espertech.esper.regressionrun.runner;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;
using SupportBean_N = com.espertech.esper.regressionlib.support.bean.SupportBean_N;
using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;

namespace com.espertech.esper.regressionrun.suite.pattern
{
    [TestFixture]
    public class TestSuitePattern
    {
        private RegressionSession session;

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

        [Test]
        public void TestPatternOperatorAnd()
        {
            RegressionRunner.Run(session, PatternOperatorAnd.Executions());
        }

        [Test]
        public void TestPatternOperatorOr()
        {
            RegressionRunner.Run(session, PatternOperatorOr.Executions());
        }

        [Test]
        public void TestPatternOperatorNot()
        {
            RegressionRunner.Run(session, PatternOperatorNot.Executions());
        }

        [Test]
        public void TestPatternObserverTimerInterval()
        {
            RegressionRunner.Run(session, PatternObserverTimerInterval.Executions());
        }

        [Test]
        public void TestPatternGuardTimerWithin()
        {
            RegressionRunner.Run(session, PatternGuardTimerWithin.Executions());
        }

        [Test]
        public void TestPatternOperatorFollowedBy()
        {
            RegressionRunner.Run(session, PatternOperatorFollowedBy.Executions());
        }

        [Test]
        public void TestPatternOperatorEvery()
        {
            RegressionRunner.Run(session, PatternOperatorEvery.Executions());
        }

        [Test]
        public void TestPatternOperatorMatchUntil()
        {
            RegressionRunner.Run(session, PatternOperatorMatchUntil.Executions());
        }

        [Test]
        public void TestPatternOperatorEveryDistinct()
        {
            RegressionRunner.Run(session, PatternOperatorEveryDistinct.Executions());
        }

        [Test]
        public void TestPatternObserverTimerAt()
        {
            RegressionRunner.Run(session, PatternObserverTimerAt.Executions());
        }

        [Test]
        public void TestPatternObserverTimerSchedule()
        {
            RegressionRunner.Run(session, PatternObserverTimerSchedule.Executions());
        }

        [Test]
        public void TestPatternGuardWhile()
        {
            RegressionRunner.Run(session, PatternGuardWhile.Executions());
        }

        [Test]
        public void TestPatternGuardTimerWithinOrMax()
        {
            RegressionRunner.Run(session, new PatternGuardTimerWithinOrMax());
        }

        [Test]
        public void TestPatternUseResult()
        {
            RegressionRunner.Run(session, PatternUseResult.Executions());
        }

        [Test]
        public void TestPatternOperatorOperatorMix()
        {
            RegressionRunner.Run(session, new PatternOperatorOperatorMix());
        }

        [Test]
        public void TestPatternComplexPropertyAccess()
        {
            RegressionRunner.Run(session, PatternComplexPropertyAccess.Executions());
        }

        [Test]
        public void TestPatternOperatorFollowedByMax()
        {
            RegressionRunner.Run(session, PatternOperatorFollowedByMax.Executions());
        }

        [Test]
        public void TestPatternCompositeSelect()
        {
            RegressionRunner.Run(session, PatternCompositeSelect.Executions());
        }

        [Test]
        public void TestPatternConsumingFilter()
        {
            RegressionRunner.Run(session, PatternConsumingFilter.Executions());
        }

        [Test]
        public void TestPatternConsumingPattern()
        {
            RegressionRunner.Run(session, PatternConsumingPattern.Executions());
        }

        [Test]
        public void TestPatternDeadPattern()
        {
            RegressionRunner.Run(session, new PatternDeadPattern());
        }

        [Test]
        public void TestPatternInvalid()
        {
            RegressionRunner.Run(session, PatternInvalid.Executions());
        }

        [Test]
        public void TestPatternStartLoop()
        {
            RegressionRunner.Run(session, new PatternStartLoop());
        }

        [Test]
        public void TestPatternMicrosecondResolution()
        {
            RegressionRunner.Run(session, new PatternMicrosecondResolution(false));
        }

        [Test]
        public void TestPatternStartStop()
        {
            RegressionRunner.Run(session, PatternStartStop.Executions());
        }

        [Test]
        public void TestPatternSuperAndInterfaces()
        {
            RegressionRunner.Run(session, new PatternSuperAndInterfaces());
        }

        [Test]
        public void TestPatternRepeatRouteEvent()
        {
            RegressionRunner.Run(session, PatternRepeatRouteEvent.Executions());
        }

        [Test]
        public void TestPatternExpressionText()
        {
            RegressionRunner.Run(session, new PatternExpressionText());
        }

        private static void Configure(Configuration configuration)
        {
            foreach (Type clazz in new Type[]{
                typeof(SupportBean_A),
                typeof(SupportBean_B),
                typeof(SupportBean_C),
                typeof(SupportBean_D),
                typeof(SupportBean_E),
                typeof(SupportBean_F),
                typeof(SupportBean_G),
                typeof(SupportBean),
                typeof(SupportCallEvent),
                typeof(SupportRFIDEvent),
                typeof(SupportBean_N),
                typeof(SupportBean_S0),
                typeof(SupportIdEventA),
                typeof(SupportIdEventB),
                typeof(SupportIdEventC),
                typeof(SupportIdEventD),
                typeof(SupportMarketDataBean),
                typeof(SupportTradeEvent),
                typeof(SupportBeanComplexProps),
                typeof(SupportBeanCombinedProps),
                typeof(ISupportC),
                typeof(ISupportBaseAB),
                typeof(ISupportA),
                typeof(ISupportB),
                typeof(ISupportD),
                typeof(ISupportBaseD),
                typeof(ISupportBaseDBase),
                typeof(ISupportAImplSuperG),
                typeof(ISupportAImplSuperGImplPlus),
                typeof(SupportOverrideBase),
                typeof(SupportOverrideOne),
                typeof(SupportOverrideOneA),
                typeof(SupportOverrideOneB),
                typeof(ISupportCImpl),
                typeof(ISupportABCImpl),
                typeof(ISupportAImpl),
                typeof(ISupportBImpl),
                typeof(ISupportDImpl),
                typeof(ISupportBCImpl),
                typeof(ISupportBaseABImpl),
                typeof(ISupportAImplSuperGImpl)})
            {
                configuration.Common.AddEventType(clazz.Name, clazz);
            }

            foreach (string name in "computeISO8601String,getThe1980Calendar,getThe1980Date,getThe1980Long,getTheSeconds,getThe1980LocalDateTime,getThe1980ZonedDateTime".SplitCsv())
            {
                configuration.Compiler.AddPlugInSingleRowFunction(name, typeof(PatternObserverTimerSchedule), name);
            }

            ConfigurationCommon common = configuration.Common;
            common.AddVariable("lower", typeof(int), null);
            common.AddVariable("upper", typeof(int), null);
            common.AddVariable("VMIN", typeof(int), 0);
            common.AddVariable("VHOUR", typeof(int), 8);
            common.AddVariable("D", typeof(double), 1);
            common.AddVariable("H", typeof(double), 2);
            common.AddVariable("M", typeof(double), 3);
            common.AddVariable("S", typeof(double), 4);
            common.AddVariable("MS", typeof(double), 5);

            configuration.Runtime.ConditionHandling.AddClass(typeof(SupportConditionHandlerFactory));

            configuration.Common.AddImportType(typeof(SupportStaticMethodLib));

            configuration.Compiler.ByteCode.AttachEPL = true;
        }
    }
} // end of namespace