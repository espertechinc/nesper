///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.execution;

using static com.espertech.esper.regression.pattern.ExecPatternOperatorFollowedByMax4Prevent;

namespace com.espertech.esper.regression.pattern
{
    public class ExecPatternOperatorFollowedByMax2Noprevent : RegressionExecution
    {
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType("SupportBean_A", typeof(SupportBean_A));
            configuration.AddEventType("SupportBean_B", typeof(SupportBean_B));
            configuration.EngineDefaults.ConditionHandling.AddClass(typeof(SupportConditionHandlerFactory));
            configuration.EngineDefaults.Patterns.MaxSubexpressions = 2L;
            configuration.EngineDefaults.Patterns.IsMaxSubexpressionPreventStart = false;
            configuration.EngineDefaults.Logging.IsEnableExecutionDebug = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            if (SupportConfigFactory.SkipTest(typeof(ExecPatternOperatorFollowedByMax2Noprevent))) {
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
    
            handler.Contexts.Clear();
            epService.EPRuntime.SendEvent(new SupportBean_A("A4"));
            AssertContextEnginePool(epService, stmt, handler.Contexts, 2, GetExpectedCountMap("A", 3));
    
            var fields = new string[]{"a", "b"};
            epService.EPRuntime.SendEvent(new SupportBean_B("B1"));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"A1", "B1"}, new object[] {"A2", "B1"}, new object[] {"A3", "B1"}, new object[] {"A4", "B1"}});
    
            // set new max
            epService.EPAdministrator.Configuration.PatternMaxSubexpressions = 1L;
    
            epService.EPRuntime.SendEvent(new SupportBean_A("A5"));
    
            handler.Contexts.Clear();
            epService.EPRuntime.SendEvent(new SupportBean_A("A6"));
            AssertContextEnginePool(epService, stmt, handler.Contexts, 1, GetExpectedCountMap("A", 1));
    
            stmt.Dispose();
        }
    }
} // end of namespace
