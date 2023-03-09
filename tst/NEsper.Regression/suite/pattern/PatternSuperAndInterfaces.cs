///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.patternassert;

namespace com.espertech.esper.regressionlib.suite.pattern
{
    public class PatternSuperAndInterfaces : RegressionExecution
    {
        private const string INTERFACE_A = nameof(ISupportA);
        private const string INTERFACE_B = nameof(ISupportB);
        private const string INTERFACE_C = nameof(ISupportC);
        private const string INTERFACE_D = nameof(ISupportD);
        private const string INTERFACE_BASE_D = nameof(ISupportBaseD);
        private const string INTERFACE_BASE_D_BASE = nameof(ISupportBaseDBase);
        private const string INTERFACE_BASE_AB = nameof(ISupportBaseAB);
        private const string SUPER_G = nameof(ISupportAImplSuperG);
        private const string SUPER_G_IMPL = nameof(ISupportAImplSuperGImplPlus);
        private const string OVERRIDE_BASE = nameof(SupportOverrideBase);
        private const string OVERRIDE_ONE = nameof(SupportOverrideOne);
        private const string OVERRIDE_ONEA = nameof(SupportOverrideOneA);
        private const string OVERRIDE_ONEB = nameof(SupportOverrideOneB);

        public void Run(RegressionEnvironment env)
        {
            var events = EventCollectionFactory.GetSetFiveInterfaces();
            EventExpressionCase testCase = null;

            testCase = new EventExpressionCase("C=" + INTERFACE_C);
            testCase.Add("e1", "C", events.GetEvent("e1"));

            (new PatternTestHarness(events, testCase, GetType())).RunTest(env);

            testCase = new EventExpressionCase("baseab=" + INTERFACE_BASE_AB);
            testCase.Add("e2", "baseab", events.GetEvent("e2"));

            (new PatternTestHarness(events, testCase, GetType())).RunTest(env);

            testCase = new EventExpressionCase("every a=" + INTERFACE_A);
            testCase.Add("e2", "a", events.GetEvent("e2"));
            testCase.Add("e3", "a", events.GetEvent("e3"));
            testCase.Add("e12", "a", events.GetEvent("e12"));
            testCase.Add("e13", "a", events.GetEvent("e13"));

            (new PatternTestHarness(events, testCase, GetType())).RunTest(env);

            testCase = new EventExpressionCase("every a=" + INTERFACE_B);
            testCase.Add("e2", "a", events.GetEvent("e2"));
            testCase.Add("e4", "a", events.GetEvent("e4"));
            testCase.Add("e6", "a", events.GetEvent("e6"));
            testCase.Add("e12", "a", events.GetEvent("e12"));

            (new PatternTestHarness(events, testCase, GetType())).RunTest(env);

            testCase = new EventExpressionCase("every a=" + INTERFACE_B + "(B='B1')");
            testCase.Add("e2", "a", events.GetEvent("e2"));
            testCase.Add("e4", "a", events.GetEvent("e4"));

            (new PatternTestHarness(events, testCase, GetType())).RunTest(env);

            testCase = new EventExpressionCase("every a=" + INTERFACE_A + "(A='A3')");
            testCase.Add("e12", "a", events.GetEvent("e12"));

            (new PatternTestHarness(events, testCase, GetType())).RunTest(env);

            testCase = new EventExpressionCase("every a=" + INTERFACE_C + "(C='C2')");
            testCase.Add("e6", "a", events.GetEvent("e6"));
            testCase.Add("e12", "a", events.GetEvent("e12"));

            (new PatternTestHarness(events, testCase, GetType())).RunTest(env);

            testCase = new EventExpressionCase("every a=" + INTERFACE_C + "(C='C1')");
            testCase.Add("e1", "a", events.GetEvent("e1"));
            testCase.Add("e2", "a", events.GetEvent("e2"));

            (new PatternTestHarness(events, testCase, GetType())).RunTest(env);

            testCase = new EventExpressionCase("every a=" + INTERFACE_D + "(D='D1')");
            testCase.Add("e5", "a", events.GetEvent("e5"));

            (new PatternTestHarness(events, testCase, GetType())).RunTest(env);

            testCase = new EventExpressionCase("every a=" + INTERFACE_BASE_D + "(BaseD='BaseD')");
            testCase.Add("e5", "a", events.GetEvent("e5"));

            (new PatternTestHarness(events, testCase, GetType())).RunTest(env);

            testCase = new EventExpressionCase("every a=" + INTERFACE_BASE_D_BASE + "(BaseDBase='BaseDBase')");
            testCase.Add("e5", "a", events.GetEvent("e5"));

            (new PatternTestHarness(events, testCase, GetType())).RunTest(env);

            testCase = new EventExpressionCase(
                "every a=" + INTERFACE_D + "(D='D1', BaseD='BaseD', BaseDBase='BaseDBase')");
            testCase.Add("e5", "a", events.GetEvent("e5"));

            (new PatternTestHarness(events, testCase, GetType())).RunTest(env);

            testCase = new EventExpressionCase(
                "every a=" + INTERFACE_BASE_D + "(BaseD='BaseD', BaseDBase='BaseDBase')");
            testCase.Add("e5", "a", events.GetEvent("e5"));

            (new PatternTestHarness(events, testCase, GetType())).RunTest(env);

            testCase = new EventExpressionCase("every a=" + SUPER_G);
            testCase.Add("e12", "a", events.GetEvent("e12"));
            testCase.Add("e13", "a", events.GetEvent("e13"));

            (new PatternTestHarness(events, testCase, GetType())).RunTest(env);

            testCase = new EventExpressionCase("every a=" + SUPER_G + "(G='G1')");
            testCase.Add("e12", "a", events.GetEvent("e12"));

            (new PatternTestHarness(events, testCase, GetType())).RunTest(env);

            testCase = new EventExpressionCase("every a=" + SUPER_G + "(BaseAB='BaseAB5')");
            testCase.Add("e13", "a", events.GetEvent("e13"));

            (new PatternTestHarness(events, testCase, GetType())).RunTest(env);

            testCase = new EventExpressionCase("every a=" + SUPER_G + "(BaseAB='BaseAB4', G='G1', A='A3')");
            testCase.Add("e12", "a", events.GetEvent("e12"));

            (new PatternTestHarness(events, testCase, GetType())).RunTest(env);

            testCase = new EventExpressionCase(
                "every a=" + SUPER_G_IMPL + "(BaseAB='BaseAB4', G='G1', A='A3', B='B4', C='C2')");
            testCase.Add("e12", "a", events.GetEvent("e12"));

            (new PatternTestHarness(events, testCase, GetType())).RunTest(env);

            testCase = new EventExpressionCase("every a=" + OVERRIDE_BASE);
            testCase.Add("e8", "a", events.GetEvent("e8"));
            testCase.Add("e9", "a", events.GetEvent("e9"));
            testCase.Add("e10", "a", events.GetEvent("e10"));
            testCase.Add("e11", "a", events.GetEvent("e11"));

            (new PatternTestHarness(events, testCase, GetType())).RunTest(env);

            testCase = new EventExpressionCase("every a=" + OVERRIDE_ONE);
            testCase.Add("e8", "a", events.GetEvent("e8"));
            testCase.Add("e9", "a", events.GetEvent("e9"));
            testCase.Add("e10", "a", events.GetEvent("e10"));

            (new PatternTestHarness(events, testCase, GetType())).RunTest(env);

            testCase = new EventExpressionCase("every a=" + OVERRIDE_ONEA);
            testCase.Add("e8", "a", events.GetEvent("e8"));

            (new PatternTestHarness(events, testCase, GetType())).RunTest(env);

            testCase = new EventExpressionCase("every a=" + OVERRIDE_ONEB);
            testCase.Add("e9", "a", events.GetEvent("e9"));

            (new PatternTestHarness(events, testCase, GetType())).RunTest(env);

            testCase = new EventExpressionCase("every a=" + OVERRIDE_BASE + "(Val='OB1')");
            testCase.Add("e9", "a", events.GetEvent("e9"));

            (new PatternTestHarness(events, testCase, GetType())).RunTest(env);

            testCase = new EventExpressionCase("every a=" + OVERRIDE_BASE + "(Val='O3')");
            testCase.Add("e10", "a", events.GetEvent("e10"));

            (new PatternTestHarness(events, testCase, GetType())).RunTest(env);

            testCase = new EventExpressionCase("every a=" + OVERRIDE_BASE + "(Val='OBase')");
            testCase.Add("e11", "a", events.GetEvent("e11"));

            (new PatternTestHarness(events, testCase, GetType())).RunTest(env);

            testCase = new EventExpressionCase("every a=" + OVERRIDE_BASE + "(Val='O2')");

            (new PatternTestHarness(events, testCase, GetType())).RunTest(env);

            testCase = new EventExpressionCase("every a=" + OVERRIDE_ONE + "(Val='OA1')");
            testCase.Add("e8", "a", events.GetEvent("e8"));

            (new PatternTestHarness(events, testCase, GetType())).RunTest(env);

            testCase = new EventExpressionCase("every a=" + OVERRIDE_ONE + "(Val='O3')");
            testCase.Add("e10", "a", events.GetEvent("e10"));

            (new PatternTestHarness(events, testCase, GetType())).RunTest(env);
        }
    }
} // end of namespace