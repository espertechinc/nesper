///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using NUnit.Framework;
using com.espertech.esper.supportregression.execution;

namespace com.espertech.esper.regression.pattern
{
    [TestFixture]
    public class TestSuitePattern
    {
        [Test]
        public void TestExecPatternOperatorAnd() {
            RegressionRunner.Run(new ExecPatternOperatorAnd());
        }
    
        [Test]
        public void TestExecPatternOperatorNot() {
            RegressionRunner.Run(new ExecPatternOperatorNot());
        }
    
        [Test]
        public void TestExecPatternOperatorOr() {
            RegressionRunner.Run(new ExecPatternOperatorOr());
        }
    
        [Test]
        public void TestExecPatternOperatorOperatorMix() {
            RegressionRunner.Run(new ExecPatternOperatorOperatorMix());
        }
    
        [Test]
        public void TestExecPatternOperatorFollowedBy() {
            RegressionRunner.Run(new ExecPatternOperatorFollowedBy());
        }
    
        [Test]
        public void TestExecPatternOperatorFollowedByMax() {
            RegressionRunner.Run(new ExecPatternOperatorFollowedByMax());
        }
    
        [Test]
        public void TestExecPatternOperatorEvery() {
            RegressionRunner.Run(new ExecPatternOperatorEvery());
        }
    
        [Test]
        public void TestExecPatternOperatorEveryDistinct() {
            RegressionRunner.Run(new ExecPatternOperatorEveryDistinct());
        }
    
        [Test]
        public void TestExecPatternOperatorMatchUntilExpr() {
            RegressionRunner.Run(new ExecPatternOperatorMatchUntilExpr());
        }
    
        [Test]
        public void TestExecPatternOperatorFollowedByMax4Prevent() {
            RegressionRunner.Run(new ExecPatternOperatorFollowedByMax4Prevent());
        }
    
        [Test]
        public void TestExecPatternOperatorFollowedByMax2Prevent() {
            RegressionRunner.Run(new ExecPatternOperatorFollowedByMax2Prevent());
        }
    
        [Test]
        public void TestExecPatternOperatorFollowedByMax2Noprevent() {
            RegressionRunner.Run(new ExecPatternOperatorFollowedByMax2Noprevent());
        }
    
        [Test]
        public void TestExecPatternComplexPropertyAccess() {
            RegressionRunner.Run(new ExecPatternComplexPropertyAccess());
        }
    
        [Test]
        public void TestExecPatternCompositeSelect() {
            RegressionRunner.Run(new ExecPatternCompositeSelect());
        }
    
        [Test]
        public void TestExecPatternConsumingFilter() {
            RegressionRunner.Run(new ExecPatternConsumingFilter());
        }
    
        [Test]
        public void TestExecPatternConsumingPattern() {
            RegressionRunner.Run(new ExecPatternConsumingPattern());
        }
    
        [Test]
        public void TestExecPatternCronParameter() {
            RegressionRunner.Run(new ExecPatternCronParameter());
        }
    
        [Test]
        public void TestExecPatternDeadPattern() {
            RegressionRunner.Run(new ExecPatternDeadPattern());
        }
    
        [Test]
        public void TestExecPatternInvalid() {
            RegressionRunner.Run(new ExecPatternInvalid());
        }
    
        [Test]
        public void TestExecPatternExpressionText() {
            RegressionRunner.Run(new ExecPatternExpressionText());
        }
    
        [Test]
        public void TestExecPatternMicrosecondResolution() {
            RegressionRunner.Run(new ExecPatternMicrosecondResolution());
        }
    
        [Test]
        public void TestExecPatternStartLoop() {
            RegressionRunner.Run(new ExecPatternStartLoop());
        }
    
        [Test]
        public void TestExecPatternStartStop() {
            RegressionRunner.Run(new ExecPatternStartStop());
        }
    
        [Test]
        public void TestExecPatternRepeatRouteEvent() {
            RegressionRunner.Run(new ExecPatternRepeatRouteEvent());
        }
    
        [Test]
        public void TestExecPatternSuperAndInterfaces() {
            RegressionRunner.Run(new ExecPatternSuperAndInterfaces());
        }
    
        [Test]
        public void TestExecPatternObserverTimerAt() {
            RegressionRunner.Run(new ExecPatternObserverTimerAt());
        }
    
        [Test]
        public void TestExecPatternObserverTimerInterval() {
            RegressionRunner.Run(new ExecPatternObserverTimerInterval());
        }
    
        [Test]
        public void TestExecPatternObserverTimerSchedule() {
            RegressionRunner.Run(new ExecPatternObserverTimerSchedule());
        }
    
        [Test]
        public void TestExecPatternObserverTimerScheduleTimeZoneEST() {
            RegressionRunner.Run(new ExecPatternObserverTimerScheduleTimeZoneEST());
        }
    
        [Test]
        public void TestExecPatternGuardTimerWithin() {
            RegressionRunner.Run(new ExecPatternGuardTimerWithin());
        }
    
        [Test]
        public void TestExecPatternGuardTimerWithinOrMax() {
            RegressionRunner.Run(new ExecPatternGuardTimerWithinOrMax());
        }
    
        [Test]
        public void TestExecPatternGuardWhile() {
            RegressionRunner.Run(new ExecPatternGuardWhile());
        }
    
        [Test]
        public void TestExecPatternUseResult() {
            RegressionRunner.Run(new ExecPatternUseResult());
        }
    
    }
} // end of namespace
