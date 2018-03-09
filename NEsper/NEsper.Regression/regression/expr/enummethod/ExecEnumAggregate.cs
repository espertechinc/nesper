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
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.bean.lambda;
using com.espertech.esper.supportregression.execution;

namespace com.espertech.esper.regression.expr.enummethod
{
    public class ExecEnumAggregate : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("Bean", typeof(SupportBean_ST0_Container));
            configuration.AddEventType("SupportCollection", typeof(SupportCollection));
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionAggregateEvents(epService);
            RunAssertionAggregateScalar(epService);
        }
    
        private void RunAssertionAggregateEvents(EPServiceProvider epService) {
    
            var fields = new string[]{"val0", "val1", "val2"};
            string eplFragment = "select " +
                    "Contained.aggregate(0, (result, item) => result + item.p00) as val0, " +
                    "Contained.aggregate('', (result, item) => result || ', ' || item.id) as val1, " +
                    "Contained.aggregate('', (result, item) => result || (case when result='' then '' else ',' end) || item.id) as val2 " +
                    " from Bean";
            EPStatement stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[]{typeof(int?), typeof(string), typeof(string)});
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,12", "E2,11", "E2,2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                    new object[]{25, ", E1, E2, E2", "E1,E2,E2"});
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value(null));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                    new object[]{null, null, null});
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value(new string[0]));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                    new object[]{0, "", ""});
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,12"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                    new object[]{12, ", E1", "E1"});
    
            stmtFragment.Dispose();
        }
    
        private void RunAssertionAggregateScalar(EPServiceProvider epService) {
    
            string[] fields = "val0".Split(',');
            string eplFragment = "select " +
                    "Strvals.aggregate('', (result, item) => result || '+' || item) as val0 " +
                    "from SupportCollection";
            EPStatement stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[]{typeof(string)});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1,E2,E3"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"+E1+E2+E3"});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"+E1"});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString(""));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{""});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString(null));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null});
    
            stmtFragment.Dispose();
        }
    }
} // end of namespace
