///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.hook;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.execution;

using static com.espertech.esper.supportregression.bean.SupportBeanConstants;

using NUnit.Framework;

namespace com.espertech.esper.regression.rowrecog
{
    public class ExecRowRecogMaxStatesEngineWideNoPreventStart : RegressionExecution
    {
        private SupportConditionHandlerFactory.SupportConditionHandler handler;
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType<SupportBean_S0>();
            configuration.AddEventType(typeof(SupportBean_S1));
            configuration.EngineDefaults.ConditionHandling.AddClass(typeof(SupportConditionHandlerFactory));
            configuration.EngineDefaults.MatchRecognize.MaxStates = 3L;
            configuration.EngineDefaults.MatchRecognize.IsMaxStatesPreventStart = false;
            configuration.EngineDefaults.Logging.IsEnableExecutionDebug = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            ConditionHandlerFactoryContext conditionHandlerFactoryContext = SupportConditionHandlerFactory.FactoryContexts[0];
            Assert.AreEqual(conditionHandlerFactoryContext.EngineURI, epService.URI);
            handler = SupportConditionHandlerFactory.LastHandler;
    
            string[] fields = "c0".Split(',');
    
            string epl = "@Name('S1') select * from SupportBean " +
                    "match_recognize (" +
                    "  partition by TheString " +
                    "  measures P1.TheString as c0" +
                    "  pattern (P1 P2) " +
                    "  define " +
                    "    P1 as P1.IntPrimitive = 1," +
                    "    P2 as P2.IntPrimitive = 2" +
                    ")";
    
            var listener = new SupportUpdateListener();
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("A", 1));
            epService.EPRuntime.SendEvent(new SupportBean("B", 1));
            epService.EPRuntime.SendEvent(new SupportBean("C", 1));
            Assert.IsTrue(handler.Contexts.IsEmpty());
    
            // overflow
            epService.EPRuntime.SendEvent(new SupportBean("D", 1));
            ExecRowRecogMaxStatesEngineWide3Instance.AssertContextEnginePool(epService, stmt, handler.GetAndResetContexts(), 3, ExecRowRecogMaxStatesEngineWide3Instance.GetExpectedCountMap("S1", 3));
            epService.EPRuntime.SendEvent(new SupportBean("E", 1));
            ExecRowRecogMaxStatesEngineWide3Instance.AssertContextEnginePool(epService, stmt, handler.GetAndResetContexts(), 3, ExecRowRecogMaxStatesEngineWide3Instance.GetExpectedCountMap("S1", 4));
    
            epService.EPRuntime.SendEvent(new SupportBean("D", 2));    // D gone
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"D"});
    
            epService.EPRuntime.SendEvent(new SupportBean("A", 2));    // A gone
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"A"});
    
            epService.EPRuntime.SendEvent(new SupportBean("C", 2));    // C gone
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"C"});
    
            epService.EPRuntime.SendEvent(new SupportBean("F", 1));
            Assert.IsTrue(handler.Contexts.IsEmpty());
    
            epService.EPRuntime.SendEvent(new SupportBean("G", 1));
            ExecRowRecogMaxStatesEngineWide3Instance.AssertContextEnginePool(epService, stmt, handler.GetAndResetContexts(), 3, ExecRowRecogMaxStatesEngineWide3Instance.GetExpectedCountMap("S1", 3));
    
            epService.EPAdministrator.Configuration.MatchRecognizeMaxStates = 4L;
    
            epService.EPRuntime.SendEvent(new SupportBean("G", 1));
            Assert.IsTrue(handler.Contexts.IsEmpty());
    
            epService.EPRuntime.SendEvent(new SupportBean("H", 1));
            ExecRowRecogMaxStatesEngineWide3Instance.AssertContextEnginePool(epService, stmt, handler.GetAndResetContexts(), 4, ExecRowRecogMaxStatesEngineWide3Instance.GetExpectedCountMap("S1", 4));
    
            epService.EPAdministrator.Configuration.MatchRecognizeMaxStates = null;
    
            epService.EPRuntime.SendEvent(new SupportBean("I", 1));
            Assert.IsTrue(handler.Contexts.IsEmpty());
        }
    }
} // end of namespace
