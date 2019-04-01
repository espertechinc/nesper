///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.regression.support;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

namespace com.espertech.esper.regression.pattern
{
    public class ExecPatternSuperAndInterfaces : RegressionExecution
    {
        private static readonly string INTERFACE_A = typeof(ISupportA).FullName;
        private static readonly string INTERFACE_B = typeof(ISupportB).FullName;
        private static readonly string INTERFACE_C = typeof(ISupportC).FullName;
        private static readonly string INTERFACE_D = typeof(ISupportD).FullName;
        private static readonly string INTERFACE_BASE_D = typeof(ISupportBaseD).FullName;
        private static readonly string INTERFACE_BASE_D_BASE = typeof(ISupportBaseDBase).FullName;
        private static readonly string INTERFACE_BASE_AB = typeof(ISupportBaseAB).FullName;
        private static readonly string SUPER_G = typeof(ISupportAImplSuperG).FullName;
        private static readonly string SUPER_G_IMPL = typeof(ISupportAImplSuperGImplPlus).FullName;
        private static readonly string OVERRIDE_BASE = typeof(SupportOverrideBase).FullName;
        private static readonly string OVERRIDE_ONE = typeof(SupportOverrideOne).FullName;
        private static readonly string OVERRIDE_ONEA = typeof(SupportOverrideOneA).FullName;
        private static readonly string OVERRIDE_ONEB = typeof(SupportOverrideOneB).FullName;
    
        public override void Run(EPServiceProvider epService)
        {
            EventCollection events = EventCollectionFactory.GetSetFiveInterfaces();
            var testCaseList = new CaseList();
            EventExpressionCase testCase = null;
    
            testCase = new EventExpressionCase("c=" + INTERFACE_C);
            testCase.Add("e1", "c", events.GetEvent("e1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("baseab=" + INTERFACE_BASE_AB);
            testCase.Add("e2", "baseab", events.GetEvent("e2"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every a=" + INTERFACE_A);
            testCase.Add("e2", "a", events.GetEvent("e2"));
            testCase.Add("e3", "a", events.GetEvent("e3"));
            testCase.Add("e12", "a", events.GetEvent("e12"));
            testCase.Add("e13", "a", events.GetEvent("e13"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every a=" + INTERFACE_B);
            testCase.Add("e2", "a", events.GetEvent("e2"));
            testCase.Add("e4", "a", events.GetEvent("e4"));
            testCase.Add("e6", "a", events.GetEvent("e6"));
            testCase.Add("e12", "a", events.GetEvent("e12"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every a=" + INTERFACE_B + "(b='B1')");
            testCase.Add("e2", "a", events.GetEvent("e2"));
            testCase.Add("e4", "a", events.GetEvent("e4"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every a=" + INTERFACE_A + "(a='A3')");
            testCase.Add("e12", "a", events.GetEvent("e12"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every a=" + INTERFACE_C + "(c='C2')");
            testCase.Add("e6", "a", events.GetEvent("e6"));
            testCase.Add("e12", "a", events.GetEvent("e12"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every a=" + INTERFACE_C + "(c='C1')");
            testCase.Add("e1", "a", events.GetEvent("e1"));
            testCase.Add("e2", "a", events.GetEvent("e2"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every a=" + INTERFACE_D + "(d='D1')");
            testCase.Add("e5", "a", events.GetEvent("e5"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every a=" + INTERFACE_BASE_D + "(baseD='BaseD')");
            testCase.Add("e5", "a", events.GetEvent("e5"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every a=" + INTERFACE_BASE_D_BASE + "(baseDBase='BaseDBase')");
            testCase.Add("e5", "a", events.GetEvent("e5"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every a=" + INTERFACE_D + "(d='D1', baseD='BaseD', baseDBase='BaseDBase')");
            testCase.Add("e5", "a", events.GetEvent("e5"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every a=" + INTERFACE_BASE_D + "(baseD='BaseD', baseDBase='BaseDBase')");
            testCase.Add("e5", "a", events.GetEvent("e5"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every a=" + SUPER_G);
            testCase.Add("e12", "a", events.GetEvent("e12"));
            testCase.Add("e13", "a", events.GetEvent("e13"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every a=" + SUPER_G + "(g='G1')");
            testCase.Add("e12", "a", events.GetEvent("e12"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every a=" + SUPER_G + "(baseAB='BaseAB5')");
            testCase.Add("e13", "a", events.GetEvent("e13"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every a=" + SUPER_G + "(baseAB='BaseAB4', g='G1', a='A3')");
            testCase.Add("e12", "a", events.GetEvent("e12"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every a=" + SUPER_G_IMPL + "(baseAB='BaseAB4', g='G1', a='A3', b='B4', c='C2')");
            testCase.Add("e12", "a", events.GetEvent("e12"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every a=" + OVERRIDE_BASE);
            testCase.Add("e8", "a", events.GetEvent("e8"));
            testCase.Add("e9", "a", events.GetEvent("e9"));
            testCase.Add("e10", "a", events.GetEvent("e10"));
            testCase.Add("e11", "a", events.GetEvent("e11"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every a=" + OVERRIDE_ONE);
            testCase.Add("e8", "a", events.GetEvent("e8"));
            testCase.Add("e9", "a", events.GetEvent("e9"));
            testCase.Add("e10", "a", events.GetEvent("e10"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every a=" + OVERRIDE_ONEA);
            testCase.Add("e8", "a", events.GetEvent("e8"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every a=" + OVERRIDE_ONEB);
            testCase.Add("e9", "a", events.GetEvent("e9"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every a=" + OVERRIDE_BASE + "(val='OB1')");
            testCase.Add("e9", "a", events.GetEvent("e9"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every a=" + OVERRIDE_BASE + "(val='O3')");
            testCase.Add("e10", "a", events.GetEvent("e10"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every a=" + OVERRIDE_BASE + "(val='OBase')");
            testCase.Add("e11", "a", events.GetEvent("e11"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every a=" + OVERRIDE_BASE + "(val='O2')");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every a=" + OVERRIDE_ONE + "(val='OA1')");
            testCase.Add("e8", "a", events.GetEvent("e8"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every a=" + OVERRIDE_ONE + "(val='O3')");
            testCase.Add("e10", "a", events.GetEvent("e10"));
            testCaseList.AddTest(testCase);
    
            var util = new PatternTestHarness(events, testCaseList, this.GetType());
            util.RunTest(epService);
        }
    }
    
    
} // end of namespace
