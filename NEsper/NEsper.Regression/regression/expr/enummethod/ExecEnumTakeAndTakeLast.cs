///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.bean.lambda;
using com.espertech.esper.supportregression.execution;

namespace com.espertech.esper.regression.expr.enummethod
{
    public class ExecEnumTakeAndTakeLast : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("Bean", typeof(SupportBean_ST0_Container));
            configuration.AddEventType("SupportCollection", typeof(SupportCollection));
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionTakeEvents(epService);
            RunAssertionTakeScalar(epService);
        }
    
        private void RunAssertionTakeEvents(EPServiceProvider epService) {
    
            string[] fields = "val0,val1,val2,val3,val4,val5".Split(',');
            string epl = "select " +
                    "Contained.take(2) as val0," +
                    "Contained.take(1) as val1," +
                    "Contained.take(0) as val2," +
                    "Contained.take(-1) as val3," +
                    "Contained.takeLast(2) as val4," +
                    "Contained.takeLast(1) as val5" +
                    " from Bean";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmt.EventType, fields, new Type[] {
                typeof(ICollection<SupportBean_ST0>),
                typeof(ICollection<SupportBean_ST0>),
                typeof(ICollection<SupportBean_ST0>),
                typeof(ICollection<SupportBean_ST0>),
                typeof(ICollection<SupportBean_ST0>),
                typeof(ICollection<SupportBean_ST0>)
            });
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,1", "E2,2", "E3,3"));
            LambdaAssertionUtil.AssertST0Id(listener, "val0", "E1,E2");
            LambdaAssertionUtil.AssertST0Id(listener, "val1", "E1");
            LambdaAssertionUtil.AssertST0Id(listener, "val2", "");
            LambdaAssertionUtil.AssertST0Id(listener, "val3", "");
            LambdaAssertionUtil.AssertST0Id(listener, "val4", "E2,E3");
            LambdaAssertionUtil.AssertST0Id(listener, "val5", "E3");
            listener.Reset();
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,1", "E2,2"));
            LambdaAssertionUtil.AssertST0Id(listener, "val0", "E1,E2");
            LambdaAssertionUtil.AssertST0Id(listener, "val1", "E1");
            LambdaAssertionUtil.AssertST0Id(listener, "val2", "");
            LambdaAssertionUtil.AssertST0Id(listener, "val3", "");
            LambdaAssertionUtil.AssertST0Id(listener, "val4", "E1,E2");
            LambdaAssertionUtil.AssertST0Id(listener, "val5", "E2");
            listener.Reset();
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,1"));
            LambdaAssertionUtil.AssertST0Id(listener, "val0", "E1");
            LambdaAssertionUtil.AssertST0Id(listener, "val1", "E1");
            LambdaAssertionUtil.AssertST0Id(listener, "val2", "");
            LambdaAssertionUtil.AssertST0Id(listener, "val3", "");
            LambdaAssertionUtil.AssertST0Id(listener, "val4", "E1");
            LambdaAssertionUtil.AssertST0Id(listener, "val5", "E1");
            listener.Reset();
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value());
            foreach (string field in fields) {
                LambdaAssertionUtil.AssertST0Id(listener, field, "");
            }
            listener.Reset();
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value(null));
            foreach (string field in fields) {
                LambdaAssertionUtil.AssertST0Id(listener, field, null);
            }
            listener.Reset();
    
            stmt.Dispose();
        }
    
        private void RunAssertionTakeScalar(EPServiceProvider epService) {
    
            string[] fields = "val0,val1,val2,val3".Split(',');
            string epl = "select " +
                    "Strvals.take(2) as val0," +
                    "Strvals.take(1) as val1," +
                    "Strvals.takeLast(2) as val2," +
                    "Strvals.takeLast(1) as val3" +
                    " from SupportCollection";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmt.EventType, fields, new Type[] {
                typeof(ICollection<string>),
                typeof(ICollection<string>),
                typeof(ICollection<string>),
                typeof(ICollection<string>)
            });
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1,E2,E3"));
            LambdaAssertionUtil.AssertValuesArrayScalar(listener, "val0", "E1", "E2");
            LambdaAssertionUtil.AssertValuesArrayScalar(listener, "val1", "E1");
            LambdaAssertionUtil.AssertValuesArrayScalar(listener, "val2", "E2", "E3");
            LambdaAssertionUtil.AssertValuesArrayScalar(listener, "val3", "E3");
            listener.Reset();
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1,E2"));
            LambdaAssertionUtil.AssertValuesArrayScalar(listener, "val0", "E1", "E2");
            LambdaAssertionUtil.AssertValuesArrayScalar(listener, "val1", "E1");
            LambdaAssertionUtil.AssertValuesArrayScalar(listener, "val2", "E1", "E2");
            LambdaAssertionUtil.AssertValuesArrayScalar(listener, "val3", "E2");
            listener.Reset();
    
            LambdaAssertionUtil.AssertSingleAndEmptySupportColl(epService, listener, fields);
    
            stmt.Dispose();
        }
    }
} // end of namespace
