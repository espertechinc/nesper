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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.bean.lambda;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.expr.enummethod
{
    public class ExecEnumTakeWhileAndWhileLast : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("Bean", typeof(SupportBean_ST0_Container));
            configuration.AddEventType("SupportCollection", typeof(SupportCollection));
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionTakeWhileEvents(epService);
            RunAssertionTakeWhileScalar(epService);
        }
    
        private void RunAssertionTakeWhileEvents(EPServiceProvider epService) {
    
            string[] fields = "val0,val1,val2,val3".Split(',');
            string epl = "select " +
                    "contained.TakeWhile(x => x.p00 > 0) as val0," +
                    "contained.TakeWhile( (x, i) => x.p00 > 0 and i<2) as val1," +
                    "contained.TakeWhileLast(x => x.p00 > 0) as val2," +
                    "contained.TakeWhileLast( (x, i) => x.p00 > 0 and i<2) as val3" +
                    " from Bean";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmt.EventType, fields, new Type[]{typeof(ICollection<object>), typeof(ICollection<object>), typeof(ICollection<object>), typeof(ICollection<object>)});
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,1", "E2,2", "E3,3"));
            LambdaAssertionUtil.AssertST0Id(listener, "val0", "E1,E2,E3");
            LambdaAssertionUtil.AssertST0Id(listener, "val1", "E1,E2");
            LambdaAssertionUtil.AssertST0Id(listener, "val2", "E1,E2,E3");
            LambdaAssertionUtil.AssertST0Id(listener, "val3", "E2,E3");
            listener.Reset();
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,0", "E2,2", "E3,3"));
            LambdaAssertionUtil.AssertST0Id(listener, "val0", "");
            LambdaAssertionUtil.AssertST0Id(listener, "val1", "");
            LambdaAssertionUtil.AssertST0Id(listener, "val2", "E2,E3");
            LambdaAssertionUtil.AssertST0Id(listener, "val3", "E2,E3");
            listener.Reset();
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,1", "E2,0", "E3,3"));
            LambdaAssertionUtil.AssertST0Id(listener, "val0", "E1");
            LambdaAssertionUtil.AssertST0Id(listener, "val1", "E1");
            LambdaAssertionUtil.AssertST0Id(listener, "val2", "E3");
            LambdaAssertionUtil.AssertST0Id(listener, "val3", "E3");
            listener.Reset();
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,1", "E2,1", "E3,0"));
            LambdaAssertionUtil.AssertST0Id(listener, "val0", "E1,E2");
            LambdaAssertionUtil.AssertST0Id(listener, "val1", "E1,E2");
            LambdaAssertionUtil.AssertST0Id(listener, "val2", "");
            LambdaAssertionUtil.AssertST0Id(listener, "val3", "");
            listener.Reset();
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,1"));
            LambdaAssertionUtil.AssertST0Id(listener, "val0", "E1");
            LambdaAssertionUtil.AssertST0Id(listener, "val1", "E1");
            LambdaAssertionUtil.AssertST0Id(listener, "val2", "E1");
            LambdaAssertionUtil.AssertST0Id(listener, "val3", "E1");
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
    
        private void RunAssertionTakeWhileScalar(EPServiceProvider epService) {
    
            string[] fields = "val0,val1,val2,val3".Split(',');
            string epl = "select " +
                    "strvals.TakeWhile(x => x != 'E1') as val0," +
                    "strvals.TakeWhile( (x, i) => x != 'E1' and i<2) as val1," +
                    "strvals.TakeWhileLast(x => x != 'E1') as val2," +
                    "strvals.TakeWhileLast( (x, i) => x != 'E1' and i<2) as val3" +
                    " from SupportCollection";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmt.EventType, fields, new Type[]{typeof(ICollection<object>), typeof(ICollection<object>), typeof(ICollection<object>), typeof(ICollection<object>)});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1,E2,E3,E4"));
            LambdaAssertionUtil.AssertValuesArrayScalar(listener, "val0");
            LambdaAssertionUtil.AssertValuesArrayScalar(listener, "val1");
            LambdaAssertionUtil.AssertValuesArrayScalar(listener, "val2", "E2", "E3", "E4");
            LambdaAssertionUtil.AssertValuesArrayScalar(listener, "val3", "E3", "E4");
            listener.Reset();
    
            stmt.Dispose();
        }
    }
} // end of namespace
