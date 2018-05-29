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
    public class ExecEnumWhere : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("Bean", typeof(SupportBean_ST0_Container));
            configuration.AddEventType("SupportCollection", typeof(SupportCollection));
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionWhereEvents(epService);
            RunAssertionWhereString(epService);
        }
    
        private void RunAssertionWhereEvents(EPServiceProvider epService) {
    
            string epl = "select " +
                    "Contained.where(x => p00 = 9) as val0," +
                    "Contained.where((x, i) => x.p00 = 9 and i >= 1) as val1 from Bean";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmt.EventType, "val0,val1".Split(','), new Type[] {
                typeof(ICollection<SupportBean_ST0>), typeof(ICollection<SupportBean_ST0>)
            });
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,1", "E2,9", "E3,1"));
            LambdaAssertionUtil.AssertST0Id(listener, "val0", "E2");
            LambdaAssertionUtil.AssertST0Id(listener, "val1", "E2");
            listener.Reset();
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,9", "E2,1", "E3,1"));
            LambdaAssertionUtil.AssertST0Id(listener, "val0", "E1");
            LambdaAssertionUtil.AssertST0Id(listener, "val1", "");
            listener.Reset();
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,1", "E2,1", "E3,9"));
            LambdaAssertionUtil.AssertST0Id(listener, "val0", "E3");
            LambdaAssertionUtil.AssertST0Id(listener, "val1", "E3");
            listener.Reset();
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value(null));
            LambdaAssertionUtil.AssertST0Id(listener, "val0", null);
            LambdaAssertionUtil.AssertST0Id(listener, "val1", null);
            listener.Reset();
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value());
            LambdaAssertionUtil.AssertST0Id(listener, "val0", "");
            LambdaAssertionUtil.AssertST0Id(listener, "val1", "");
            listener.Reset();
    
            stmt.Dispose();
        }
    
        private void RunAssertionWhereString(EPServiceProvider epService) {
    
            string[] fields = "val0,val1".Split(',');
            string eplFragment = "select " +
                    "Strvals.where(x => x not like '%1%') as val0, " +
                    "Strvals.where((x, i) => x not like '%1%' and i > 1) as val1 " +
                    "from SupportCollection";
            EPStatement stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[] {
                typeof(ICollection<string>), typeof(ICollection<string>)
            });
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1,E2,E3"));
            LambdaAssertionUtil.AssertValuesArrayScalar(listener, "val0", "E2", "E3");
            LambdaAssertionUtil.AssertValuesArrayScalar(listener, "val1", "E3");
            listener.Reset();
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString("E4,E2,E1"));
            LambdaAssertionUtil.AssertValuesArrayScalar(listener, "val0", "E4", "E2");
            LambdaAssertionUtil.AssertValuesArrayScalar(listener, "val1", new string[0]);
            listener.Reset();
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString(""));
            LambdaAssertionUtil.AssertValuesArrayScalar(listener, "val0", new string[0]);
            LambdaAssertionUtil.AssertValuesArrayScalar(listener, "val1", new string[0]);
            listener.Reset();
    
            stmtFragment.Dispose();
    
            // test boolean
            eplFragment = "select " +
                    "Boolvals.where(x => x) as val0 " +
                    "from SupportCollection";
            stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, "val0".Split(','), new Type[] {
                typeof(ICollection<bool?>)
            });
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeBoolean("true,true,false"));
            LambdaAssertionUtil.AssertValuesArrayScalar(listener, "val0", true, true);
            listener.Reset();
    
            stmtFragment.Dispose();
        }
    }
} // end of namespace
