///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.execution;

using static com.espertech.esper.regression.pattern.ExecPatternOperatorFollowedByMax4Prevent;

using NUnit.Framework;

namespace com.espertech.esper.regression.pattern
{
    public class ExecPatternOperatorFollowedByMax2Prevent : RegressionExecution
    {
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType("SupportBean_A", typeof(SupportBean_A));
            configuration.AddEventType("SupportBean_B", typeof(SupportBean_B));
            configuration.EngineDefaults.ConditionHandling.AddClass(typeof(SupportConditionHandlerFactory));
            configuration.EngineDefaults.Patterns.MaxSubexpressions = 2L;
            configuration.EngineDefaults.Patterns.IsMaxSubexpressionPreventStart = true;
            configuration.EngineDefaults.Logging.IsEnableExecutionDebug = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            if (SupportConfigFactory.SkipTest(typeof(ExecPatternOperatorFollowedByMax2Prevent))) {
                return;
            }
            SupportConditionHandlerFactory.SupportConditionHandler handler = SupportConditionHandlerFactory.LastHandler;
    
            string expression = "@Name('A') select a.id as a, b.id as b from pattern [every a=SupportBean_A -> b=SupportBean_B]";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(expression);
    
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_A("A1"));
            epService.EPRuntime.SendEvent(new SupportBean_A("A2"));
    
            handler.Contexts.Clear();
            epService.EPRuntime.SendEvent(new SupportBean_A("A3"));
            AssertContextEnginePool(epService, stmt, handler.Contexts, 2, GetExpectedCountMap("A", 2));
    
            var fields = new string[]{"a", "b"};
            epService.EPRuntime.SendEvent(new SupportBean_B("B1"));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"A1", "B1"}, new object[] {"A2", "B1"}});
    
            epService.EPRuntime.SendEvent(new SupportBean_A("A4"));
            epService.EPRuntime.SendEvent(new SupportBean_B("B2"));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"A4", "B2"}});
            Assert.IsTrue(handler.Contexts.IsEmpty());
    
            for (int i = 5; i < 9; i++) {
                epService.EPRuntime.SendEvent(new SupportBean_A("A" + i));
                if (i >= 7) {
                    AssertContextEnginePool(epService, stmt, handler.Contexts, 2, GetExpectedCountMap("A", 2));
                }
            }
    
            epService.EPRuntime.SendEvent(new SupportBean_B("B3"));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"A5", "B3"}, new object[] {"A6", "B3"}});
    
            epService.EPRuntime.SendEvent(new SupportBean_B("B4"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_A("A20"));
            epService.EPRuntime.SendEvent(new SupportBean_A("A21"));
            epService.EPRuntime.SendEvent(new SupportBean_B("B5"));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"A20", "B5"}, new object[] {"A21", "B5"}});
            Assert.IsTrue(handler.Contexts.IsEmpty());
    
            stmt.Dispose();
        }
    }
} // end of namespace
