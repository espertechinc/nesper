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
    public class ExecEnumReverse : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("Bean", typeof(SupportBean_ST0_Container));
            configuration.AddEventType("SupportCollection", typeof(SupportCollection));
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionReverseEvents(epService);
            RunAssertionReverseScalar(epService);
        }
    
        private void RunAssertionReverseEvents(EPServiceProvider epService) {
    
            string epl = "select Contained.Reverse() as val from Bean";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
            LambdaAssertionUtil.AssertTypes(stmt.EventType, "val".Split(','), new Type[]{typeof(Collection)});
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,1", "E2,9", "E3,1"));
            LambdaAssertionUtil.AssertST0Id(listener, "val", "E3,E2,E1");
            listener.Reset();
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E2,9", "E1,1"));
            LambdaAssertionUtil.AssertST0Id(listener, "val", "E1,E2");
            listener.Reset();
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,1"));
            LambdaAssertionUtil.AssertST0Id(listener, "val", "E1");
            listener.Reset();
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value(null));
            LambdaAssertionUtil.AssertST0Id(listener, "val", null);
            listener.Reset();
    
            epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value());
            LambdaAssertionUtil.AssertST0Id(listener, "val", "");
            listener.Reset();
    
            stmt.Dispose();
        }
    
        private void RunAssertionReverseScalar(EPServiceProvider epService) {
    
            string[] fields = "val0".Split(',');
            string eplFragment = "select " +
                    "strvals.Reverse() as val0 " +
                    "from SupportCollection";
            EPStatement stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.AddListener(listener);
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[]{typeof(Collection), typeof(Collection)});
    
            epService.EPRuntime.SendEvent(SupportCollection.MakeString("E2,E1,E5,E4"));
            LambdaAssertionUtil.AssertValuesArrayScalar(listener, "val0", "E4", "E5", "E1", "E2");
            listener.Reset();
    
            LambdaAssertionUtil.AssertSingleAndEmptySupportColl(epService, listener, fields);
    
            stmtFragment.Dispose();
        }
    }
} // end of namespace
