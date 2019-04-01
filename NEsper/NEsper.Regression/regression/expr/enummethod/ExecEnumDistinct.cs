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
    public class ExecEnumDistinct : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("Bean", typeof(SupportBean_ST0_Container));
            configuration.AddEventType("SupportCollection", typeof(SupportCollection));
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionDistinctEvents(epService);
            RunAssertionDistinctScalar(epService);
        }
    
        private void RunAssertionDistinctEvents(EPServiceProvider epService) {
    
            string[] fields = "val0".Split(',');
            string eplFragment = "select " +
                    "Contained.distinctOf(x => p00) as val0 " +
                    " from Bean";
            EPStatement stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[] {
                typeof(ICollection<SupportBean_ST0>)
            });
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,1", "E2,2", "E3,1"));
            LambdaAssertionUtil.AssertST0Id(listener, "val0", "E1,E2");
            listener.Reset();
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E3,1", "E2,2", "E4,1", "E1,2"));
            LambdaAssertionUtil.AssertST0Id(listener, "val0", "E3,E2");
            listener.Reset();
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value(null));
            foreach (string field in fields) {
                LambdaAssertionUtil.AssertST0Id(listener, field, null);
            }
            listener.Reset();
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value());
            foreach (string field in fields) {
                LambdaAssertionUtil.AssertST0Id(listener, field, "");
            }
            listener.Reset();
    
            stmtFragment.Dispose();
        }
    
        private void RunAssertionDistinctScalar(EPServiceProvider epService) {
    
            epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("extractNum", typeof(ExecEnumMinMax.MyService), "ExtractNum");
    
            string[] fields = "val0,val1".Split(',');
            string eplFragment = "select " +
                    "Strvals.distinctOf() as val0, " +
                    "Strvals.distinctOf(v => extractNum(v)) as val1 " +
                    "from SupportCollection";
            EPStatement stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[] {
                typeof(ICollection<string>), typeof(ICollection<string>)
            });
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString("E2,E1,E2,E2"));
            LambdaAssertionUtil.AssertValuesArrayScalar(listener, "val0", "E2", "E1");
            LambdaAssertionUtil.AssertValuesArrayScalar(listener, "val1", "E2", "E1");
            listener.Reset();
    
            LambdaAssertionUtil.AssertSingleAndEmptySupportColl(epService, listener, fields);
            stmtFragment.Dispose();
        }
    }
} // end of namespace
