///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
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
    public class ExecRowRecogMaxStatesEngineWide4Instance : RegressionExecution
    {
        private SupportConditionHandlerFactory.SupportConditionHandler handler;
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType<SupportBean_S0>();
            configuration.AddEventType(typeof(SupportBean_S1));
            configuration.EngineDefaults.ConditionHandling.AddClass(typeof(SupportConditionHandlerFactory));
            configuration.EngineDefaults.MatchRecognize.MaxStates = 4L;
            configuration.EngineDefaults.MatchRecognize.IsMaxStatesPreventStart = true;
            configuration.EngineDefaults.Logging.IsEnableExecutionDebug = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            handler = SupportConditionHandlerFactory.LastHandler;
            string[] fields = "c0".Split(',');
    
            string eplOne = "@Name('S1') select * from SupportBean(TheString = 'A') " +
                    "match_recognize (" +
                    "  partition by IntPrimitive " +
                    "  measures P2.IntPrimitive as c0" +
                    "  pattern (P1 P2) " +
                    "  define " +
                    "    P1 as P1.LongPrimitive = 1," +
                    "    P2 as P2.LongPrimitive = 2" +
                    ")";
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL(eplOne);
            var listenerOne = new SupportUpdateListener();
            stmtOne.Events += listenerOne.Update;
    
            string eplTwo = "@Name('S2') select * from SupportBean(TheString = 'B')#length(2) " +
                    "match_recognize (" +
                    "  partition by IntPrimitive " +
                    "  measures P2.IntPrimitive as c0" +
                    "  pattern (P1 P2) " +
                    "  define " +
                    "    P1 as P1.LongPrimitive = 1," +
                    "    P2 as P2.LongPrimitive = 2" +
                    ")";
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL(eplTwo);
            var listenerTwo = new SupportUpdateListener();
            stmtTwo.Events += listenerTwo.Update;
    
            epService.EPRuntime.SendEvent(ExecRowRecogMaxStatesEngineWide3Instance.MakeBean("A", 100, 1));
            epService.EPRuntime.SendEvent(ExecRowRecogMaxStatesEngineWide3Instance.MakeBean("A", 200, 1));
            epService.EPRuntime.SendEvent(ExecRowRecogMaxStatesEngineWide3Instance.MakeBean("B", 100, 1));
            epService.EPRuntime.SendEvent(ExecRowRecogMaxStatesEngineWide3Instance.MakeBean("B", 200, 1));
            epService.EPRuntime.SendEvent(ExecRowRecogMaxStatesEngineWide3Instance.MakeBean("B", 300, 1));
            epService.EPRuntime.SendEvent(ExecRowRecogMaxStatesEngineWide3Instance.MakeBean("B", 400, 1));
            EPAssertionUtil.EnumeratorToArray(stmtTwo.GetEnumerator());
            Assert.IsTrue(handler.Contexts.IsEmpty());
    
            // overflow
            epService.EPRuntime.SendEvent(ExecRowRecogMaxStatesEngineWide3Instance.MakeBean("A", 300, 1));
            ExecRowRecogMaxStatesEngineWide3Instance.AssertContextEnginePool(epService, stmtOne, handler.GetAndResetContexts(), 4, ExecRowRecogMaxStatesEngineWide3Instance.GetExpectedCountMap("S1", 2, "S2", 2));
    
            // terminate B
            epService.EPRuntime.SendEvent(ExecRowRecogMaxStatesEngineWide3Instance.MakeBean("B", 400, 2));
            EPAssertionUtil.AssertProps(listenerTwo.AssertOneGetNewAndReset(), fields, new object[]{400});
    
            // terminate one of A
            epService.EPRuntime.SendEvent(ExecRowRecogMaxStatesEngineWide3Instance.MakeBean("A", 100, 2));
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), fields, new object[]{100});
    
            // fill up A
            epService.EPRuntime.SendEvent(ExecRowRecogMaxStatesEngineWide3Instance.MakeBean("A", 300, 1));
            epService.EPRuntime.SendEvent(ExecRowRecogMaxStatesEngineWide3Instance.MakeBean("A", 400, 1));
            epService.EPRuntime.SendEvent(ExecRowRecogMaxStatesEngineWide3Instance.MakeBean("A", 500, 1));
            Assert.IsTrue(handler.Contexts.IsEmpty());
    
            // overflow
            epService.EPRuntime.SendEvent(ExecRowRecogMaxStatesEngineWide3Instance.MakeBean("B", 500, 1));
            ExecRowRecogMaxStatesEngineWide3Instance.AssertContextEnginePool(epService, stmtTwo, handler.GetAndResetContexts(), 4, ExecRowRecogMaxStatesEngineWide3Instance.GetExpectedCountMap("S1", 4, "S2", 0));
    
            // destroy statement-1 freeing up all "A"
            stmtOne.Dispose();
    
            // any number of B doesn't trigger overflow because of data window
            epService.EPRuntime.SendEvent(ExecRowRecogMaxStatesEngineWide3Instance.MakeBean("B", 600, 1));
            epService.EPRuntime.SendEvent(ExecRowRecogMaxStatesEngineWide3Instance.MakeBean("B", 700, 1));
            epService.EPRuntime.SendEvent(ExecRowRecogMaxStatesEngineWide3Instance.MakeBean("B", 800, 1));
            epService.EPRuntime.SendEvent(ExecRowRecogMaxStatesEngineWide3Instance.MakeBean("B", 900, 1));
            Assert.IsTrue(handler.Contexts.IsEmpty());
        }
    }
} // end of namespace
