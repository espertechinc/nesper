///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

using com.espertech.esper.regression.support;
using com.espertech.esper.supportregression.bean;

using NUnit.Framework;

namespace com.espertech.esper.regression.pattern
{
    [TestFixture]
    public class TestSuperAndInterfaces 
    {
        private static String INTERFACE_A = typeof(ISupportA).FullName;
        private static String INTERFACE_B = typeof(ISupportB).FullName;
        private static String INTERFACE_C = typeof(ISupportC).FullName;
        private static String INTERFACE_D = typeof(ISupportD).FullName;
        private static String INTERFACE_BASE_D = typeof(ISupportBaseD).FullName;
        private static String INTERFACE_BASE_D_BASE = typeof(ISupportBaseDBase).FullName;
        private static String INTERFACE_BASE_AB = typeof(ISupportBaseAB).FullName;
        private static String SUPER_G = typeof(ISupportAImplSuperG).FullName;
        private static String SUPER_G_IMPL = typeof(ISupportAImplSuperGImplPlus).FullName;
        private static String OVERRIDE_BASE = typeof(SupportOverrideBase).FullName;
        private static String OVERRIDE_ONE = typeof(SupportOverrideOne).FullName;
        private static String OVERRIDE_ONEA = typeof(SupportOverrideOneA).FullName;
        private static String OVERRIDE_ONEB = typeof(SupportOverrideOneB).FullName;
    
        [Test]
        public void TestInterfacedEvents()
        {
            EventCollection events = EventCollectionFactory.GetSetFiveInterfaces();
            CaseList testCaseList = new CaseList();
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
    
            testCase = new EventExpressionCase("every a=" + INTERFACE_B + "(B='B1')");
            testCase.Add("e2", "a", events.GetEvent("e2"));
            testCase.Add("e4", "a", events.GetEvent("e4"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every a=" + INTERFACE_A + "(A='A3')");
            testCase.Add("e12", "a", events.GetEvent("e12"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every a=" + INTERFACE_C + "(C='C2')");
            testCase.Add("e6", "a", events.GetEvent("e6"));
            testCase.Add("e12", "a", events.GetEvent("e12"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every a=" + INTERFACE_C + "(C='C1')");
            testCase.Add("e1", "a", events.GetEvent("e1"));
            testCase.Add("e2", "a", events.GetEvent("e2"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every a=" + INTERFACE_D + "(D='D1')");
            testCase.Add("e5", "a", events.GetEvent("e5"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every a=" + INTERFACE_BASE_D + "(BaseD='BaseD')");
            testCase.Add("e5", "a", events.GetEvent("e5"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every a=" + INTERFACE_BASE_D_BASE + "(BaseDBase='BaseDBase')");
            testCase.Add("e5", "a", events.GetEvent("e5"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every a=" + INTERFACE_D + "(D='D1', BaseD='BaseD', BaseDBase='BaseDBase')");
            testCase.Add("e5", "a", events.GetEvent("e5"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every a=" + INTERFACE_BASE_D + "(BaseD='BaseD', BaseDBase='BaseDBase')");
            testCase.Add("e5", "a", events.GetEvent("e5"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every a=" + SUPER_G);
            testCase.Add("e12", "a", events.GetEvent("e12"));
            testCase.Add("e13", "a", events.GetEvent("e13"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every a=" + SUPER_G + "(G='G1')");
            testCase.Add("e12", "a", events.GetEvent("e12"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every a=" + SUPER_G + "(BaseAB='BaseAB5')");
            testCase.Add("e13", "a", events.GetEvent("e13"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every a=" + SUPER_G + "(BaseAB='BaseAB4', G='G1', A='A3')");
            testCase.Add("e12", "a", events.GetEvent("e12"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every a=" + SUPER_G_IMPL + "(BaseAB='BaseAB4', G='G1', A='A3', B='B4', C='C2')");
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
    
            testCase = new EventExpressionCase("every a=" + OVERRIDE_BASE + "(Val='OB1')");
            testCase.Add("e9", "a", events.GetEvent("e9"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every a=" + OVERRIDE_BASE + "(Val='O3')");
            testCase.Add("e10", "a", events.GetEvent("e10"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every a=" + OVERRIDE_BASE + "(Val='OBase')");
            testCase.Add("e11", "a", events.GetEvent("e11"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every a=" + OVERRIDE_BASE + "(Val='O2')");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every a=" + OVERRIDE_ONE + "(Val='OA1')");
            testCase.Add("e8", "a", events.GetEvent("e8"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every a=" + OVERRIDE_ONE + "(Val='O3')");
            testCase.Add("e10", "a", events.GetEvent("e10"));
            testCaseList.AddTest(testCase);
    
            PatternTestHarness util = new PatternTestHarness(events, testCaseList, GetType(), GetType().FullName);
            util.RunTest();
        }
    }
    
    
}
